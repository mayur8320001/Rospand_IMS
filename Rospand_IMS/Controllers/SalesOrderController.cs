using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;

namespace Rospand_IMS.Controllers
{
    public class SalesOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Generate unique SO number
        private string GenerateSONumber()
        {
            var prefix = "SO";
            var today = DateTime.Today.ToString("yyMMdd");
            var lastNumber = _context.SalesOrders
                .Where(so => so.SONumber.StartsWith(prefix + today))
                .OrderByDescending(so => so.SONumber)
                .Select(so => so.SONumber)
                .FirstOrDefault();

            var nextNum = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var numPart = lastNumber.Substring(prefix.Length + 6);
                if (int.TryParse(numPart, out var lastNum))
                {
                    nextNum = lastNum + 1;
                }
            }

            return $"{prefix}{today}{nextNum:D4}";
        }

        // Generate unique outward number
        private string GenerateOutwardNumber()
        {
            var prefix = "OUT";
            var today = DateTime.Today.ToString("yyMMdd");
            var lastNumber = _context.OutwardEntries
                .Where(oe => oe.OutwardNumber.StartsWith(prefix + today))
                .OrderByDescending(oe => oe.OutwardNumber)
                .Select(oe => oe.OutwardNumber)
                .FirstOrDefault();

            var nextNum = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var numPart = lastNumber.Substring(prefix.Length + 6);
                if (int.TryParse(numPart, out var lastNum))
                {
                    nextNum = lastNum + 1;
                }
            }

            return $"{prefix}{today}{nextNum:D4}";
        }

        // GET: SalesOrder
        public async Task<IActionResult> Index(SalesOrderStatus? status, string searchString)
        {
            var query = _context.SalesOrders
                .Include(so => so.Customer)
                .Include(so => so.Currency)
                .OrderByDescending(so => so.OrderDate)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(so => so.Status == status.Value);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(so => so.SONumber.Contains(searchString) ||
                                         so.Customer.CustomerDisplayName.Contains(searchString));
            }

            ViewBag.StatusFilter = status;
            ViewBag.SearchString = searchString;
            return View(await query.ToListAsync());
        }

        // GET: SalesOrder/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new SalesOrderCreateViewModel
            {
                OrderDate = DateTime.Today,
                ExpectedDeliveryDate = DateTime.Today.AddDays(7),
                SONumber = GenerateSONumber(),
                Customers = await _context.Customers
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CustomerDisplayName)
                    .ToListAsync(),
                Currencies = await _context.Currencies.ToListAsync(),
                AvailableProducts = await _context.Inventories
                    .Include(i => i.Product)
                    .ThenInclude(p => p.Category)
                    .Select(i => new InventoryViewModel
                    {
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        ProductSKU = i.Product.SKU,
                        CategoryName = i.Product.Category != null ? i.Product.Category.Name : "No Category",
                        SalesPrice = i.Product.SalesPrice ?? 0,
                        QuantityAvailable = i.QuantityOnHand - i.QuantityReserved,
                        WarehouseId = i.WarehouseId,
                        WarehouseName = i.Warehouse.Name
                    })
                    .Where(i => i.QuantityAvailable > 0)
                    .OrderBy(i => i.ProductName)
                    .ToListAsync(),
                Items = new List<SalesOrderItemViewModel>()
            };

            return View(viewModel);
        }

        // POST: SalesOrder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalesOrderCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewModelDropdowns(viewModel);
                return View(viewModel);
            }

            // Validate inventory and product availability
            foreach (var item in viewModel.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    ModelState.AddModelError("", $"Product {item.ProductName} (ID: {item.ProductId}) no longer exists");
                    await PopulateViewModelDropdowns(viewModel);
                    return View(viewModel);
                }

                var inventory = await _context.Inventories
                    .Where(i => i.ProductId == item.ProductId)
                    .SumAsync(i => i.QuantityOnHand - i.QuantityReserved);

                if (inventory < item.Quantity)
                {
                    ModelState.AddModelError($"Items[{viewModel.Items.IndexOf(item)}].Quantity",
                        $"Insufficient stock for {item.ProductName}. Available: {inventory}");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateViewModelDropdowns(viewModel);
                return View(viewModel);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var salesOrder = new SalesOrder
                {
                    SONumber = viewModel.SONumber,
                    OrderDate = viewModel.OrderDate,
                    ExpectedDeliveryDate = viewModel.ExpectedDeliveryDate,
                    CustomerId = viewModel.CustomerId,
                    Status = SalesOrderStatus.Draft,
                    CurrencyId = viewModel.CurrencyId,
                    Notes = viewModel.Notes,
                    CreatedDate = DateTime.UtcNow
                };

                foreach (var item in viewModel.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    salesOrder.Items.Add(new SalesOrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice > 0 ? item.UnitPrice : product.SalesPrice ?? 0,
                        DiscountPercent = item.DiscountPercent,
                        TaxRate = item.TaxRate,
                        Notes = item.Notes,
                        LineTotal = CalculateLineTotal(item.Quantity, item.UnitPrice > 0 ? item.UnitPrice : product.SalesPrice ?? 0, item.DiscountPercent, item.TaxRate)
                    });
                }

                CalculateOrderTotals(salesOrder);
                _context.SalesOrders.Add(salesOrder);
                await _context.SaveChangesAsync();

                // Reserve inventory and create transaction
                foreach (var item in salesOrder.Items)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.QuantityOnHand - i.QuantityReserved >= item.Quantity);

                    if (inventory != null)
                    {
                        inventory.QuantityReserved += item.Quantity;
                        inventory.LastUpdated = DateTime.UtcNow;
                        _context.Update(inventory);

                        var transactionEntry = new InventoryTransaction
                        {
                            ProductId = item.ProductId,
                            WarehouseId = inventory.WarehouseId,
                            TransactionType = InventoryTransactionType.Reservation,
                            Quantity = item.Quantity,
                            TransactionDate = DateTime.UtcNow,
                            SalesOrderId = salesOrder.Id,
                            ReferenceNumber = salesOrder.SONumber,
                            Notes = $"Reserved for sales order {salesOrder.SONumber}",
                            PreviousQuantity = inventory.QuantityOnHand,
                            NewQuantity = inventory.QuantityOnHand
                        };
                        _context.InventoryTransactions.Add(transactionEntry);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Details), new { id = salesOrder.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Error creating sales order: {ex.Message}");
                await PopulateViewModelDropdowns(viewModel);
                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var salesOrder = await _context.SalesOrders
                .Include(so => so.Items)
                .ThenInclude(i => i.Product)
                .Include(so => so.Customer)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (salesOrder == null)
            {
                return NotFound();
            }

            if (salesOrder.Status != SalesOrderStatus.Draft)
            {
                return BadRequest("Sales order must be in Draft status to confirm.");
            }

            var outOfStockItems = new List<SalesOrderItem>();
            var insufficientStockItems = new List<(SalesOrderItem item, int available)>();

            foreach (var item in salesOrder.Items)
            {
                var inventory = await _context.Inventories
                    .Where(i => i.ProductId == item.ProductId)
                    .SumAsync(i => i.QuantityOnHand - i.QuantityReserved);

                if (inventory == 0)
                {
                    outOfStockItems.Add(item);
                }
                else if (inventory < item.Quantity)
                {
                    insufficientStockItems.Add((item, inventory));
                }
            }

            if (outOfStockItems.Any() || insufficientStockItems.Any())
            {
                var viewModel = new ConfirmSalesOrderViewModel
                {
                    SalesOrderId = salesOrder.Id,
                    SONumber = salesOrder.SONumber,
                    CustomerName = salesOrder.Customer.CustomerDisplayName,
                    OutOfStockItems = outOfStockItems.Select(i => new StockIssueItemViewModel
                    {
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        Available = 0
                    }).ToList(),
                    InsufficientStockItems = insufficientStockItems.Select(i => new StockIssueItemViewModel
                    {
                        ProductId = i.item.ProductId,
                        ProductName = i.item.Product.Name,
                        Quantity = i.item.Quantity,
                        Available = i.available
                    }).ToList()
                };

                return View("StockIssues", viewModel);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in salesOrder.Items)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.QuantityOnHand - i.QuantityReserved >= item.Quantity);

                    if (inventory != null)
                    {
                        inventory.QuantityReserved += item.Quantity;
                        inventory.LastUpdated = DateTime.UtcNow;
                        _context.Update(inventory);

                        var transactionEntry = new InventoryTransaction
                        {
                            ProductId = item.ProductId,
                            WarehouseId = inventory.WarehouseId,
                            TransactionType = InventoryTransactionType.Reservation,
                            Quantity = item.Quantity,
                            TransactionDate = DateTime.UtcNow,
                            SalesOrderId = salesOrder.Id,
                            ReferenceNumber = salesOrder.SONumber,
                            Notes = $"Confirmed reservation for sales order {salesOrder.SONumber}"
                        };
                        _context.InventoryTransactions.Add(transactionEntry);
                    }
                }

                salesOrder.Status = SalesOrderStatus.Confirmed;
                salesOrder.ModifiedDate = DateTime.UtcNow;
                _context.Update(salesOrder);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Details), new { id = salesOrder.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error confirming sales order: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateOutwardEntry(int salesOrderId)
        {
            var salesOrder = await _context.SalesOrders
                .Include(so => so.Items)
                .ThenInclude(i => i.Product)
                .Include(so => so.Customer)
                .Include(so => so.OutwardEntries)
                    .ThenInclude(oe => oe.Items)
                .FirstOrDefaultAsync(so => so.Id == salesOrderId);

            if (salesOrder == null)
            {
                return NotFound("Sales order not found.");
            }

            if (salesOrder.Status != SalesOrderStatus.Confirmed && salesOrder.Status != SalesOrderStatus.PartiallyShipped)
            {
                return BadRequest("Sales order must be in Confirmed or Partially Shipped status to create an outward entry.");
            }

            var viewModel = new OutwardEntryCreateViewModel
            {
                SalesOrderId = salesOrderId,
                SONumber = salesOrder.SONumber ?? throw new Exception("Sales order number is missing."),
                CustomerDisplayName = salesOrder.Customer?.CustomerDisplayName ?? throw new Exception("Customer name is missing."),
                OutwardNumber = GenerateOutwardNumber(),
                OutwardDate = DateTime.Today,
                WarehouseId = 0, // Default to no selection
                Warehouses = new SelectList(await _context.Warehouses
                    .Where(w => w.IsActive)
                    .OrderBy(w => w.Name)
                    .ToListAsync(), "Id", "Name"),
                Items = new List<OutwardEntryItemViewModel>()
            };

            var dispatchedQuantities = salesOrder.OutwardEntries
                .SelectMany(oe => oe.Items)
                .GroupBy(oi => oi.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(oi => oi.Quantity));

            var productIds = salesOrder.Items.Select(i => i.ProductId).ToList();
            var inventories = await _context.Inventories
                .Where(i => productIds.Contains(i.ProductId))
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    QuantityAvailable = g.Sum(i => i.QuantityOnHand - i.QuantityReserved)
                })
                .ToListAsync();

            foreach (var item in salesOrder.Items)
            {
                var alreadyDispatched = dispatchedQuantities.ContainsKey(item.ProductId)
                    ? dispatchedQuantities[item.ProductId]
                    : 0;

                var remainingToDispatch = item.Quantity - alreadyDispatched;
                var inventory = inventories.FirstOrDefault(i => i.ProductId == item.ProductId);
                var availableToDispatch = Math.Min(remainingToDispatch, inventory?.QuantityAvailable ?? 0);

                viewModel.Items.Add(new OutwardEntryItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ProductSKU = item.Product.SKU,
                    QuantityOrdered = item.Quantity,
                    QuantityDispatched = alreadyDispatched,
                    QuantityAvailable = availableToDispatch,
                    Quantity = availableToDispatch > 0 ? availableToDispatch : 0
                });
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOutwardEntry(OutwardEntryCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Warehouses = await GetWarehousesSelectList();
                return View(viewModel);
            }

            var salesOrder = await _context.SalesOrders
                .Include(so => so.Items)
                .Include(so => so.OutwardEntries)
                    .ThenInclude(oe => oe.Items)
                .FirstOrDefaultAsync(so => so.Id == viewModel.SalesOrderId);

            if (salesOrder == null)
            {
                ModelState.AddModelError("", "Sales order not found.");
                viewModel.Warehouses = await GetWarehousesSelectList();
                return View(viewModel);
            }

            // Pre-validation
            foreach (var item in viewModel.Items.Where(i => i.Quantity > 0))
            {
                var orderedQty = salesOrder.Items.First(i => i.ProductId == item.ProductId).Quantity;
                var alreadyDispatched = salesOrder.OutwardEntries
                    .SelectMany(oe => oe.Items)
                    .Where(oi => oi.ProductId == item.ProductId)
                    .Sum(oi => oi.Quantity);

                var remainingToDispatch = orderedQty - alreadyDispatched;
                if (item.Quantity > remainingToDispatch)
                {
                    ModelState.AddModelError("",
                        $"Cannot dispatch {item.Quantity} of {item.ProductName}. Only {remainingToDispatch} remaining.");
                    viewModel.Warehouses = await GetWarehousesSelectList();
                    return View(viewModel);
                }

                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.WarehouseId == viewModel.WarehouseId && i.ProductId == item.ProductId);

                if (inventory == null || inventory.QuantityOnHand < item.Quantity)
                {
                    ModelState.AddModelError("",
                        $"Insufficient stock for {item.ProductName} in warehouse. Available: {inventory?.QuantityOnHand ?? 0}");
                    viewModel.Warehouses = await GetWarehousesSelectList();
                    return View(viewModel);
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var outwardEntry = new OutwardEntry
                {
                    OutwardNumber = viewModel.OutwardNumber,
                    OutwardDate = viewModel.OutwardDate,
                    SalesOrderId = viewModel.SalesOrderId,
                    WarehouseId = viewModel.WarehouseId,
                    Notes = viewModel.Notes,
                    Status = OutwardEntryStatus.Processed,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                _context.OutwardEntries.Add(outwardEntry);
                await _context.SaveChangesAsync(); // Save to get the ID

                foreach (var item in viewModel.Items.Where(i => i.Quantity > 0))
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.WarehouseId == viewModel.WarehouseId && i.ProductId == item.ProductId);

                    inventory.QuantityOnHand -= item.Quantity;
                    inventory.QuantityReserved -= item.Quantity;
                    inventory.LastUpdated = DateTime.UtcNow;
                    _context.Update(inventory);

                    var outwardItem = new OutwardEntryItem
                    {
                        OutwardEntryId = outwardEntry.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Notes = item.Notes
                    };
                    _context.OutwardEntryItems.Add(outwardItem);

                    var transactionEntry = new InventoryTransaction
                    {
                        ProductId = item.ProductId,
                        WarehouseId = viewModel.WarehouseId,
                        TransactionType = InventoryTransactionType.Outward,
                        Quantity = item.Quantity,
                        TransactionDate = DateTime.UtcNow,
                        OutwardEntryId = outwardEntry.Id,
                        SalesOrderId = salesOrder.Id,
                        ReferenceNumber = outwardEntry.OutwardNumber,
                        Notes = $"Outward for sales order {salesOrder.SONumber}",
                        PreviousQuantity = inventory.QuantityOnHand + item.Quantity,
                        NewQuantity = inventory.QuantityOnHand
                    };
                    _context.InventoryTransactions.Add(transactionEntry);
                }

                await UpdateSalesOrderStatus(salesOrder.Id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Details), new { id = viewModel.SalesOrderId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var errorMessage = $"Error creating outward entry: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nInner Exception: {ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $"\nInner Inner Exception: {ex.InnerException.InnerException.Message}";
                    }
                }
                ModelState.AddModelError("", errorMessage);
                viewModel.Warehouses = await GetWarehousesSelectList();
                return View(viewModel);
            }
        }

        // Helper method to get warehouse inventory (for AJAX call)
        [HttpGet]
        public async Task<IActionResult> GetWarehouseInventory(int warehouseId, int[] productIds)
        {
            var inventory = await _context.Inventories
                .Where(i => i.WarehouseId == warehouseId && productIds.Contains(i.ProductId))
                .Select(i => new
                {
                    productId = i.ProductId,
                    quantityAvailable = i.QuantityOnHand - i.QuantityReserved
                })
                .ToListAsync();

            return Json(inventory);
        }

        // GET: SalesOrder/InventoryOverview
        [HttpGet]
        public async Task<IActionResult> InventoryOverview()
        {
            var inventoryView = await _context.Inventories
                .Include(i => i.Product)
                .ThenInclude(p => p.Category)
                .Include(i => i.Warehouse)
                .Select(i => new InventoryViewModel
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    ProductSKU = i.Product.SKU,
                    CategoryName = i.Product.Category != null ? i.Product.Category.Name : "No Category",
                    WarehouseId = i.WarehouseId,
                    WarehouseName = i.Warehouse.Name,
                    QuantityOnHand = i.QuantityOnHand,
                    QuantityReserved = i.QuantityReserved,
                    QuantityAvailable = i.QuantityOnHand - i.QuantityReserved,
                    LastUpdated = i.LastUpdated,
                    PurchasePrice = i.Product.PurchasePrice ?? 0,
                    SalesPrice = i.Product.SalesPrice ?? 0,
                    InventoryValue = (i.QuantityOnHand - i.QuantityReserved) * (i.Product.PurchasePrice ?? 0),
                    PotentialSalesValue = (i.QuantityOnHand - i.QuantityReserved) * (i.Product.SalesPrice ?? 0)
                })
                .OrderBy(i => i.ProductName)
                .ToListAsync();

            return View(inventoryView);
        }

        // GET: SalesOrder/Details
        public async Task<IActionResult> Details(int id)
        {
            var salesOrder = await _context.SalesOrders
                .Include(so => so.Customer)
                .Include(so => so.Currency)
                .Include(so => so.Items)
                    .ThenInclude(i => i.Product)
                .Include(so => so.OutwardEntries)
                    .ThenInclude(oe => oe.Items)
                        .ThenInclude(oi => oi.Product)
                .Include(so => so.OutwardEntries)
                    .ThenInclude(oe => oe.Warehouse)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (salesOrder == null)
            {
                return NotFound();
            }

            var viewModel = new SalesOrderDetailsViewModel
            {
                SalesOrder = salesOrder,
                TotalOrdered = salesOrder.Items.Sum(i => i.Quantity),
                TotalDispatched = salesOrder.OutwardEntries
                    .SelectMany(oe => oe.Items)
                    .Sum(oi => oi.Quantity),
                RemainingToDispatch = salesOrder.Items.Sum(i => i.Quantity) -
                    salesOrder.OutwardEntries.SelectMany(oe => oe.Items).Sum(oi => oi.Quantity)
            };

            return View(viewModel);
        }

        // Helper methods
        private decimal CalculateLineTotal(int quantity, decimal unitPrice, decimal discountPercent, decimal taxRate)
        {
            decimal discountedPrice = unitPrice * (1 - discountPercent / 100);
            decimal subtotal = quantity * discountedPrice;
            decimal tax = subtotal * (taxRate / 100);
            return subtotal + tax;
        }

        private void CalculateOrderTotals(SalesOrder salesOrder)
        {
            salesOrder.SubTotal = salesOrder.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.DiscountPercent / 100));
            salesOrder.TaxAmount = salesOrder.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.DiscountPercent / 100) * (i.TaxRate / 100));
            salesOrder.TotalAmount = salesOrder.SubTotal + salesOrder.TaxAmount;
        }

        private async Task PopulateViewModelDropdowns(SalesOrderCreateViewModel viewModel)
        {
            viewModel.Customers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.CustomerDisplayName)
                .ToListAsync();
            viewModel.Currencies = await _context.Currencies.ToListAsync();
            viewModel.AvailableProducts = await _context.Inventories
                .Include(i => i.Product)
                .ThenInclude(p => p.Category)
                .Select(i => new InventoryViewModel
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    ProductSKU = i.Product.SKU,
                    CategoryName = i.Product.Category != null ? i.Product.Category.Name : "No Category",
                    WarehouseId = i.WarehouseId,
                    WarehouseName = i.Warehouse.Name,
                    QuantityOnHand = i.QuantityOnHand,
                    QuantityReserved = i.QuantityReserved,
                    QuantityAvailable = i.QuantityOnHand - i.QuantityReserved,
                    PurchasePrice = i.Product.PurchasePrice ?? 0,
                    SalesPrice = i.Product.SalesPrice ?? 0
                })
                .Where(i => i.QuantityAvailable > 0)
                .OrderBy(i => i.ProductName)
                .ToListAsync();
        }

        private async Task UpdateSalesOrderStatus(int salesOrderId)
        {
            var salesOrder = await _context.SalesOrders
                .Include(so => so.Items)
                .Include(so => so.OutwardEntries)
                    .ThenInclude(oe => oe.Items)
                .FirstOrDefaultAsync(so => so.Id == salesOrderId);

            if (salesOrder != null)
            {
                var totalOrdered = salesOrder.Items.Sum(i => i.Quantity);
                var totalDispatched = salesOrder.OutwardEntries
                    .SelectMany(oe => oe.Items)
                    .Sum(oi => oi.Quantity);

                salesOrder.Status = totalDispatched >= totalOrdered
                    ? SalesOrderStatus.Shipped
                    : totalDispatched > 0
                        ? SalesOrderStatus.PartiallyShipped
                        : SalesOrderStatus.Confirmed;

                salesOrder.ModifiedDate = DateTime.UtcNow;
                _context.Update(salesOrder);
            }
        }

        private async Task<SelectList> GetWarehousesSelectList()
        {
            return new SelectList(await _context.Warehouses
                .Where(w => w.IsActive)
                .ToListAsync(), "Id", "Name");
        }
    }
}