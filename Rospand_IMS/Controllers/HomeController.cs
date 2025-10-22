using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;
using Rospand_IMS.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rospand_IMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(int? month, int? year)
        {
            // Get selected month/year or use current
            var now = DateTime.Now;
            var selectedMonth = month ?? now.Month;
            var selectedYear = year ?? now.Year;

            var selectedDate = new DateTime(selectedYear, selectedMonth, 1);
            var startOfMonth = selectedDate;
            var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

            // Recent inventory transactions (last 10)
            var recentTransactions = await _context.InventoryTransactions
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .OrderByDescending(t => t.TransactionDate)
                .Take(10)
                .Select(t => new InventoryTransactionViewModel
                {
                    Id = t.Id,
                    TransactionType = t.TransactionType,
                    TransactionDate = t.TransactionDate,
                    ReferenceNumber = t.ReferenceNumber,
                    ProductName = t.Product.Name,
                    Quantity = t.Quantity,
                    Value = t.Quantity * (t.TransactionType == InventoryTransactionType.Purchase ?
                        t.Product.PurchasePrice ?? 0 : t.Product.SalesPrice ?? 0),
                    ProductId = t.ProductId,
                    WarehouseId = t.WarehouseId
                })
                .ToListAsync();

            var model = new DashboardViewModel
            {
                Period = selectedDate.ToString("MMMM yyyy"),
                PurchaseOrderCount = await _context.PurchaseOrders
                    .Where(po => po.OrderDate >= startOfMonth && po.OrderDate <= endOfMonth)
                    .CountAsync(),
                SalesOrderCount = await _context.SalesOrders
                    .Where(so => so.OrderDate >= startOfMonth && so.OrderDate <= endOfMonth)
                    .CountAsync(),
                OutwardEntryCount = await _context.OutwardEntries
                    .Where(oe => oe.OutwardDate >= startOfMonth && oe.OutwardDate <= endOfMonth)
                    .CountAsync(),
                TotalStock = await _context.Inventories.SumAsync(i => i.QuantityOnHand - i.QuantityReserved),
                PurchaseTotal = await _context.PurchaseOrders
                    .Where(po => po.OrderDate >= startOfMonth && po.OrderDate <= endOfMonth)
                    .SumAsync(po => po.TotalAmount),
                SalesTotal = await _context.SalesOrders
                    .Where(so => so.OrderDate >= startOfMonth && so.OrderDate <= endOfMonth)
                    .SumAsync(so => so.TotalAmount),
                TrendLabels = GetLast6MonthsLabels(),
                PurchaseTrends = await GetPurchaseTrends(), // Keep these as last 6 months
                SalesTrends = await GetSalesTrends(),
                RecentTransactions = recentTransactions
            };

            return View(model);
        }


        // Add this new action for transaction details with pagination
        [HttpGet]
        public async Task<IActionResult> TransactionDetails(
         int? id,
         string referenceNumber,
         InventoryTransactionType? transactionType,
         DateTime? fromDate,
         DateTime? toDate,
         int? productId,
         int? warehouseId,
         int pageNumber = 1)
        {
            const int pageSize = 10;

            // Handle details view request
            if (id.HasValue || !string.IsNullOrEmpty(referenceNumber))
            {
                var transaction = await _context.InventoryTransactions
                    .Include(it => it.Product)
                    .Include(it => it.Warehouse)
                    .FirstOrDefaultAsync(it =>
                        (id.HasValue && it.Id == id) ||
                        (!string.IsNullOrEmpty(referenceNumber) && it.ReferenceNumber == referenceNumber));

                if (transaction == null)
                {
                    return NotFound();
                }

                // Fetch related transaction for transfers
                if (transaction.TransactionType == InventoryTransactionType.TransferIn ||
                    transaction.TransactionType == InventoryTransactionType.TransferOut)
                {
                    var relatedTransaction = await _context.InventoryTransactions
                        .Include(t => t.Warehouse)
                        .FirstOrDefaultAsync(t =>
                            t.ReferenceNumber == transaction.ReferenceNumber &&
                            t.Id != transaction.Id);

                    ViewBag.RelatedTransaction = relatedTransaction;
                }

                return View("TransactionDetail", transaction);
            }

            // Handle list view
            IQueryable<InventoryTransaction> query = _context.InventoryTransactions
                .Include(it => it.Product)
                .Include(it => it.Warehouse)
                .OrderByDescending(it => it.TransactionDate);

            if (transactionType.HasValue)
                query = query.Where(it => it.TransactionType == transactionType.Value);

            if (fromDate.HasValue)
                query = query.Where(it => it.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(it => it.TransactionDate <= toDate.Value.AddDays(1));

            if (productId.HasValue)
                query = query.Where(it => it.ProductId == productId.Value);

            if (warehouseId.HasValue)
                query = query.Where(it => it.WarehouseId == warehouseId.Value);

            var transactions = await PaginatedList<InventoryTransaction>.CreateAsync(query, pageNumber, pageSize);

            // Store filter values
            ViewBag.TransactionType = transactionType;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.ProductId = productId;
            ViewBag.WarehouseId = warehouseId;

            ViewBag.Products = await _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToListAsync();

            ViewBag.Warehouses = await _context.Warehouses
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();

            return View(transactions);
        }

        // Add this new action for transaction details with pagination

        public IActionResult Privacy()
        {
            return View();
        }

        private List<string> GetLast6MonthsLabels()
        {
            var labels = new List<string>();
            var now = DateTime.Now;

            for (int i = 5; i >= 0; i--)
            {
                var date = now.AddMonths(-i);
                labels.Add(date.ToString("MMM yyyy"));
            }

            return labels;
        }

        private async Task<List<decimal>> GetPurchaseTrends()
        {
            var trends = new List<decimal>();
            var now = DateTime.Now;

            for (int i = 5; i >= 0; i--)
            {
                var startDate = new DateTime(now.AddMonths(-i).Year, now.AddMonths(-i).Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var total = await _context.PurchaseOrders
                    .Where(po => po.OrderDate >= startDate && po.OrderDate <= endDate)
                    .SumAsync(po => po.TotalAmount);

                trends.Add(total);
            }

            return trends;
        }

        private async Task<List<decimal>> GetSalesTrends()
        {
            var trends = new List<decimal>();
            var now = DateTime.Now;

            for (int i = 5; i >= 0; i--)
            {
                var startDate = new DateTime(now.AddMonths(-i).Year, now.AddMonths(-i).Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var total = await _context.SalesOrders
                    .Where(so => so.OrderDate >= startDate && so.OrderDate <= endDate)
                    .SumAsync(so => so.TotalAmount);

                trends.Add(total);
            }

            return trends;
        }
    }

    public class DashboardViewModel
    {
        public string Period { get; set; }
        public int PurchaseOrderCount { get; set; }
        public int SalesOrderCount { get; set; }
        public int OutwardEntryCount { get; set; }
        public int TotalStock { get; set; }
        public decimal PurchaseTotal { get; set; }
        public decimal SalesTotal { get; set; }
        public List<string> TrendLabels { get; set; }
        public List<decimal> PurchaseTrends { get; set; }
        public List<decimal> SalesTrends { get; set; }
        public List<InventoryTransactionViewModel> RecentTransactions { get; set; }
    }

    public class InventoryTransactionViewModel
    {
        public int Id { get; set; }
        public InventoryTransactionType TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ReferenceNumber { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Value { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
    }
}