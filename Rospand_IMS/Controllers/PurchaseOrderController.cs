using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Rospand_IMS.Data;
using Rospand_IMS.Models;
using Rospand_IMS.Pagination;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rospand_IMS.Controllers
{
    [Authorize]
    public class PurchaseOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConverter _converter;

        public PurchaseOrderController(ApplicationDbContext context, IConverter converter)
        {
            _context = context;
            _converter = converter;
        }

        // GET: PurchaseOrder
        public async Task<IActionResult> Index(PurchaseOrderStatus? status, int? pageNumber)
        {
            int pageSize = 12; // You can adjust this number based on how many cards you want per page

            var query = _context.PurchaseOrders
                .Include(po => po.Vendor)
                .Include(po => po.Currency)
                .OrderByDescending(po => po.OrderDate)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(po => po.Status == status.Value);
            }

            ViewBag.StatusFilter = status;
            return View(await PaginatedList<PurchaseOrder>.CreateAsync(query, pageNumber ?? 1, pageSize));
        }

        // GET: PurchaseOrder/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new PurchaseOrderCreateViewModel
            {
                OrderDate = DateTime.Today,
                ExpectedDeliveryDate = DateTime.Today.AddDays(7),
                PONumber = GeneratePONumber(),
                Vendors = await _context.Vendors.OrderBy(v => v.CompanyName).ToListAsync(),
                Currencies = await _context.Currencies.ToListAsync(),
                Products = await _context.Products.Include(p => p.Category).Where(p => p.Category != null).ToListAsync(),
                Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync()
            };

            return View(viewModel);
        }
        // POST: PurchaseOrder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrderCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var purchaseOrder = new PurchaseOrder
                {
                    PONumber = viewModel.PONumber,
                    OrderDate = viewModel.OrderDate,
                    ExpectedDeliveryDate = viewModel.ExpectedDeliveryDate,
                    VendorId = viewModel.VendorId,
                    Status = PurchaseOrderStatus.Draft,
                    CurrencyId = viewModel.CurrencyId,
                    Notes = viewModel.Notes,
                    CreatedDate = DateTime.Now,
                    //CreatedBy = User.Identity.Name
                };

                foreach (var item in viewModel.Items)
                {

                    // Update product purchase price if provided in PO
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null && item.UnitPrice > 0)
                    {
                        product.PurchasePrice = item.UnitPrice;
                        _context.Products.Update(product);
                    }

                    purchaseOrder.Items.Add(new PurchaseOrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountPercent = item.DiscountPercent,
                        TaxRate = item.TaxRate,
                        Notes = item.Notes
                    });
                }

                CalculateOrderTotals(purchaseOrder);

                _context.Add(purchaseOrder);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = purchaseOrder.Id });
            }

            // Repopulate dropdowns if model is invalid
            viewModel.Vendors = await _context.Vendors.ToListAsync();
            viewModel.Currencies = await _context.Currencies.ToListAsync();
            viewModel.Products = await _context.Products.ToListAsync();

            return View(viewModel);
        }

        // GET: PurchaseOrder/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder == null)
            {
                return NotFound();
            }

            if (purchaseOrder.Status != PurchaseOrderStatus.Draft)
            {
                TempData["ErrorMessage"] = "Only draft purchase orders can be edited.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var viewModel = new PurchaseOrderCreateViewModel
            {
                Id = purchaseOrder.Id,
                PONumber = purchaseOrder.PONumber,
                OrderDate = purchaseOrder.OrderDate,
                ExpectedDeliveryDate = purchaseOrder.ExpectedDeliveryDate,
                VendorId = purchaseOrder.VendorId,
                CurrencyId = purchaseOrder.CurrencyId,
                Notes = purchaseOrder.Notes,
                Vendors = await _context.Vendors.ToListAsync(),
                Currencies = await _context.Currencies.ToListAsync(),
                Products = await _context.Products.ToListAsync(),
                Items = purchaseOrder.Items.Select(i => new PurchaseOrderItemViewModel
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountPercent = i.DiscountPercent,
                    TaxRate = i.TaxRate,
                    Notes = i.Notes
                }).ToList()
            };

            return View(viewModel);
        }
        // POST: PurchaseOrder/Receive/5
        [HttpGet]
        public async Task<IActionResult> Receive(int id)
        {
            var order = await _context.PurchaseOrders
                .Include(po => po.Items)
                    .ThenInclude(i => i.Product)
                .Include(po => po.Vendor)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != PurchaseOrderStatus.Ordered && order.Status != PurchaseOrderStatus.PartiallyReceived)
            {
                TempData["ErrorMessage"] = "Only ordered or partially received purchase orders can be received.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(order);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int id, Dictionary<int, int> receivedQuantities)
        {
            // In your DbInitializer or similar
            if (!_context.Warehouses.Any())
            {
                _context.Warehouses.Add(new Warehouse { Name = "Main Warehouse", Location = "Headquarters" });

            }
            var order = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != PurchaseOrderStatus.Ordered && order.Status != PurchaseOrderStatus.PartiallyReceived)
            {
                TempData["ErrorMessage"] = "Only ordered or partially received purchase orders can be received.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Get the default warehouse or first available warehouse
            var warehouse = await _context.Warehouses.FirstOrDefaultAsync();
            if (warehouse == null)
            {
                TempData["ErrorMessage"] = "No warehouses exist in the system. Please create a warehouse first.";
                return RedirectToAction(nameof(Details), new { id });
            }

            bool allItemsFullyReceived = true;
            bool anyItemsReceived = false;

            foreach (var item in order.Items)
            {
                if (receivedQuantities.TryGetValue(item.Id, out int receivedQty) && receivedQty > 0)
                {
                    // Validate received quantity doesn't exceed ordered quantity
                    int remainingToReceive = item.Quantity - item.ReceivedQuantity;
                    receivedQty = Math.Min(receivedQty, remainingToReceive);

                    if (receivedQty > 0)
                    {
                        // Update received quantity
                        item.ReceivedQuantity += receivedQty;
                        anyItemsReceived = true;

                        // Update inventory
                        var inventory = await _context.Inventories
                            .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.WarehouseId == warehouse.Id);

                        if (inventory == null)
                        {
                            // Create inventory record if it doesn't exist
                            inventory = new Inventory
                            {
                                ProductId = item.ProductId,
                                WarehouseId = warehouse.Id, // Use the actual warehouse ID
                                QuantityOnHand = receivedQty,
                                QuantityReserved = 0
                            };
                            _context.Add(inventory);
                        }
                        else
                        {
                            inventory.QuantityOnHand += receivedQty;
                            _context.Update(inventory);
                        }

                        // Create inventory transaction
                        var transaction = new InventoryTransaction
                        {
                            ProductId = item.ProductId,
                            WarehouseId = warehouse.Id, // Use the actual warehouse ID
                            TransactionType = InventoryTransactionType.Purchase,
                            Quantity = receivedQty,
                            PurchaseOrderId = order.Id,
                            ReferenceNumber = $"PO-{order.PONumber}",
                            Notes = $"Received {receivedQty} of {item.Quantity} ordered"
                        };
                        _context.Add(transaction);
                    }

                    // Check if all items are fully received
                    if (item.ReceivedQuantity < item.Quantity)
                    {
                        allItemsFullyReceived = false;
                    }
                }
            }

            if (!anyItemsReceived)
            {
                TempData["ErrorMessage"] = "No items were received. Please enter quantities.";
                return RedirectToAction(nameof(Receive), new { id });
            }

            // Update PO status
            if (allItemsFullyReceived)
            {
                order.Status = PurchaseOrderStatus.Received;
                order.ReceivedDate = DateTime.Now;
                order.ReceivedBy = User.Identity.Name;
            }
            else
            {
                order.Status = PurchaseOrderStatus.PartiallyReceived;
            }

            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Purchase order items have been received successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        // POST: PurchaseOrder/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PurchaseOrderCreateViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var purchaseOrder = await _context.PurchaseOrders
                    .Include(po => po.Items)
                    .FirstOrDefaultAsync(po => po.Id == id);

                if (purchaseOrder == null)
                {
                    return NotFound();
                }

                if (purchaseOrder.Status != PurchaseOrderStatus.Draft)
                {
                    TempData["ErrorMessage"] = "Only draft purchase orders can be edited.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Update main PO properties
                purchaseOrder.PONumber = viewModel.PONumber;
                purchaseOrder.OrderDate = viewModel.OrderDate;
                purchaseOrder.ExpectedDeliveryDate = viewModel.ExpectedDeliveryDate;
                purchaseOrder.VendorId = viewModel.VendorId;
                purchaseOrder.CurrencyId = viewModel.CurrencyId;
                purchaseOrder.Notes = viewModel.Notes;
                purchaseOrder.ModifiedDate = DateTime.Now;
                purchaseOrder.ModifiedBy = User.Identity.Name;

                // Update items
                foreach (var item in purchaseOrder.Items.ToList())
                {
                    if (!viewModel.Items.Any(i => i.Id == item.Id))
                    {
                        _context.Remove(item); // Remove items not in the view model
                    }
                }

                foreach (var itemViewModel in viewModel.Items)
                {
                    var existingItem = purchaseOrder.Items.FirstOrDefault(i => i.Id == itemViewModel.Id);
                    if (existingItem != null)
                    {
                        // Update existing item
                        existingItem.ProductId = itemViewModel.ProductId;
                        existingItem.Quantity = itemViewModel.Quantity;
                        existingItem.UnitPrice = itemViewModel.UnitPrice;
                        existingItem.DiscountPercent = itemViewModel.DiscountPercent;
                        existingItem.TaxRate = itemViewModel.TaxRate;
                        existingItem.Notes = itemViewModel.Notes;
                    }
                    else
                    {
                        // Add new item
                        purchaseOrder.Items.Add(new PurchaseOrderItem
                        {
                            ProductId = itemViewModel.ProductId,
                            Quantity = itemViewModel.Quantity,
                            UnitPrice = itemViewModel.UnitPrice,
                            DiscountPercent = itemViewModel.DiscountPercent,
                            TaxRate = itemViewModel.TaxRate,
                            Notes = itemViewModel.Notes
                        });
                    }
                }

                CalculateOrderTotals(purchaseOrder);

                try
                {
                    _context.Update(purchaseOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseOrderExists(purchaseOrder.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = purchaseOrder.Id });
            }

            // Repopulate dropdowns if model is invalid
            viewModel.Vendors = await _context.Vendors.ToListAsync();
            viewModel.Currencies = await _context.Currencies.ToListAsync();
            viewModel.Products = await _context.Products.ToListAsync();

            return View(viewModel);
        }

        // GET: PurchaseOrder/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.PurchaseOrders
                .Include(po => po.Vendor)
                .Include(po => po.Currency)
                .Include(po => po.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: PurchaseOrder/Print/5
        public async Task<IActionResult> Print(int id)
        {
            var order = await _context.PurchaseOrders
                .Include(po => po.Vendor)
                .Include(po => po.Currency)
                .Include(po => po.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }



        [HttpGet]
        public IActionResult DownloadPurchaseOrderPdf(int id)
        {
            // Get the purchase order from database
            var purchaseOrder = _context.PurchaseOrders
                .Include(po => po.Vendor)
                .Include(po => po.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefault(po => po.Id == id);

            if (purchaseOrder == null)
            {
                return NotFound();
            }

            // Generate the PDF
            var pdfBytes = GeneratePurchaseOrderPdf(purchaseOrder);

            // Return as file download
            return File(pdfBytes, "application/pdf", $"PurchaseOrder_{purchaseOrder.PONumber}.pdf");
        }
        private byte[] GeneratePurchaseOrderPdf(PurchaseOrder purchaseOrder)
        {
            return QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
               //   page.Size(PageSizes.A4);
                  // page.Margin(1, Unit.Inch);

                    // Header with company info and PO number
                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Your Company Name").FontSize(16).Bold();
                            row.RelativeItem().Text($"PO #: {purchaseOrder.PONumber}").FontSize(14).AlignRight();
                        });

                        column.Item().Text("Purchase Order").FontSize(20).Bold().AlignCenter();
                        column.Item().PaddingBottom(10).LineHorizontal(1);
                    });

                    // Vendor and date information
                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(vendorCol =>
                            {
                                vendorCol.Item().Text("Vendor:").FontSize(12).Bold();
                                vendorCol.Item().Text(purchaseOrder.Vendor.VendorDisplayName);
                                vendorCol.Item().Text(purchaseOrder.Vendor.CompanyName);
                                vendorCol.Item().Text(purchaseOrder.Vendor.VendorEmail);
                            });

                            row.RelativeItem().Column(dateCol =>
                            {
                                dateCol.Item().Text("PO Date:").FontSize(12).Bold();
                                dateCol.Item().Text(purchaseOrder.OrderDate.ToString("yyyy-MM-dd"));

                                dateCol.Item().PaddingTop(10).Text("Expected Delivery:").FontSize(12).Bold();
                                dateCol.Item().Text(purchaseOrder.ExpectedDeliveryDate.ToString("yyyy-MM-dd") ?? "N/A");

                            });
                        });

                        // Line items table
                        column.Item().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Product
                                columns.RelativeColumn();   // SKU
                                columns.RelativeColumn();   // Quantity
                                columns.RelativeColumn();   // Unit Price
                                columns.RelativeColumn();   // Total
                            });

                            // Table header
                            table.Header(header =>
                            {
                                header.Cell().Text("Product").Bold();
                                header.Cell().Text("SKU").Bold();
                                header.Cell().Text("Qty").Bold();
                                header.Cell().Text("Unit Price").Bold();
                                header.Cell().Text("Total").Bold();
                            });

                            // Table rows
                            foreach (var item in purchaseOrder.Items)
                            {
                                table.Cell().Text(item.Product.Name);
                                table.Cell().Text(item.Product.SKU);
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text(item.UnitPrice.ToString("C"));
                                table.Cell().Text((item.Quantity * item.UnitPrice).ToString("C"));
                            }

                            // Footer with totals
                            table.Footer(footer =>
                            {
                                footer.Cell().ColumnSpan(3).AlignRight().Text("Subtotal:").Bold();
                                footer.Cell().Text(purchaseOrder.SubTotal.ToString("C"));

                                footer.Cell().ColumnSpan(3).AlignRight().Text("Tax:").Bold();
                                footer.Cell().Text(purchaseOrder.TaxAmount.ToString("C"));

                                footer.Cell().ColumnSpan(3).AlignRight().Text("Total:").Bold();
                                footer.Cell().Text(purchaseOrder.TotalAmount.ToString("C")).Bold();
                            });
                        });

                        // Notes section
                        if (!string.IsNullOrEmpty(purchaseOrder.Notes))
                        {
                            column.Item().PaddingTop(15).Text("Notes:").FontSize(12).Bold();
                            column.Item().Text(purchaseOrder.Notes);
                        }
                    });

                    // Footer with page numbers
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();
        }
        // POST: PurchaseOrder/Submit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var order = await _context.PurchaseOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != PurchaseOrderStatus.Draft)
            {
                TempData["ErrorMessage"] = "Only draft purchase orders can be submitted.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = PurchaseOrderStatus.Submitted;
            order.SubmittedDate = DateTime.Now;
            order.SubmittedBy = User.Identity.Name;
            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Purchase order has been submitted successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: PurchaseOrder/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var order = await _context.PurchaseOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != PurchaseOrderStatus.Submitted)
            {
                TempData["ErrorMessage"] = "Only submitted purchase orders can be approved.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = PurchaseOrderStatus.Approved;
            order.ApprovedDate = DateTime.Now;
            order.ApprovedBy = User.Identity.Name;
            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Purchase order has been approved successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: PurchaseOrder/Order/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Order(int id)
        {
            var order = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != PurchaseOrderStatus.Approved)
            {
                TempData["ErrorMessage"] = "Only approved purchase orders can be ordered.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = PurchaseOrderStatus.Ordered;
            order.OrderedDate = DateTime.Now;
            order.OrderedBy = User.Identity.Name;
            _context.Update(order);

            await _context.SaveChangesAsync(); // Removed the inventory update code

            TempData["SuccessMessage"] = "Purchase order has been marked as ordered successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: PurchaseOrder/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.PurchaseOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == PurchaseOrderStatus.Received || order.Status == PurchaseOrderStatus.Cancelled)
            {
                TempData["ErrorMessage"] = "Cannot cancel a purchase order that is already received or cancelled.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = PurchaseOrderStatus.Cancelled;
            order.CancelledDate = DateTime.Now;
            //order.CancelledBy = User.Identity.Name;
            order.CancellationReason = "Cancelled by user";
            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Purchase order has been cancelled successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: PurchaseOrder/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.PurchaseOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != PurchaseOrderStatus.Draft)
            {
                TempData["ErrorMessage"] = "Only draft purchase orders can be deleted.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.PurchaseOrders.Remove(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Purchase order has been deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool PurchaseOrderExists(int id)
        {
            return _context.PurchaseOrders.Any(e => e.Id == id);
        }

        private void CalculateOrderTotals(PurchaseOrder order)
        {
            order.SubTotal = order.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.DiscountPercent / 100));
            order.TaxAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.DiscountPercent / 100) * i.TaxRate / 100);
            order.TotalAmount = order.SubTotal + order.TaxAmount + order.ShippingCharges - order.DiscountAmount;
        }

        [HttpGet]
        public async Task<IActionResult> ReceiveOrder()
        {
            var viewModel = new ReceiveOrderViewModel
            {
                Vendors = await _context.Vendors
                    .OrderBy(v => v.CompanyName)
                    .Select(v => new SelectListItem
                    {
                        Value = v.Id.ToString(),
                        Text = v.CompanyName
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GetPurchaseOrders(int vendorId)
        {
            var purchaseOrders = await _context.PurchaseOrders
                .Where(po => po.VendorId == vendorId &&
                            (po.Status == PurchaseOrderStatus.Ordered ||
                             po.Status == PurchaseOrderStatus.PartiallyReceived))
                .OrderByDescending(po => po.OrderDate)
                .Select(po => new SelectListItem
                {
                    Value = po.Id.ToString(),
                    Text = $"{po.PONumber} - {po.OrderDate.ToShortDateString()} ({po.Status})"
                })
                .ToListAsync();

            return Json(purchaseOrders);
        }

        [HttpPost]
        public async Task<IActionResult> LoadPurchaseOrder(int purchaseOrderId)
        {
            var order = await _context.PurchaseOrders
                .Include(po => po.Vendor)
                .Include(po => po.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(po => po.Id == purchaseOrderId);

            if (order == null)
            {
                return NotFound();
            }

            return PartialView("_ReceiveOrderItems", order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReceiveOrder(ReceiveOrderViewModel viewModel)
        {
            // Ensure we have a warehouse
            var warehouse = await _context.Warehouses.FirstOrDefaultAsync();
            if (warehouse == null)
            {
                warehouse = new Warehouse { Name = "Main Warehouse", Location = "Headquarters" };
                _context.Warehouses.Add(warehouse);
                await _context.SaveChangesAsync();
            }

            var order = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == viewModel.PurchaseOrderId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != PurchaseOrderStatus.Ordered && order.Status != PurchaseOrderStatus.PartiallyReceived)
            {
                TempData["ErrorMessage"] = "Only ordered or partially received purchase orders can be received.";
                return RedirectToAction(nameof(ReceiveOrder));
            }

            bool allItemsFullyReceived = true;
            bool anyItemsReceived = false;

            foreach (var item in order.Items)
            {
                if (viewModel.ReceivedQuantities.TryGetValue(item.Id, out int receivedQty) && receivedQty > 0)
                {
                    int remainingToReceive = item.Quantity - item.ReceivedQuantity;
                    receivedQty = Math.Min(receivedQty, remainingToReceive);

                    if (receivedQty > 0)
                    {
                        item.ReceivedQuantity += receivedQty;
                        anyItemsReceived = true;

                        // Update inventory
                        var inventory = await _context.Inventories
                            .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.WarehouseId == warehouse.Id);

                        if (inventory == null)
                        {
                            inventory = new Inventory
                            {
                                ProductId = item.ProductId,
                                WarehouseId = warehouse.Id,
                                QuantityOnHand = receivedQty,
                                QuantityReserved = 0
                            };
                            _context.Add(inventory);
                        }
                        else
                        {
                            inventory.QuantityOnHand += receivedQty;
                            _context.Update(inventory);
                        }

                        // Add transaction
                        var transaction = new InventoryTransaction
                        {
                            ProductId = item.ProductId,
                            WarehouseId = warehouse.Id,
                            TransactionType = InventoryTransactionType.Purchase,
                            Quantity = receivedQty,
                            PurchaseOrderId = order.Id,
                            ReferenceNumber = $"PO-{order.PONumber}",
                            Notes = $"Received {receivedQty} of {item.Quantity} ordered"
                        };
                        _context.Add(transaction);
                    }

                    if (item.ReceivedQuantity < item.Quantity)
                    {
                        allItemsFullyReceived = false;
                    }
                }
            }

            if (!anyItemsReceived)
            {
                TempData["ErrorMessage"] = "No items were received. Please enter quantities.";
                return RedirectToAction(nameof(ReceiveOrder));
            }

            // Update PO status
            if (allItemsFullyReceived)
            {
                order.Status = PurchaseOrderStatus.Received;
                order.ReceivedDate = DateTime.Now;
                order.ReceivedBy = User.Identity.Name;
            }
            else
            {
                order.Status = PurchaseOrderStatus.PartiallyReceived;
            }

            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Purchase order items have been received successfully.";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        // Helper method to render view to string
        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = ControllerContext.ActionDescriptor.ActionName;
            }

            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                var viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                var viewResult = viewEngine.FindView(ControllerContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }

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
    }
}