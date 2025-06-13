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
            var lastSO = _context.SalesOrders
                .OrderByDescending(so => so.Id)
                .FirstOrDefault();

            int lastNumber = 0;
            if (lastSO != null && lastSO.SONumber != null)
            {
                var numberPart = lastSO.SONumber.Replace("SO-", "");
                if (int.TryParse(numberPart, out lastNumber))
                {
                    lastNumber = int.Parse(numberPart);
                }
            }

            return $"SO-{(lastNumber + 1).ToString("D6")}";
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
        public async Task<IActionResult> Index(string searchString, SalesOrderStatus? status, int pageNumber = 1)
        {
            int pageSize = 10;

            // Start with a base query
            IQueryable<SalesOrder> salesOrders = _context.SalesOrders.Include(s => s.Customer).Include(s => s.Currency);

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                salesOrders = salesOrders.Where(s => s.SONumber.Contains(searchString) || s.Customer.CustomerDisplayName.Contains(searchString));
            }

            // Apply status filter
            if (status.HasValue)
            {
                salesOrders = salesOrders.Where(s => s.Status == status.Value);
            }

            // Calculate total number of pages
            int totalCount = await salesOrders.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Apply pagination
            salesOrders = salesOrders.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            // Execute the query
            var salesOrderList = await salesOrders.ToListAsync();

            // Create the view model
            var viewModel = new SalesOrderIndexViewModel
            {
                SalesOrders = salesOrderList,
                SearchString = searchString,
                Status = status,
                PageIndex = pageNumber,
                TotalPages = totalPages
            };

            return View(viewModel);
        }
        // GET: SalesOrder/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new SalesOrderCreateViewModel
            {
                OrderDate = DateTime.UtcNow,
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
                var available = await _context.Inventories
                    .Where(i => i.ProductId == item.ProductId)
                    .SumAsync(i => i.QuantityOnHand - i.QuantityReserved);

                if (available < item.Quantity)
                {
                    ModelState.AddModelError("", $"Not enough stock available for product {item.ProductName}. Available: {available}, Requested: {item.Quantity}");
                    await PopulateViewModelDropdowns(viewModel);
                    return View(viewModel);
                }
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
                    CreatedDate = DateTime.Now,
                };

                // Add items first without reserving inventory
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
                        LineTotal = CalculateLineTotal(item.Quantity,
                            item.UnitPrice > 0 ? item.UnitPrice : product.SalesPrice ?? 0,
                            item.DiscountPercent, item.TaxRate)
                    });
                }

                CalculateOrderTotals(salesOrder);
                _context.SalesOrders.Add(salesOrder);
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

            // Check inventory availability
            foreach (var item in salesOrder.Items)
            {
                var available = await _context.Inventories
                    .Where(i => i.ProductId == item.ProductId)
                    .SumAsync(i => i.QuantityOnHand - i.QuantityReserved);

                if (available == 0)
                {
                    outOfStockItems.Add(item);
                }
                else if (available < item.Quantity)
                {
                    insufficientStockItems.Add((item, available));
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
                // First verify all items can be reserved
                foreach (var item in salesOrder.Items)
                {
                    var totalToReserve = item.Quantity;
                    var inventories = await _context.Inventories
                        .Where(i => i.ProductId == item.ProductId &&
                               (i.QuantityOnHand - i.QuantityReserved) > 0)
                        .OrderByDescending(i => i.QuantityOnHand - i.QuantityReserved)
                        .ToListAsync();

                    foreach (var inventory in inventories)
                    {
                        var available = inventory.QuantityOnHand - inventory.QuantityReserved;
                        var reserveAmount = Math.Min(available, totalToReserve);

                        inventory.QuantityReserved += reserveAmount;
                        totalToReserve -= reserveAmount;

                        if (totalToReserve <= 0) break;
                    }

                    if (totalToReserve > 0)
                    {
                        throw new Exception($"Unable to reserve full quantity for product {item.Product.Name}");
                    }
                }

                // Update sales order status
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

            // Get reserved quantities for all products in the sales order
            var reservedQuantities = await _context.Inventories
                .Where(i => productIds.Contains(i.ProductId))
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    QuantityReserved = g.Sum(i => i.QuantityReserved)
                })
                .ToListAsync();

            foreach (var item in salesOrder.Items)
            {
                var alreadyDispatched = dispatchedQuantities.ContainsKey(item.ProductId)
                    ? dispatchedQuantities[item.ProductId]
                    : 0;

                var remainingToDispatch = item.Quantity - alreadyDispatched;
                var reservedForProduct = reservedQuantities.FirstOrDefault(i => i.ProductId == item.ProductId);
                var reservedQty = reservedForProduct?.QuantityReserved ?? 0;

                // The maximum we can dispatch is the lesser of remaining quantity or reserved quantity
                var maxDispatchQty = Math.Min(remainingToDispatch, reservedQty);

                viewModel.Items.Add(new OutwardEntryItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ProductSKU = item.Product.SKU,
                    QuantityOrdered = item.Quantity,
                    QuantityDispatched = alreadyDispatched,
                    QuantityReserved = reservedQty, // This is the actual reserved quantity
                    Quantity = maxDispatchQty > 0 ? maxDispatchQty : 0
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

    // Validate quantities
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

        var reservedInWarehouse = await _context.Inventories
            .Where(i => i.WarehouseId == viewModel.WarehouseId &&
                   i.ProductId == item.ProductId)
            .SumAsync(i => i.QuantityReserved);

        if (reservedInWarehouse < item.Quantity)
        {
            ModelState.AddModelError("",
                $"Only {reservedInWarehouse} units of {item.ProductName} are reserved in this warehouse.");
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
            CreatedDate = DateTime.Now,
        };

        _context.OutwardEntries.Add(outwardEntry);
        await _context.SaveChangesAsync();

        // Process each item being dispatched
        foreach (var item in viewModel.Items.Where(i => i.Quantity > 0))
        {
            var quantityToDispatch = item.Quantity;
            var inventories = await _context.Inventories
                .Where(i => i.WarehouseId == viewModel.WarehouseId &&
                       i.ProductId == item.ProductId &&
                       i.QuantityReserved > 0)
                .OrderByDescending(i => i.QuantityReserved)
                .ToListAsync();

            foreach (var inventory in inventories)
            {
                var reserveToDeduct = Math.Min(inventory.QuantityReserved, quantityToDispatch);
                
                // Reduce both reserved and on-hand quantities
                inventory.QuantityReserved -= reserveToDeduct;
                inventory.QuantityOnHand -= reserveToDeduct;
                inventory.LastUpdated = DateTime.Now;
                _context.Update(inventory);

                // Add outward item
                _context.OutwardEntryItems.Add(new OutwardEntryItem
                {
                    OutwardEntryId = outwardEntry.Id,
                    ProductId = item.ProductId,
                    Quantity = reserveToDeduct,
                    Notes = item.Notes
                });

                // Add single transaction record for the outward
                _context.InventoryTransactions.Add(new InventoryTransaction
                {
                    ProductId = item.ProductId,
                    WarehouseId = viewModel.WarehouseId,
                    TransactionType = InventoryTransactionType.Outward,
                    Quantity = reserveToDeduct,
                    TransactionDate = DateTime.Now,
                    OutwardEntryId = outwardEntry.Id,
                    SalesOrderId = salesOrder.Id,
                    ReferenceNumber = outwardEntry.OutwardNumber,
                    Notes = $"Outward for SO {salesOrder.SONumber}",
                    PreviousQuantity = inventory.QuantityOnHand + reserveToDeduct,
                    NewQuantity = inventory.QuantityOnHand,
                    PreviousReservedQuantity = inventory.QuantityReserved + reserveToDeduct,
                    NewReservedQuantity = inventory.QuantityReserved
                });

                quantityToDispatch -= reserveToDeduct;
                if (quantityToDispatch <= 0) break;
            }
        }

        await UpdateSalesOrderStatus(salesOrder.Id);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return RedirectToAction(nameof(Details), new { id = viewModel.SalesOrderId });
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        ModelState.AddModelError("", $"Error creating outward entry: {ex.Message}");
        viewModel.Warehouses = await GetWarehousesSelectList();
        return View(viewModel);
    }
}




        // GET: Mark as Delivered
        [HttpGet]
        public async Task<IActionResult> MarkAsDelivered(int id)
        {
            var outwardEntry = await _context.OutwardEntries
                .Include(oe => oe.SalesOrder)
                .FirstOrDefaultAsync(oe => oe.Id == id);

            if (outwardEntry == null)
            {
                return NotFound();
            }

            if (outwardEntry.Status != OutwardEntryStatus.Processed)
            {
                return BadRequest("Only shipped outward entries can be marked as delivered.");
            }

            var viewModel = new MarkAsDeliveredViewModel
            {
                OutwardEntryId = outwardEntry.Id,
                OutwardNumber = outwardEntry.OutwardNumber,
                DeliveryDate = outwardEntry.DeliveryDate ?? DateTime.Today,
              //  DeliveredBy = User.Identity?.Name // Or get from your user system
            };

            return View(viewModel);
        }

        // POST: Mark as Delivered
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsDelivered(MarkAsDeliveredViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var outwardEntry = await _context.OutwardEntries
                .Include(oe => oe.SalesOrder)
                    .ThenInclude(so => so.OutwardEntries)
                        .ThenInclude(oe => oe.Items)
                .Include(oe => oe.SalesOrder)
                    .ThenInclude(so => so.Items)
                .FirstOrDefaultAsync(oe => oe.Id == viewModel.OutwardEntryId);

            if (outwardEntry == null)
            {
                return NotFound();
            }

            if (outwardEntry.Status != OutwardEntryStatus.Processed)
            {
                ModelState.AddModelError("", "Only shipped outward entries can be marked as delivered.");
                return View(viewModel);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update outward entry status
                outwardEntry.Status = OutwardEntryStatus.Delivered;
                outwardEntry.DeliveryDate = viewModel.DeliveryDate;
              //  outwardEntry.DeliveredBy = viewModel.DeliveredBy;
                outwardEntry.ModifiedDate = DateTime.Now;
/*
                if (!string.IsNullOrEmpty(viewModel.Notes))
                {
                    outwardEntry.Notes = string.IsNullOrEmpty(outwardEntry.Notes)
                        ? $"Delivered on {viewModel.DeliveryDate:d} by {viewModel.DeliveredBy}. {viewModel.Notes}"
                        : $"{outwardEntry.Notes}\nDelivered on {viewModel.DeliveryDate:d} by {viewModel.DeliveredBy}. {viewModel.Notes}";
                }*/

                _context.Update(outwardEntry);

                // Check if all outward entries for this sales order are delivered
                var salesOrder = outwardEntry.SalesOrder;
                if (salesOrder != null && salesOrder.Status != SalesOrderStatus.Delivered)
                {
                    // Check if all outward entries are delivered
                    bool allOutwardDelivered = salesOrder.OutwardEntries.All(oe => oe.Status == OutwardEntryStatus.Delivered);

                    // Check if all items have been fully dispatched
                    bool allItemsFullyDispatched = true;
                    foreach (var item in salesOrder.Items)
                    {
                        var totalDispatched = salesOrder.OutwardEntries
                            .SelectMany(oe => oe.Items)
                            .Where(oi => oi.ProductId == item.ProductId)
                            .Sum(oi => oi.Quantity);

                        if (totalDispatched < item.Quantity)
                        {
                            allItemsFullyDispatched = false;
                            break;
                        }
                    }

                    if (allOutwardDelivered && allItemsFullyDispatched)
                    {
                        salesOrder.Status = SalesOrderStatus.Delivered;
                        salesOrder.ModifiedDate = DateTime.Now;
                        _context.Update(salesOrder);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Details), new { id = outwardEntry.SalesOrderId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Error marking as delivered: {ex.Message}");
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> OutwardDetails(int id)
        {
            var outwardEntry = await _context.OutwardEntries
                .Include(oe => oe.SalesOrder)
                .Include(oe => oe.Warehouse)
                .Include(oe => oe.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(oe => oe.Id == id);

            if (outwardEntry == null)
            {
                return NotFound();
            }

            return View(outwardEntry);
        }


        private bool IsSalesOrderFullyDelivered(SalesOrder salesOrder)
        {
            if (!salesOrder.OutwardEntries.Any())
                return false;

            // Check if all outward entries are delivered
            bool allOutwardDelivered = salesOrder.OutwardEntries.All(oe => oe.Status == OutwardEntryStatus.Delivered);

            // Check if all items have been fully dispatched
            bool allItemsFullyDispatched = true;
            foreach (var item in salesOrder.Items)
            {
                var totalDispatched = salesOrder.OutwardEntries
                    .SelectMany(oe => oe.Items)
                    .Where(oi => oi.ProductId == item.ProductId)
                    .Sum(oi => oi.Quantity);

                if (totalDispatched < item.Quantity)
                {
                    allItemsFullyDispatched = false;
                    break;
                }
            }

            return allOutwardDelivered && allItemsFullyDispatched;
        }

        // Helper method to get warehouse inventory (for AJAX call)
        [HttpGet]
        public async Task<IActionResult> GetWarehouseReservedInventory(int warehouseId, List<int> productIds)
        {
            var reservedQuantities = await _context.Inventories
                .Where(i => i.WarehouseId == warehouseId && productIds.Contains(i.ProductId))
                .GroupBy(i => i.ProductId)
                .Select(g => new {
                    productId = g.Key,
                    quantityReserved = g.Sum(i => i.QuantityReserved)
                })
                .ToListAsync();

            return Json(reservedQuantities);
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
            var subtotal = quantity * unitPrice;
            var discountAmount = subtotal * (discountPercent / 100);
            var amountAfterDiscount = subtotal - discountAmount;
            var taxAmount = amountAfterDiscount * (taxRate / 100);
            return amountAfterDiscount + taxAmount;
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

            if (salesOrder == null) return;

            if (IsSalesOrderFullyDelivered(salesOrder))
            {
                salesOrder.Status = SalesOrderStatus.Delivered;
            }
            else
            {
                // Existing status update logic for other statuses
                bool allItemsDispatched = true;
                foreach (var item in salesOrder.Items)
                {
                    var totalDispatched = salesOrder.OutwardEntries
                        .SelectMany(oe => oe.Items)
                        .Where(oi => oi.ProductId == item.ProductId)
                        .Sum(oi => oi.Quantity);

                    if (totalDispatched < item.Quantity)
                    {
                        allItemsDispatched = false;
                        break;
                    }
                }

                if (allItemsDispatched)
                {
                    salesOrder.Status = SalesOrderStatus.Shipped;
                }
                else if (salesOrder.OutwardEntries.Any())
                {
                    salesOrder.Status = SalesOrderStatus.PartiallyShipped;
                }
                else
                {
                    salesOrder.Status = SalesOrderStatus.Confirmed;
                }
            }

            salesOrder.ModifiedDate = DateTime.Now;
            _context.Update(salesOrder);
            await _context.SaveChangesAsync();
        }

        private async Task<SelectList> GetWarehousesSelectList()
        {
            return new SelectList(await _context.Warehouses
                .Where(w => w.IsActive)
                .ToListAsync(), "Id", "Name");
        }
    }
}