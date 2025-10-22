using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;
using Rospand_IMS.Models.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rospand_IMS.Controllers
{
    public class AccountingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Accounting Dashboard
        public async Task<IActionResult> Index()
        {
            var model = new AccountingDashboardViewModel
            {
                TotalLedgers = await _context.Ledgers.CountAsync(),
                TotalTransactions = await _context.Transactions.CountAsync(),
                TotalSalesInvoices = await _context.SalesInvoices.CountAsync(),
                TotalExpenses = await _context.DailyExpenses.CountAsync(),
                RecentTransactions = await _context.Transactions
                    .Include(t => t.Ledger)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(10)
                    .ToListAsync(),
                RecentInvoices = await _context.SalesInvoices
                    .OrderByDescending(i => i.InvoiceDate)
                    .Take(10)
                    .ToListAsync(),
                RecentExpenses = await _context.DailyExpenses
                    .OrderByDescending(e => e.ExpenseDate)
                    .Take(10)
                    .ToListAsync()
            };

            return View(model);
        }

        // API endpoint to get products for autocomplete
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Unit)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Sku = p.SKU,
                    SalesPrice = p.SalesPrice ?? 0,
                    UnitName = p.Unit != null ? p.Unit.Name : "",
                    AvailableQuantity = _context.Inventories
                        .Where(i => i.ProductId == p.Id)
                        .Sum(i => i.QuantityOnHand - i.QuantityReserved)
                })
                .ToListAsync();

            return Json(products);
        }

        // Ledgers
        public async Task<IActionResult> Ledgers()
        {
            var ledgers = await _context.Ledgers.ToListAsync();
            return View(ledgers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLedger(Ledger ledger)
        {
            if (ModelState.IsValid)
            {
                ledger.CreatedDate = DateTime.Now;
                ledger.CurrentBalance = ledger.OpeningBalance;
                _context.Ledgers.Add(ledger);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ledger created successfully.";
                return RedirectToAction(nameof(Ledgers));
            }
            
            var ledgers = await _context.Ledgers.ToListAsync();
            TempData["ErrorMessage"] = "Error creating ledger. Please check the form.";
            return View(nameof(Ledgers), ledgers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLedger(int id, Ledger ledger)
        {
            if (id != ledger.LedgerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingLedger = await _context.Ledgers.FindAsync(id);
                    if (existingLedger == null)
                    {
                        return NotFound();
                    }

                    existingLedger.LedgerName = ledger.LedgerName;
                    existingLedger.LedgerType = ledger.LedgerType;
                    existingLedger.OpeningBalance = ledger.OpeningBalance;
                    // Note: We don't update CurrentBalance here as it's calculated from transactions

                    _context.Update(existingLedger);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Ledger updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LedgerExists(ledger.LedgerId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Ledgers));
            }
            
            TempData["ErrorMessage"] = "Error updating ledger. Please check the form.";
            var ledgers = await _context.Ledgers.ToListAsync();
            return View(nameof(Ledgers), ledgers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLedger(int id)
        {
            var ledger = await _context.Ledgers.FindAsync(id);
            if (ledger != null)
            {
                // Check if ledger has transactions
                var hasTransactions = await _context.Transactions.AnyAsync(t => t.LedgerId == id);
                if (hasTransactions)
                {
                    TempData["ErrorMessage"] = "Cannot delete ledger with existing transactions.";
                    return RedirectToAction(nameof(Ledgers));
                }
                
                _context.Ledgers.Remove(ledger);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ledger deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Ledger not found.";
            }
            return RedirectToAction(nameof(Ledgers));
        }

        private bool LedgerExists(int id)
        {
            return _context.Ledgers.Any(e => e.LedgerId == id);
        }

        // Transactions
        public async Task<IActionResult> Transactions()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Ledger)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
            ViewBag.Ledgers = await _context.Ledgers.ToListAsync();
            return View(transactions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTransaction(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                transaction.TransactionDate = DateTime.Now;
                _context.Transactions.Add(transaction);
                
                // Update ledger balance
                var ledger = await _context.Ledgers.FindAsync(transaction.LedgerId);
                if (ledger != null)
                {
                    if (transaction.IsDebit)
                    {
                        ledger.CurrentBalance += transaction.Amount;
                    }
                    else
                    {
                        ledger.CurrentBalance -= transaction.Amount;
                    }
                    _context.Update(ledger);
                }
                
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Transaction created successfully.";
                return RedirectToAction(nameof(Transactions));
            }
            
            var transactions = await _context.Transactions
                .Include(t => t.Ledger)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
            ViewBag.Ledgers = await _context.Ledgers.ToListAsync();
            TempData["ErrorMessage"] = "Error creating transaction. Please check the form.";
            return View(nameof(Transactions), transactions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                // Reverse the ledger balance update
                var ledger = await _context.Ledgers.FindAsync(transaction.LedgerId);
                if (ledger != null)
                {
                    if (transaction.IsDebit)
                    {
                        ledger.CurrentBalance -= transaction.Amount;
                    }
                    else
                    {
                        ledger.CurrentBalance += transaction.Amount;
                    }
                    _context.Update(ledger);
                }
                
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Transaction deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Transaction not found.";
            }
            return RedirectToAction(nameof(Transactions));
        }

        // Sales Invoices
        public async Task<IActionResult> SalesInvoices()
        {
            var invoices = await _context.SalesInvoices
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
            return View(invoices);
        }

        // Create Sales Invoice form
        public async Task<IActionResult> CreateSalesInvoice()
        {
            // Get customers for dropdown
            ViewBag.Customers = await _context.Customers
                .OrderBy(c => c.CustomerDisplayName)
                .Select(c => new { c.Id, c.CustomerDisplayName })
                .ToListAsync();
            
            return View();
        }

        // Create Sales Invoice POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSalesInvoice(int customerId, Dictionary<string, string> items)
        {
            if (customerId <= 0)
            {
                TempData["ErrorMessage"] = "Please select a customer.";
                ViewBag.Customers = await _context.Customers
                    .OrderBy(c => c.CustomerDisplayName)
                    .Select(c => new { c.Id, c.CustomerDisplayName })
                    .ToListAsync();
                return View(new SalesInvoice());
            }

            // Parse items from form data
            var invoiceItems = new List<SalesInvoiceItemViewModel>();
            
            // The items will be sent as form fields like items[0].ProductId, items[0].Quantity, etc.
            // We need to parse them manually
            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("items[") && key.EndsWith("].ProductId"))
                {
                    var index = key.Substring(6, key.Length - 16); // Extract index from items[0].ProductId
                    
                    var productId = int.Parse(Request.Form[$"items[{index}].ProductId"]);
                    var quantity = int.Parse(Request.Form[$"items[{index}].Quantity"]);
                    var unitPrice = decimal.Parse(Request.Form[$"items[{index}].UnitPrice"]);
                    var totalPrice = decimal.Parse(Request.Form[$"items[{index}].TotalPrice"]);
                    
                    invoiceItems.Add(new SalesInvoiceItemViewModel
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = totalPrice
                    });
                }
            }

            if (!invoiceItems.Any())
            {
                TempData["ErrorMessage"] = "At least one invoice item is required.";
                ViewBag.Customers = await _context.Customers
                    .OrderBy(c => c.CustomerDisplayName)
                    .Select(c => new { c.Id, c.CustomerDisplayName })
                    .ToListAsync();
                return View(new SalesInvoice());
            }

            // Validate items
            foreach (var item in invoiceItems)
            {
                if (item.ProductId <= 0)
                {
                    TempData["ErrorMessage"] = "All items must have a valid product selected.";
                    ViewBag.Customers = await _context.Customers
                        .OrderBy(c => c.CustomerDisplayName)
                        .Select(c => new { c.Id, c.CustomerDisplayName })
                        .ToListAsync();
                    return View(new SalesInvoice());
                }
                
                if (item.Quantity <= 0)
                {
                    TempData["ErrorMessage"] = "All items must have a quantity greater than zero.";
                    ViewBag.Customers = await _context.Customers
                        .OrderBy(c => c.CustomerDisplayName)
                        .Select(c => new { c.Id, c.CustomerDisplayName })
                        .ToListAsync();
                    return View(new SalesInvoice());
                }
                
                if (item.UnitPrice <= 0)
                {
                    TempData["ErrorMessage"] = "All items must have a unit price greater than zero.";
                    ViewBag.Customers = await _context.Customers
                        .OrderBy(c => c.CustomerDisplayName)
                        .Select(c => new { c.Id, c.CustomerDisplayName })
                        .ToListAsync();
                    return View(new SalesInvoice());
                }
            }

            try
            {
                // Create the invoice
                var invoice = new SalesInvoice
                {
                    CustomerId = customerId,
                    InvoiceDate = DateTime.Now
                };

                // Generate invoice number
                var lastInvoice = await _context.SalesInvoices
                    .OrderByDescending(i => i.InvoiceId)
                    .FirstOrDefaultAsync();
                
                int lastNumber = 0;
                if (lastInvoice != null && !string.IsNullOrEmpty(lastInvoice.InvoiceNumber))
                {
                    var numberPart = lastInvoice.InvoiceNumber.Replace("INV-", "");
                    if (int.TryParse(numberPart, out int parsedNumber))
                    {
                        lastNumber = parsedNumber;
                    }
                }
                
                invoice.InvoiceNumber = $"INV-{(lastNumber + 1).ToString("D6")}";
                
                // Calculate total amount from items
                invoice.TotalAmount = invoiceItems.Sum(i => i.TotalPrice);
                
                // Add items to invoice
                foreach (var item in invoiceItems)
                {
                    invoice.Items.Add(new SalesInvoiceItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    });
                }
                
                _context.SalesInvoices.Add(invoice);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sales invoice created successfully.";
                return RedirectToAction(nameof(SalesInvoices));
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error saving invoice: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while saving the invoice. Please try again.";
                
                ViewBag.Customers = await _context.Customers
                    .OrderBy(c => c.CustomerDisplayName)
                    .Select(c => new { c.Id, c.CustomerDisplayName })
                    .ToListAsync();
                return View(new SalesInvoice());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSalesInvoice(int id)
        {
            var invoice = await _context.SalesInvoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);
                
            if (invoice != null)
            {
                // Remove all items first
                _context.SalesInvoiceItems.RemoveRange(invoice.Items);
                _context.SalesInvoices.Remove(invoice);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sales invoice deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Sales invoice not found.";
            }
            return RedirectToAction(nameof(SalesInvoices));
        }

        // Daily Expenses
        public async Task<IActionResult> DailyExpenses()
        {
            var expenses = await _context.DailyExpenses
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
            return View(expenses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDailyExpense(DailyExpense expense)
        {
            if (ModelState.IsValid)
            {
                expense.ExpenseDate = DateTime.Now;
                _context.DailyExpenses.Add(expense);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Expense created successfully.";
                return RedirectToAction(nameof(DailyExpenses));
            }
            
            var expenses = await _context.DailyExpenses
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
            TempData["ErrorMessage"] = "Error creating expense. Please check the form.";
            return View(nameof(DailyExpenses), expenses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDailyExpense(int id)
        {
            var expense = await _context.DailyExpenses.FindAsync(id);
            if (expense != null)
            {
                _context.DailyExpenses.Remove(expense);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Expense deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Expense not found.";
            }
            return RedirectToAction(nameof(DailyExpenses));
        }

        // Financial Reports
        public async Task<IActionResult> Reports()
        {
            // Get data for the last 12 months
            var now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-11);
            
            var monthlyData = new List<MonthlyFinancialData>();
            
            for (int i = 0; i < 12; i++)
            {
                var monthStart = startDate.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                
                var income = await _context.SalesInvoices
                    .Where(inv => inv.InvoiceDate >= monthStart && inv.InvoiceDate <= monthEnd)
                    .SumAsync(inv => (decimal?)inv.TotalAmount) ?? 0;
                    
                var expenses = await _context.DailyExpenses
                    .Where(exp => exp.ExpenseDate >= monthStart && exp.ExpenseDate <= monthEnd)
                    .SumAsync(exp => (decimal?)exp.Amount) ?? 0;
                    
                monthlyData.Add(new MonthlyFinancialData
                {
                    Month = monthStart.ToString("MMM yyyy"),
                    Income = income,
                    Expenses = expenses,
                    Profit = income - expenses
                });
            }
            
            var model = new FinancialReportViewModel
            {
                MonthlyData = monthlyData,
                TotalIncome = monthlyData.Sum(d => d.Income),
                TotalExpenses = monthlyData.Sum(d => d.Expenses),
                NetProfit = monthlyData.Sum(d => d.Profit)
            };
            
            return View(model);
        }
        
        // Details pages for individual records
        public async Task<IActionResult> LedgerDetails(int id)
        {
            var ledger = await _context.Ledgers
                .Include(l => l.LedgerTransactions)
                .FirstOrDefaultAsync(l => l.LedgerId == id);
                
            if (ledger == null)
            {
                return NotFound();
            }
            
            return View(ledger);
        }
        
        public async Task<IActionResult> TransactionDetails(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Ledger)
                .FirstOrDefaultAsync(t => t.TransactionId == id);
                
            if (transaction == null)
            {
                return NotFound();
            }
            
            return View(transaction);
        }
        
        public async Task<IActionResult> SalesInvoiceDetails(int id)
        {
            var invoice = await _context.SalesInvoices
                .Include(i => i.Items)
                .ThenInclude(item => item.Invoice)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);
                
            if (invoice == null)
            {
                return NotFound();
            }
            
            return View(invoice);
        }
    }

    // View Models
    public class AccountingDashboardViewModel
    {
        public int TotalLedgers { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalSalesInvoices { get; set; }
        public int TotalExpenses { get; set; }
        public List<Transaction> RecentTransactions { get; set; }
        public List<SalesInvoice> RecentInvoices { get; set; }
        public List<DailyExpense> RecentExpenses { get; set; }
    }

    public class MonthlyFinancialData
    {
        public string Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Profit { get; set; }
    }

    public class FinancialReportViewModel
    {
        public List<MonthlyFinancialData> MonthlyData { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
    }
    
    // ViewModel for Sales Invoice Items
    public class SalesInvoiceItemViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}