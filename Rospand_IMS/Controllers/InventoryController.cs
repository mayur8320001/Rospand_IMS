
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rospand_IMS.Controllers
{

    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Inventory (Stock Levels)
        public async Task<IActionResult> Index()
        {
            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .ThenInclude(p => p.Category)
                .Include(i => i.Warehouse)
                .OrderBy(i => i.Product.Name)
                .ThenBy(i => i.Warehouse.Name)
                .ToListAsync();

            // Calculate existing totals
            ViewBag.TotalInventoryValue = inventory.Sum(i => (i.Product.PurchasePrice ?? 0) * i.QuantityOnHand);
            ViewBag.TotalPotentialSalesValue = inventory.Sum(i => (i.Product.SalesPrice ?? 0) * i.QuantityOnHand);
            ViewBag.TotalQuantityAvailable = inventory.Sum(i => i.QuantityOnHand - i.QuantityReserved);

            // Calculate total sales value from sales orders
            var salesOrderItems = await _context.SalesOrderItems
                .Include(soi => soi.SalesOrder)
                .Include(soi => soi.Product)
                .Where(soi => soi.SalesOrder.Status == SalesOrderStatus.Confirmed ||
                              soi.SalesOrder.Status == SalesOrderStatus.PartiallyShipped ||
                              soi.SalesOrder.Status == SalesOrderStatus.Shipped ||
                              soi.SalesOrder.Status == SalesOrderStatus.Delivered)
                .GroupBy(soi => soi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSalesValue = g.Sum(soi => soi.LineTotal)
                })
                .ToListAsync();

            // Create a dictionary for quick lookup of sales value by product
            var salesValueByProduct = salesOrderItems.ToDictionary(s => s.ProductId, s => s.TotalSalesValue);

            // Calculate total sales value across all products
            ViewBag.TotalSalesValue = salesOrderItems.Sum(s => s.TotalSalesValue);

            // Add sales value to each inventory item for the view
            ViewBag.SalesValueByProduct = salesValueByProduct;

            return View(inventory);
        }

        // GET: Inventory/Details/5
        public async Task<IActionResult> Details(int productId, int warehouseId)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);

            if (inventory == null)
            {
                return NotFound();
            }

            // Get recent transactions for this product/warehouse
            var transactions = await _context.InventoryTransactions
                .Where(t => t.ProductId == productId && t.WarehouseId == warehouseId)
                .OrderByDescending(t => t.TransactionDate)
                .Take(20)
                .ToListAsync();

            ViewBag.Transactions = transactions;

            return View(inventory);
        }

        // GET: Inventory/Transactions
        public async Task<IActionResult> Transactions(DateTime? fromDate, DateTime? toDate, InventoryTransactionType? transactionType)
        {
            var query = _context.InventoryTransactions
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .AsQueryable();

            // Apply filters
            if (fromDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= toDate.Value);
            }

            if (transactionType.HasValue)
            {
                query = query.Where(t => t.TransactionType == transactionType.Value);
            }

            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            return View(transactions);
        }

       

        [HttpGet]
        public async Task<IActionResult> LowStock(int? threshold)
        {
            int lowStockThreshold = threshold ?? 5; // Allow dynamic threshold
            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .Where(i => (i.QuantityOnHand - i.QuantityReserved) < lowStockThreshold)
                .OrderBy(i => i.QuantityOnHand - i.QuantityReserved)
                .ToListAsync();

            ViewBag.Threshold = lowStockThreshold;
            return View(inventory);
        }

                 

        [HttpGet]
        public async Task<IActionResult> Adjust()
        {
            var model = new InventoryAdjustmentViewModel();
            ViewBag.Products = await _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToListAsync();
            ViewBag.Warehouses = await _context.Warehouses
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();

            return View(model);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(InventoryAdjustmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == model.ProductId && i.WarehouseId == model.WarehouseId);

                if (inventory == null && model.AdjustmentType == -1)
                {
                    ModelState.AddModelError("", "Cannot decrease inventory for a product that doesn't exist in this warehouse.");
                    return View(model);
                }

                // Create or update inventory
                if (inventory == null)
                {
                    inventory = new Inventory
                    {
                        ProductId = model.ProductId,
                        WarehouseId = model.WarehouseId,
                        QuantityOnHand = model.Quantity,
                        QuantityReserved = 0
                    };
                    _context.Add(inventory);
                }
                else
                {
                    if (model.AdjustmentType == 1)
                    {
                        inventory.QuantityOnHand += model.Quantity;
                    }
                    else
                    {
                        if (inventory.QuantityAvailable < model.Quantity)
                        {
                            ModelState.AddModelError("Quantity", $"Cannot adjust by {model.Quantity}. Only {inventory.QuantityAvailable} available.");
                            return View(model);
                        }
                        inventory.QuantityOnHand -= model.Quantity;
                    }
                    _context.Update(inventory);
                }

                // Create transaction
                var transaction = new InventoryTransaction
                {
                    ProductId = model.ProductId,
                    WarehouseId = model.WarehouseId,
                    TransactionType = model.AdjustmentType == 1 ?
                        InventoryTransactionType.Adjustment : InventoryTransactionType.Damage,
                    Quantity = model.AdjustmentType == 1 ? model.Quantity : -model.Quantity,
                    TransactionDate = DateTime.Now,
                    Notes = $"{model.Reason} adjustment. {model.Notes}"
                };
                _context.Add(transaction);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Inventory adjustment processed successfully.";
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            ViewBag.Products = await _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToListAsync();
            ViewBag.Warehouses = await _context.Warehouses
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Transfer()
        {
            var model = new StockTransferViewModel();
            ViewBag.Products = await _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToListAsync();
            ViewBag.Warehouses = await _context.Warehouses
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(StockTransferViewModel model)
        {
            if (model.FromWarehouseId == model.ToWarehouseId)
            {
                ModelState.AddModelError("ToWarehouseId", "Source and destination warehouses cannot be the same.");
            }

            if (ModelState.IsValid)
            {
                // Check source inventory
                var fromInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == model.ProductId && i.WarehouseId == model.FromWarehouseId);

                if (fromInventory == null || fromInventory.QuantityAvailable < model.Quantity)
                {
                    ModelState.AddModelError("Quantity", "Not enough stock available in source warehouse.");
                    return View(model);
                }

                // Get or create destination inventory
                var toInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == model.ProductId && i.WarehouseId == model.ToWarehouseId);

                if (toInventory == null)
                {
                    toInventory = new Inventory
                    {
                        ProductId = model.ProductId,
                        WarehouseId = model.ToWarehouseId,
                        QuantityOnHand = model.Quantity,
                        QuantityReserved = 0
                    };
                    _context.Add(toInventory);
                }
                else
                {
                    toInventory.QuantityOnHand += model.Quantity;
                    _context.Update(toInventory);
                }

                // Update source inventory
                fromInventory.QuantityOnHand -= model.Quantity;
                _context.Update(fromInventory);

                // Create transactions
                var outTransaction = new InventoryTransaction
                {
                    ProductId = model.ProductId,
                    WarehouseId = model.FromWarehouseId,
                    TransactionType = InventoryTransactionType.TransferOut,
                    Quantity = -model.Quantity,
                    TransactionDate = DateTime.Now,
                    ReferenceNumber = model.ReferenceNumber,
                    Notes = $"Transfer to {toInventory.Warehouse.Name}. {model.Notes}"
                };

                var inTransaction = new InventoryTransaction
                {
                    ProductId = model.ProductId,
                    WarehouseId = model.ToWarehouseId,
                    TransactionType = InventoryTransactionType.TransferIn,
                    Quantity = model.Quantity,
                    TransactionDate = DateTime.Now,
                    ReferenceNumber = model.ReferenceNumber,
                    Notes = $"Transfer from {fromInventory.Warehouse.Name}. {model.Notes}"
                };

                _context.Add(outTransaction);
                _context.Add(inTransaction);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Stock transfer completed successfully.";
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            ViewBag.Products = await _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToListAsync();
            ViewBag.Warehouses = await _context.Warehouses
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryLevel(int productId, int warehouseId)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId);

            return Json(new
            {
                quantityAvailable = inventory?.QuantityAvailable ?? 0
            });
        }

     
    }
}
