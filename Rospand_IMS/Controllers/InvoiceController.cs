using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;
using Rospand_IMS.Services;

namespace Rospand_IMS.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
      InvoiceStatus? status,
      string searchString,
      DateTime? fromDate,
      DateTime? toDate,
      string sortOrder)
        {
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["AmountSortParm"] = sortOrder == "Amount" ? "amount_desc" : "Amount";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "status_desc" : "Status";
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = status;
            ViewData["FromDate"] = fromDate;
            ViewData["ToDate"] = toDate;

            var query = _context.Invoices
                .Include(i => i.SalesOrder)
                    .ThenInclude(so => so.Customer)
                .Include(i => i.SalesOrder)
                    .ThenInclude(so => so.Currency)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(i =>
                    i.InvoiceNumber.Contains(searchString) ||
                    i.SalesOrder.Customer.CustomerDisplayName.Contains(searchString) ||
                    i.SalesOrder.SONumber.Contains(searchString));
            }

            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(i => i.InvoiceDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(i => i.InvoiceDate <= toDate.Value);
            }

            query = sortOrder switch
            {
                "Date" => query.OrderBy(i => i.InvoiceDate),
                "date_desc" => query.OrderByDescending(i => i.InvoiceDate),
                "Amount" => query.OrderBy(i => i.TotalAmount),
                "amount_desc" => query.OrderByDescending(i => i.TotalAmount),
                "Status" => query.OrderBy(i => i.Status),
                "status_desc" => query.OrderByDescending(i => i.Status),
                _ => query.OrderByDescending(i => i.InvoiceDate),
            };

            var viewModel = new InvoiceIndexViewModel
            {
                Invoices = await query.ToListAsync(),
                SearchString = searchString,
                StatusFilter = status,
                FromDate = fromDate,
                ToDate = toDate,
                SortOrder = sortOrder
            };

            return View(viewModel);
        }
        // Add to your SalesOrderController or create a new InvoiceController

        private string GenerateInvoiceNumber()
        {
            var prefix = "INV";
            var today = DateTime.Today.ToString("yyMMdd");
            var lastNumber = _context.Invoices
                .Where(i => i.InvoiceNumber.StartsWith(prefix + today))
                .OrderByDescending(i => i.InvoiceNumber)
                .Select(i => i.InvoiceNumber)
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

        [HttpGet]
        public async Task<IActionResult> CreateInvoice(int salesOrderId)
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

            // Check if invoice already exists
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.SalesOrderId == salesOrderId);

            if (existingInvoice != null)
            {
              

                return RedirectToAction("InvoiceDetails", new { id = existingInvoice.Id });
            }

            var viewModel = new InvoiceCreateViewModel
            {
                SalesOrderId = salesOrderId,
                SONumber = salesOrder.SONumber,
                CustomerName = salesOrder.Customer.CustomerDisplayName,
                
                InvoiceDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(30), // Default 30-day terms
                Items = salesOrder.Items.Select(i => new InvoiceItemViewModel
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    ProductSKU = i.Product.SKU,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountPercent = i.DiscountPercent,
                    TaxRate = i.TaxRate,
                    Description = i.Notes
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInvoice(InvoiceCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var salesOrder = await _context.SalesOrders
                .Include(so => so.Customer)
                .Include(so => so.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(so => so.Id == viewModel.SalesOrderId);

            if (salesOrder == null)
            {
                return NotFound();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var invoice = new Invoice
                {
                    InvoiceNumber = GenerateInvoiceNumber(),
                    InvoiceDate = viewModel.InvoiceDate,
                    DueDate = viewModel.DueDate,
                    SalesOrderId = viewModel.SalesOrderId,
                    Status = InvoiceStatus.Draft,
                    Notes = viewModel.Notes,
                    CreatedDate = DateTime.Now
                };

                // Add items
                foreach (var item in viewModel.Items)
                {
                    invoice.Items.Add(new InvoiceItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountPercent = item.DiscountPercent,
                        TaxRate = item.TaxRate,
                        Description = item.Description,
                        LineTotal = CalculateLineTotal(item.Quantity, item.UnitPrice, item.DiscountPercent, item.TaxRate)
                    });
                }

                // Calculate totals
                invoice.SubTotal = invoice.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.DiscountPercent / 100));
                invoice.TaxAmount = invoice.Items.Sum(i => i.Quantity * i.UnitPrice * (1 - i.DiscountPercent / 100) * (i.TaxRate / 100));
                invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount;

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("InvoiceDetails", new { id = invoice.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Error creating invoice: {ex.Message}");
                return View(viewModel);
            }
        }



        private decimal CalculateLineTotal(int quantity, decimal unitPrice, decimal discountPercent, decimal taxRate)
        {
            var subtotal = quantity * unitPrice;
            var discountAmount = subtotal * (discountPercent / 100);
            var amountAfterDiscount = subtotal - discountAmount;
            var taxAmount = amountAfterDiscount * (taxRate / 100);
            return amountAfterDiscount + taxAmount;
        }
        [HttpGet]
        public async Task<IActionResult> InvoiceDetails(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.SalesOrder)
                    .ThenInclude(so => so.Customer)
                .Include(i => i.Items)
                    .ThenInclude(ii => ii.Product)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            var viewModel = new InvoiceDetailsViewModel
            {
                Invoice = invoice,
                TotalPaid = invoice.Payments.Sum(p => p.Amount),
                BalanceDue = invoice.TotalAmount - invoice.Payments.Sum(p => p.Amount),
                Payments = invoice.Payments.OrderByDescending(p => p.PaymentDate).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendInvoice(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            invoice.Status = InvoiceStatus.Sent;
            invoice.ModifiedDate = DateTime.Now;
            _context.Update(invoice);
            await _context.SaveChangesAsync();

            // TODO: Add email sending logic here

            return RedirectToAction("InvoiceDetails", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> AddPayment(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null)
            {
                return NotFound();
            }

            var viewModel = new PaymentViewModel
            {
                InvoiceId = invoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                PaymentDate = DateTime.Today,
                MaxAmount = invoice.TotalAmount - invoice.Payments.Sum(p => p.Amount)
            };

            return View(viewModel);
        }


       /* [HttpGet]s
        public async Task<IActionResult> DownloadInvoicePdf(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.SalesOrder)
                    .ThenInclude(so => so.Customer)
                .Include(i => i.Items)
                    .ThenInclude(ii => ii.Product)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            var pdfService = HttpContext.RequestServices.GetRequiredService<IPdfService>();
            var pdfBytes = await pdfService.GenerateInvoicePdf(invoice);

            return File(pdfBytes, "application/pdf", $"Invoice-{invoice.InvoiceNumber}.pdf");
        }*/

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(PaymentViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == viewModel.InvoiceId);

            if (invoice == null)
            {
                return NotFound();
            }

            var totalPaid = invoice.Payments.Sum(p => p.Amount);
            var remainingBalance = invoice.TotalAmount - totalPaid;

            if (viewModel.Amount > remainingBalance)
            {
                ModelState.AddModelError("Amount", $"Payment amount cannot exceed remaining balance of {remainingBalance:C}");
                return View(viewModel);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var payment = new Payment
                {
                    InvoiceId = viewModel.InvoiceId,
                    Amount = viewModel.Amount,
                    PaymentDate = viewModel.PaymentDate,
                    Method = viewModel.Method,
                    TransactionReference = viewModel.TransactionReference,
                    Notes = viewModel.Notes,
                    CreatedDate = DateTime.Now
                };

                _context.Payments.Add(payment);

                // Update invoice status
                var newTotalPaid = totalPaid + viewModel.Amount;
                if (newTotalPaid >= invoice.TotalAmount)
                {
                    invoice.Status = InvoiceStatus.Paid;
                }
                else
                {
                    invoice.Status = InvoiceStatus.PartiallyPaid;
                }

                invoice.AmountPaid = newTotalPaid;
                invoice.ModifiedDate = DateTime.Now;
                _context.Update(invoice);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("InvoiceDetails", new { id = viewModel.InvoiceId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Error recording payment: {ex.Message}");
                return View(viewModel);
            }
        }
    }
}
