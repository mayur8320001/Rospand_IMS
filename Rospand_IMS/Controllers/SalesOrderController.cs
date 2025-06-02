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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id, bool createPurchaseForMissingItems = false)
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

            // Track inventory issues
            var outOfStockItems = new List<SalesOrderItem>();
            var insufficientStockItems = new List<(SalesOrderItem item, int available)>();

            // Validate inventory availability
            foreach (var item in salesOrder.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                if (inventory == null)
                {
                    outOfStockItems.Add(item);
                }
                else if (inventory.QuantityAvailable < item.Quantity)
                {
                    insufficientStockItems.Add((item, inventory.QuantityAvailable));
                }
            }

            // Handle missing/insufficient stock
            if (outOfStockItems.Any() || insufficientStockItems.Any())
            {
                if (createPurchaseForMissingItems)
                {
                    return await CreatePurchaseOrderForMissingItems(salesOrder, outOfStockItems, insufficientStockItems);
                }

                // Return view showing stock issues with options
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

            // Proceed with confirmation if all items are available
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Reserve stock
                    foreach (var item in salesOrder.Items)
                    {
                        var inventory = await _context.Inventories
                            .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                        inventory.QuantityReserved += item.Quantity;
                        inventory.LastUpdated = DateTime.UtcNow;
                        _context.Update(inventory);

                        // Record inventory transaction
                        var transactionEntry = new InventoryTransaction
                        {
                            ProductId = item.ProductId,
                            WarehouseId = inventory.WarehouseId,
                          //  TransactionType = InventoryTransactionType.SaleReservation,
                            Quantity = item.Quantity,
                            TransactionDate = DateTime.UtcNow,
                            SalesOrderId = salesOrder.Id,
                            ReferenceNumber = salesOrder.SONumber,
                            Notes = $"Reserved for sales order {salesOrder.SONumber}"
                        };
                        _context.InventoryTransactions.Add(transactionEntry);
                    }

                    // Update sales order status
                    salesOrder.Status = SalesOrderStatus.Confirmed;
                    salesOrder.ModifiedDate = DateTime.UtcNow;
                    _context.Update(salesOrder);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "An error occurred while confirming the sales order.");
                }
            }

            return RedirectToAction(nameof(Details), new { id = salesOrder.Id });
        }
        private string GeneratePONumber()
        {
            var lastPO = _context.PurchaseOrders
                .OrderByDescending(po => po.Id)
                .FirstOrDefault();

            int lastNumber = 0;
            if (lastPO != null && lastPO.PONumber != null)
            {
                var numberPart = lastPO.PONumber.Replace("PO-", "");
                if (int.TryParse(numberPart, out lastNumber))
                {
                    lastNumber = int.Parse(numberPart);
                }
            }

            return $"PO-{(lastNumber + 1).ToString("D6")}";
        }
        private async Task<IActionResult> CreatePurchaseOrderForMissingItems(
       SalesOrder salesOrder,
       List<SalesOrderItem> outOfStockItems,
       List<(SalesOrderItem item, int available)> insufficientStockItems)
        {
            // Verify currency exists
            var currency = await _context.Currencies.FindAsync(salesOrder.CurrencyId);
            /*if (currency == null)
            {
                // Fall back to default currency if specified currency doesn't exist
                currency = await _context.Currencies.FirstOrDefaultAsync(c => c.IsDefault);
                if (currency == null)
                {
                    return BadRequest("No valid currency available for purchase order");
                }
            }*/

            var purchaseOrder = new PurchaseOrder
            {
                PONumber = GeneratePONumber(),
                OrderDate = DateTime.UtcNow,
                ExpectedDeliveryDate = DateTime.UtcNow.AddDays(7),
                Status = PurchaseOrderStatus.Draft,
                Notes = $"Auto-generated for sales order {salesOrder.SONumber}",
                CreatedDate = DateTime.UtcNow,
                CurrencyId = currency.Id,
               // VendorId = await DetermineDefaultVendorId() // You'll need to implement this
            };

            // Add items that are completely out of stock
            foreach (var item in outOfStockItems)
            {
                purchaseOrder.Items.Add(new PurchaseOrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.PurchasePrice ?? 0,
                    Notes = $"Required for sales order {salesOrder.SONumber}"
                });
            }

            // Add items with insufficient stock
            foreach (var (item, available) in insufficientStockItems)
            {
                var neededQuantity = item.Quantity - available;
                if (neededQuantity > 0)
                {
                    purchaseOrder.Items.Add(new PurchaseOrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = neededQuantity,
                        UnitPrice = item.Product.PurchasePrice ?? 0,
                        Notes = $"Required for sales order {salesOrder.SONumber}"
                    });
                }
            }

            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();

            // Update sales order with backorder status
           // salesOrder.Status = SalesOrderStatus.Backordered;
            salesOrder.ModifiedDate = DateTime.UtcNow;
            _context.Update(salesOrder);
            await _context.SaveChangesAsync();

            // Redirect to purchase order for completion
            return RedirectToAction("Edit", "PurchaseOrder", new { id = purchaseOrder.Id });
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
                AvailableProducts = await _context.Products
                    .Include(p => p.Category)
                  
                    .Select(p => new InventoryViewModel
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        ProductSKU = p.SKU,
                        CategoryName = p.Category != null ? p.Category.Name : "No Category",
                        SalesPrice = p.SalesPrice ?? 0,
                        // Include inventory information if needed
                        QuantityAvailable = _context.Inventories
                            .Where(i => i.ProductId == p.Id)
                            .Sum(i => i.QuantityOnHand - i.QuantityReserved)
                    })
                    .Where(p => p.QuantityAvailable > 0)
                    .OrderBy(p => p.ProductName)
                    .ToListAsync(),
                Items = new List<SalesOrderItemViewModel>() // Initialize empty list
            };

            return View(viewModel);
        }

        // POST: SalesOrder/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalesOrderCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Validate inventory availability
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
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                    if (inventory == null || (inventory.QuantityOnHand - inventory.QuantityReserved) < item.Quantity)
                    {
                        ModelState.AddModelError($"Items[{viewModel.Items.IndexOf(item)}].Quantity",
                            $"Insufficient stock for product {item.ProductName}. Available: {(inventory != null ? inventory.QuantityOnHand - inventory.QuantityReserved : 0)}");
                    }
                }

                if (!ModelState.IsValid)
                {
                    // Repopulate dropdowns
                    await PopulateViewModelDropdowns(viewModel);
                    return View(viewModel);
                }

                var salesOrder = new SalesOrder
                {
                    SONumber = viewModel.SONumber,
                    OrderDate = viewModel.OrderDate,
                    ExpectedDeliveryDate = viewModel.ExpectedDeliveryDate,
                    CustomerId = viewModel.CustomerId,
                    Status = SalesOrderStatus.Draft,
                    CurrencyId = viewModel.CurrencyId,
                    Notes = viewModel.Notes,
                    CreatedDate = DateTime.Now
                };

                foreach (var item in viewModel.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        salesOrder.Items.Add(new SalesOrderItem
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice > 0 ? item.UnitPrice : product.SalesPrice ?? 0,
                            DiscountPercent = item.DiscountPercent,
                            TaxRate = item.TaxRate,
                            Notes = item.Notes,
                            LineTotal = CalculateLineTotal(item.Quantity, item.UnitPrice, item.DiscountPercent, item.TaxRate)
                        });
                    }
                }

                CalculateOrderTotals(salesOrder);

                // Add sales order to context
                _context.Add(salesOrder);
                await _context.SaveChangesAsync();

                // Update inventory and create transaction
                foreach (var item in salesOrder.Items)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);
                    if (inventory != null)
                    {
                        inventory.QuantityReserved += item.Quantity;
                        inventory.LastUpdated = DateTime.Now;
                        _context.Update(inventory);

                        // Record inventory transaction
                        var transaction = new InventoryTransaction
                        {
                            ProductId = item.ProductId,
                            WarehouseId = inventory.WarehouseId,
                            TransactionType = InventoryTransactionType.Sale,
                            Quantity = item.Quantity,
                            TransactionDate = DateTime.UtcNow,
                            SalesOrderId = salesOrder.Id,
                            ReferenceNumber = salesOrder.SONumber,
                            Notes = $"Sale order {salesOrder.SONumber}",

                        };
                        _context.InventoryTransactions.Add(transaction);
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = salesOrder.Id });
            }

            // Repopulate dropdowns if model is invalid
            await PopulateViewModelDropdowns(viewModel);
            return View(viewModel);
        }

        // Update PopulateViewModelDropdowns to match
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
                    QuantityAvailable = i.QuantityOnHand - i.QuantityReserved, // Compute in projection
                    PurchasePrice = i.Product.PurchasePrice ?? 0,
                    SalesPrice = i.Product.SalesPrice ?? 0
                })
                .Where(i => i.QuantityOnHand - i.QuantityReserved > 0) // Use database fields
                .OrderBy(i => i.ProductName)
                .ToListAsync();
        }

        // GET: SalesOrder/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var salesOrder = await _context.SalesOrders
                .Include(so => so.Customer)
                .Include(so => so.Currency)
                .Include(so => so.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (salesOrder == null)
            {
                return NotFound();
            }

            return View(salesOrder);
        }

        // Helper method to calculate line total
        private decimal CalculateLineTotal(int quantity, decimal unitPrice, decimal discountPercent, decimal taxRate)
        {
            decimal discountedPrice = unitPrice * (1 - discountPercent / 100);
            decimal subtotal = quantity * discountedPrice;
            decimal tax = subtotal * (taxRate / 100);
            return subtotal + tax;
        }

        // Helper method to calculate order totals
        private void CalculateOrderTotals(SalesOrder salesOrder)
        {
            salesOrder.SubTotal = salesOrder.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.DiscountPercent / 100));
            salesOrder.TaxAmount = salesOrder.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.DiscountPercent / 100) * (i.TaxRate / 100));
            salesOrder.TotalAmount = salesOrder.SubTotal + salesOrder.TaxAmount;
        }

        // Helper method to generate unique SO number
        private string GenerateSONumber()
        {
            // Simple implementation; consider a more robust method in production
            return $"SO-{DateTime.Now:yyyyMM}";
        }

        // GET: SalesOrder/CreateOutwardEntry/5
        public async Task<IActionResult> CreateOutwardEntry(int salesOrderId)
        {
            var salesOrder = await _context.SalesOrders
                .Include(so => so.Customer)
                .Include(so => so.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(so => so.Id == salesOrderId);

            if (salesOrder == null)
            {
                return NotFound();
            }

            if (salesOrder.Status != SalesOrderStatus.Confirmed)
            {
                return BadRequest("Sales order must be in Confirmed status to create an outward entry.");
            }

            var viewModel = new OutwardEntryCreateViewModel
            {
                SalesOrderId = salesOrder.Id,
                SONumber = salesOrder.SONumber,
                CustomerDisplayName = salesOrder.Customer.CustomerDisplayName,
                OutwardNumber = GenerateOutwardNumber(),
                OutwardDate = DateTime.UtcNow,
                Warehouses = new SelectList(await _context.Warehouses.Where(w => w.IsActive).ToListAsync(), "Id", "Name"),
                Items = salesOrder.Items.Select(i => new OutwardEntryItemViewModel
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    ProductSKU = i.Product.SKU,
                    QuantityOrdered = i.Quantity,
                    QuantityAvailable = _context.Inventories
                        .Where(inv => inv.ProductId == i.ProductId)
                        .Select(inv => inv.QuantityAvailable)
                        .FirstOrDefault(),
                    Quantity = i.Quantity, // Default to ordered quantity
                    Notes = ""
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: SalesOrder/CreateOutwardEntry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOutwardEntry(OutwardEntryCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Validate inventory availability
                foreach (var item in viewModel.Items)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.WarehouseId == viewModel.WarehouseId);

                    if (inventory == null || inventory.QuantityAvailable < item.Quantity)
                    {
                        ModelState.AddModelError($"Items[{viewModel.Items.IndexOf(item)}].Quantity",
                            $"Insufficient stock for product {item.ProductName} in warehouse. Available: {(inventory?.QuantityAvailable ?? 0)}");
                    }

                    if (item.Quantity > item.QuantityOrdered)
                    {
                        ModelState.AddModelError($"Items[{viewModel.Items.IndexOf(item)}].Quantity",
                            $"Quantity cannot exceed ordered quantity ({item.QuantityOrdered}) for product {item.ProductName}.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    viewModel.Warehouses = new SelectList(await _context.Warehouses.Where(w => w.IsActive).ToListAsync(), "Id", "Name");
                    return View(viewModel);
                }

                var outwardEntry = new OutwardEntry
                {
                    OutwardNumber = viewModel.OutwardNumber,
                    SalesOrderId = viewModel.SalesOrderId,
                    WarehouseId = viewModel.WarehouseId,
                    OutwardDate = viewModel.OutwardDate,
                    Notes = viewModel.Notes,
                    CreatedBy = User.Identity?.Name,
                    CreatedDate = DateTime.UtcNow
                };

                foreach (var item in viewModel.Items)
                {
                    outwardEntry.Items.Add(new OutwardEntryItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Notes = item.Notes
                    });
                }

                _context.Add(outwardEntry);
                await _context.SaveChangesAsync();

                // Update inventory and create transactions
                foreach (var item in outwardEntry.Items)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.WarehouseId == outwardEntry.WarehouseId);

                    if (inventory != null)
                    {
                        inventory.QuantityOnHand -= item.Quantity;
                        inventory.QuantityReserved -= item.Quantity;
                        inventory.LastUpdated = DateTime.UtcNow;
                      //  inventory.LastTransactionId = null; // Will be updated below
                        _context.Update(inventory);

                        var transaction = new InventoryTransaction
                        {
                            ProductId = item.ProductId,
                            WarehouseId = outwardEntry.WarehouseId,
                            TransactionType = InventoryTransactionType.TransferOut,
                            Quantity = item.Quantity,
                            TransactionDate = DateTime.UtcNow,
                            SalesOrderId = outwardEntry.SalesOrderId,
                            ReferenceNumber = outwardEntry.OutwardNumber,
                            Notes = $"Outward entry for sales order {outwardEntry.SalesOrder.SONumber}",
                         //   CreatedBy = User.Identity?.Name
                        };
                        _context.InventoryTransactions.Add(transaction);
                        await _context.SaveChangesAsync(); // Save to get transaction ID
                       // inventory.LastUpdated = transaction.Id;
                        _context.Update(inventory);
                    }
                }

                // Update sales order status to Shipped
                var salesOrder = await _context.SalesOrders.FindAsync(outwardEntry.SalesOrderId);
                if (salesOrder != null)
                {
                    salesOrder.Status = SalesOrderStatus.Shipped;
                    salesOrder.ModifiedDate = DateTime.UtcNow;
                    _context.Update(salesOrder);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = outwardEntry.SalesOrderId });
            }

            viewModel.Warehouses = new SelectList(await _context.Warehouses.Where(w => w.IsActive).ToListAsync(), "Id", "Name");
            return View(viewModel);
        }

        // Helper method to generate unique outward number
        private string GenerateOutwardNumber()
        {
            return $"OUT-{DateTime.Now:yyyyMM}";
        }

    }
}