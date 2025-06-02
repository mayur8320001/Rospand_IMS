using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;

namespace Rospand_IMS.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

    
     
        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .Include(c => c.BillingAddress)
                    .ThenInclude(a => a.City)
                .Include(c => c.BillingAddress)
                    .ThenInclude(a => a.State)
                .Include(c => c.BillingAddress)
                    .ThenInclude(a => a.Country)
                .Include(c => c.ShippingAddress)
                    .ThenInclude(a => a.City)
                .Include(c => c.ShippingAddress)
                    .ThenInclude(a => a.State)
                .Include(c => c.ShippingAddress)
                    .ThenInclude(a => a.Country)
                .Include(c => c.TaxType)
                .Include(c => c.PaymentTerm)
                .Include(c => c.Currency)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null) return NotFound();

            return View(customer);
        }
        // GET: Customers/Create
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers
                .Include(c => c.BillingAddress)
                .Include(c => c.ShippingAddress)
                .Include(c => c.TaxType)
                .Include(c => c.PaymentTerm)
                .Include(c => c.Currency)
                .Where(c => c.IsActive)
                .OrderBy(c => c.CustomerDisplayName)
                .ToListAsync();

            return View(customers);
        }

        // GET: Customers/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CustomerViewModel
            {
                TaxTypes = new SelectList(await _context.TaxTypes.ToListAsync(), "Id", "Name"),
                PaymentTerms = new SelectList(await _context.PaymentTerms.ToListAsync(), "Id", "Name"),
                Currencies = new SelectList(await _context.Currencies.ToListAsync(), "Id", "Name"),
                Countries = new SelectList(await _context.Countries.ToListAsync(), "Id", "Name")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = new Customer
                {
                    CustomerType = model.CustomerType,
                    Salutation = model.Salutation,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    ContactPersonName = model.ContactPersonName,
                    CompanyName = model.CompanyName,
                    CustomerDisplayName = model.CustomerDisplayName,
                    CustomerEmail = model.CustomerEmail,
                    WorkPhone = model.WorkPhone,
                    Mobile = model.Mobile,
                    TaxTypeId = model.TaxTypeId,
                    TRNNumber = model.TRNNumber,
                    PaymentTermId = model.PaymentTermId,
                    CurrencyId = model.CurrencyId,
                    OpeningBalance = model.OpeningBalance,
                    ShippingSameAsBilling = model.ShippingSameAsBilling,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                // Handle billing address
                if (model.BillingCountryId.HasValue)
                {
                    customer.BillingAddress = new Address
                    {
                        Attention = model.BillingAttention,
                        ContactNo = model.BillingContactNo,
                        CountryId = model.BillingCountryId,
                        StateId = model.BillingStateId,
                        CityId = model.BillingCityId,
                        ZipCode = model.BillingZipCode,
                        StreetAddress = model.BillingStreetAddress
                    };
                }

                // Handle shipping address
                if (!model.ShippingSameAsBilling && model.ShippingCountryId.HasValue)
                {
                    customer.ShippingAddress = new Address
                    {
                        Attention = model.ShippingAttention,
                        ContactNo = model.ShippingContactNo,
                        CountryId = model.ShippingCountryId,
                        StateId = model.ShippingStateId,
                        CityId = model.ShippingCityId,
                        ZipCode = model.ShippingZipCode,
                        StreetAddress = model.ShippingStreetAddress
                    };
                }
                else if (model.ShippingSameAsBilling)
                {
                    customer.ShippingAddress = customer.BillingAddress;
                }

                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns if validation fails
            model.TaxTypes = new SelectList(await _context.TaxTypes.ToListAsync(), "Id", "Name", model.TaxTypeId);
            model.PaymentTerms = new SelectList(await _context.PaymentTerms.ToListAsync(), "Id", "Name", model.PaymentTermId);
            model.Currencies = new SelectList(await _context.Currencies.ToListAsync(), "Id", "Name", model.CurrencyId);
            model.Countries = new SelectList(await _context.Countries.ToListAsync(), "Id", "Name", model.BillingCountryId);

            if (model.BillingCountryId.HasValue)
            {
                model.States = new SelectList(await _context.States
                    .Where(s => s.CountryId == model.BillingCountryId)
                    .ToListAsync(), "Id", "Name", model.BillingStateId);
            }

            if (model.BillingStateId.HasValue)
            {
                model.Cities = new SelectList(await _context.Cities
                    .Where(c => c.StateId == model.BillingStateId)
                    .ToListAsync(), "Id", "Name", model.BillingCityId);
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .Include(c => c.BillingAddress)
                .Include(c => c.ShippingAddress)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null) return NotFound();

            ViewBag.TaxTypeId = new SelectList(_context.TaxTypes, "Id", "Name", customer.TaxTypeId);
            ViewBag.PaymentTermId = new SelectList(_context.PaymentTerms, "Id", "Name", customer.PaymentTermId);
            ViewBag.CurrencyId = new SelectList(_context.Currencies, "Id", "Name", customer.CurrencyId);
            ViewBag.CountryId = new SelectList(_context.Countries, "Id", "Name",
                customer.BillingAddress?.CountryId);

            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    customer.ModifiedDate = DateTime.Now;

                    if (customer.ShippingSameAsBilling)
                    {
                        customer.ShippingAddress = customer.BillingAddress;
                    }

                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns if validation fails
            ViewBag.TaxTypeId = new SelectList(_context.TaxTypes, "Id", "Name", customer.TaxTypeId);
            ViewBag.PaymentTermId = new SelectList(_context.PaymentTerms, "Id", "Name", customer.PaymentTermId);
            ViewBag.CurrencyId = new SelectList(_context.Currencies, "Id", "Name", customer.CurrencyId);
            ViewBag.CountryId = new SelectList(_context.Countries, "Id", "Name",
                customer.BillingAddress?.CountryId);

            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .Include(c => c.BillingAddress)
                .Include(c => c.ShippingAddress)
                .Include(c => c.TaxType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // AJAX methods for cascading dropdowns
        public JsonResult GetStatesByCountry(int countryId)
        {
            var states = _context.States
                .Where(s => s.CountryId == countryId)
                .Select(s => new { id = s.Id, name = s.Name })
                .ToList();

            return Json(states);
        }

        public JsonResult GetCitiesByState(int stateId)
        {
            var cities = _context.Cities
                .Where(c => c.StateId == stateId)
                .Select(c => new { id = c.Id, name = c.Name })
                .ToList();

            return Json(cities);
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}
