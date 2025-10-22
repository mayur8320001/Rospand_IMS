using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;
using Rospand_IMS.Pagination;

namespace Rospand_IMS.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customer
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["EmailSortParm"] = sortOrder == "Email" ? "email_desc" : "Email";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var customers = from c in _context.Customers
                                .Include(c => c.BillingAddress)
                                .Include(c => c.ShippingAddress)
                            select c;

            if (!String.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c => c.CustomerDisplayName.Contains(searchString)
                                       || c.CustomerEmail.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    customers = customers.OrderByDescending(c => c.CustomerDisplayName);
                    break;
                case "Email":
                    customers = customers.OrderBy(c => c.CustomerEmail);
                    break;
                case "email_desc":
                    customers = customers.OrderByDescending(c => c.CustomerEmail);
                    break;
                default:
                    customers = customers.OrderBy(c => c.CustomerDisplayName);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<Customer>.CreateAsync(customers.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.BillingAddress)
                .Include(c => c.ShippingAddress)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            ViewData["BillingAddressId"] = new SelectList(_context.Addresses, "Id", "Id");
            ViewData["ShippingAddressId"] = new SelectList(_context.Addresses, "Id", "Id");
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerType,Salutation,FirstName,LastName,ContactPersonName,CompanyName,CustomerDisplayName,CustomerEmail,WorkPhone,Mobile,TaxTypeId,TRNNumber,PaymentTermId,CurrencyId,OpeningBalance,BillingAddressId,ShippingAddressId,ShippingSameAsBilling,CreatedDate,ModifiedDate,IsActive")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BillingAddressId"] = new SelectList(_context.Addresses, "Id", "Id", customer.BillingAddressId);
            ViewData["ShippingAddressId"] = new SelectList(_context.Addresses, "Id", "Id", customer.ShippingAddressId);
            return View(customer);
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            ViewData["BillingAddressId"] = new SelectList(_context.Addresses, "Id", "Id", customer.BillingAddressId);
            ViewData["ShippingAddressId"] = new SelectList(_context.Addresses, "Id", "Id", customer.ShippingAddressId);
            return View(customer);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerType,Salutation,FirstName,LastName,ContactPersonName,CompanyName,CustomerDisplayName,CustomerEmail,WorkPhone,Mobile,TaxTypeId,TRNNumber,PaymentTermId,CurrencyId,OpeningBalance,BillingAddressId,ShippingAddressId,ShippingSameAsBilling,CreatedDate,ModifiedDate,IsActive")] Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BillingAddressId"] = new SelectList(_context.Addresses, "Id", "Id", customer.BillingAddressId);
            ViewData["ShippingAddressId"] = new SelectList(_context.Addresses, "Id", "Id", customer.ShippingAddressId);
            return View(customer);
        }

        // GET: Customer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.BillingAddress)
                .Include(c => c.ShippingAddress)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}