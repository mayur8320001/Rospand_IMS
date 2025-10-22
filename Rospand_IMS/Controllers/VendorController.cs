
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Rospand_IMS.Controllers
{

    public class VendorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VendorController(ApplicationDbContext context)
        {
            _context = context;
        }
       
        private async Task PopulateViewData()
        {
            ViewData["TaxTypeId"] = new SelectList(await _context.TaxTypes.ToListAsync(), "Id", "Name");
            ViewData["PaymentTermId"] = new SelectList(await _context.PaymentTerms.ToListAsync(), "Id", "Name");
            ViewData["CurrencyId"] = new SelectList(await _context.Currencies.ToListAsync(), "Id", "Code");
            ViewData["CountryId"] = new SelectList(await _context.Countries.ToListAsync(), "Id", "Name");
        }

        // GET: Vendor
        public async Task<IActionResult> Index()
        {
            var vendors = await _context.Vendors
                .Include(v => v.TaxType)
                .Include(v => v.PaymentTerm)
                .Include(v => v.Currency)
                .Include(v => v.BillingAddress)
                .ThenInclude(a => a.Country)
                .Include(v => v.BillingAddress)
                .ThenInclude(a => a.State)
                .Include(v => v.BillingAddress)
                .ThenInclude(a => a.City)
                .OrderBy(v => v.CompanyName)
                .ToListAsync();

            return View(vendors);
        }
     

        // GET: Vendor/Create
        public async Task<IActionResult> Create()
        {
            await PopulateViewData();
            return View(new VendorCreateViewModel());
        }

        // POST: Vendor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VendorCreateViewModel viewModel)
        {
            // Custom validation - at least one contact method required
            if (string.IsNullOrEmpty(viewModel.VendorEmail) &&
                string.IsNullOrEmpty(viewModel.CustomerPhone) &&
                string.IsNullOrEmpty(viewModel.Mobile))
            {
                ModelState.AddModelError("", "At least one contact method (email or phone) is required");
            }

            // Validate billing address if provided
            if (viewModel.BillingAddress != null && !viewModel.ShippingSameAsBilling)
            {
                if (string.IsNullOrEmpty(viewModel.BillingAddress.StreetAddress))
                {
                    ModelState.AddModelError("BillingAddress.StreetAddress", "Street address is required for billing");
                }
            }

            if (ModelState.IsValid)
            {
                var vendor = new Vendor
                {
                    VendorType = viewModel.VendorType,
                    ContactPersonName = viewModel.ContactPersonName,
                    Salutation = viewModel.Salutation,
                    FirstName = viewModel.FirstName,
                    LastName = viewModel.LastName,
                    CompanyName = viewModel.CompanyName,
                    VendorDisplayName = viewModel.VendorDisplayName,
                    VendorEmail = viewModel.VendorEmail,
                    CustomerPhone = viewModel.CustomerPhone,
                    WorkPhone = viewModel.WorkPhone,
                    Mobile = viewModel.Mobile,
                    TaxTypeId = viewModel.TaxTypeId,
                    TRNNumber = viewModel.TRNNumber,
                    PaymentTermId = viewModel.PaymentTermId,
                    CurrencyId = viewModel.CurrencyId,
                    OpeningBalance = viewModel.OpeningBalance,
                    ShippingSameAsBilling = viewModel.ShippingSameAsBilling
                };

                // Handle billing address
                if (viewModel.BillingAddress != null && !string.IsNullOrEmpty(viewModel.BillingAddress.StreetAddress))
                {
                    vendor.BillingAddress = new Address
                    {
                        Attention = viewModel.BillingAddress.Attention,
                        ContactNo = viewModel.BillingAddress.ContactNo,
                        CountryId = viewModel.BillingAddress.CountryId,
                        StateId = viewModel.BillingAddress.StateId,
                        CityId = viewModel.BillingAddress.CityId,
                        ZipCode = viewModel.BillingAddress.ZipCode,
                        StreetAddress = viewModel.BillingAddress.StreetAddress
                    };
                    _context.Add(vendor.BillingAddress);
                }

                // Handle shipping address
                if (viewModel.ShippingSameAsBilling && viewModel.BillingAddress != null)
                {
                    // Copy billing address to shipping address
                    viewModel.ShippingAddress = new AddressViewModel
                    {
                        Attention = viewModel.BillingAddress.Attention,
                        ContactNo = viewModel.BillingAddress.ContactNo,
                        CountryId = viewModel.BillingAddress.CountryId,
                        StateId = viewModel.BillingAddress.StateId,
                        CityId = viewModel.BillingAddress.CityId,
                        ZipCode = viewModel.BillingAddress.ZipCode,
                        StreetAddress = viewModel.BillingAddress.StreetAddress
                    };
                    _context.Add(vendor.ShippingAddress);
                }
                else if (viewModel.ShippingAddress != null && !string.IsNullOrEmpty(viewModel.ShippingAddress.StreetAddress))
                {
                    vendor.ShippingAddress = new Address
                    {
                        Attention = viewModel.ShippingAddress.Attention,
                        ContactNo = viewModel.ShippingAddress.ContactNo,
                        CountryId = viewModel.ShippingAddress.CountryId,
                        StateId = viewModel.ShippingAddress.StateId,
                        CityId = viewModel.ShippingAddress.CityId,
                        ZipCode = viewModel.ShippingAddress.ZipCode,
                        StreetAddress = viewModel.ShippingAddress.StreetAddress
                    };
                    _context.Add(vendor.ShippingAddress);
                }

                await _context.SaveChangesAsync();

                // Set address IDs after saving
                if (vendor.BillingAddress != null)
                {
                    vendor.BillingAddressId = vendor.BillingAddress.Id;
                }
                if (vendor.ShippingAddress != null)
                {
                    vendor.ShippingAddressId = vendor.ShippingAddress.Id;
                }

                _context.Add(vendor);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Vendor created successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateViewData();
            return View(viewModel);
        }
        // GET: Supplier/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Supplier ID is required.";
                return NotFound();
            }

            var supplier = await _context.Vendors
                .Include(s => s.TaxType)
                .Include(s => s.PaymentTerm)
                .Include(s => s.Currency)
                .Include(s => s.BillingAddress)
                .ThenInclude(a => a.Country)
                .Include(s => s.BillingAddress)
                .ThenInclude(a => a.State)
                .Include(s => s.BillingAddress)
                .ThenInclude(a => a.City)
                .Include(s => s.ShippingAddress)
                .ThenInclude(a => a.Country)
                .Include(s => s.ShippingAddress)
                .ThenInclude(a => a.State)
                .Include(s => s.ShippingAddress)
                .ThenInclude(a => a.City)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supplier == null)
            {
                TempData["ErrorMessage"] = "Supplier not found.";
                return NotFound();
            }

            TempData["SuccessMessage"] = "Supplier details loaded successfully!";
            return View(supplier);
        }

    
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Supplier ID is required.";
                return NotFound();
            }

            var supplier = await _context.Vendors
                .Include(s => s.BillingAddress)
                .Include(s => s.ShippingAddress)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
            {
                TempData["ErrorMessage"] = "Supplier not found.";
                return NotFound();
            }

            await PopulateViewData();
            return View(supplier);
        }

        // POST: Supplier/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VendorType,ContactPersonName,Salutation,FirstName,LastName," +
    "CompanyName,VendorDisplayName,VendorEmail,CustomerPhone,WorkPhone,Mobile,TaxTypeId,TRNNumber,PaymentTermId," +
    "CurrencyId,OpeningBalance,ShippingSameAsBilling,BillingAddressId,ShippingAddressId")] Vendor supplier)
        {
            if (id != supplier.Id)
            {
                TempData["ErrorMessage"] = "Invalid supplier ID.";
                return NotFound();
            }

            // Get the existing vendor from the database including addresses
            var existingVendor = await _context.Vendors
                .Include(v => v.BillingAddress)
                .Include(v => v.ShippingAddress)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (existingVendor == null)
            {
                TempData["ErrorMessage"] = "Supplier not found.";
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the vendor properties (excluding navigation properties)
                    _context.Entry(existingVendor).CurrentValues.SetValues(supplier);

                    // Handle billing address
                    if (supplier.BillingAddress != null)
                    {
                        if (existingVendor.BillingAddress == null)
                        {
                            // Create new address
                            existingVendor.BillingAddress = supplier.BillingAddress;
                            _context.Addresses.Add(existingVendor.BillingAddress);
                        }
                        else
                        {
                            // Update existing address without changing its ID
                            supplier.BillingAddress.Id = existingVendor.BillingAddress.Id;
                            _context.Entry(existingVendor.BillingAddress).CurrentValues.SetValues(supplier.BillingAddress);
                        }
                    }
                    else if (existingVendor.BillingAddress != null)
                    {
                        // Remove existing billing address if new one is null
                        _context.Addresses.Remove(existingVendor.BillingAddress);
                        existingVendor.BillingAddress = null;
                        existingVendor.BillingAddressId = null;
                    }

                    // Handle shipping address
                    if (supplier.ShippingSameAsBilling)
                    {
                        if (existingVendor.BillingAddress == null)
                        {
                            TempData["ErrorMessage"] = "Cannot copy billing address to shipping address because billing address is not set.";
                            await PopulateViewData();
                            return View(supplier);
                        }

                        if (existingVendor.ShippingAddress == null)
                        {
                            // Create new address by copying billing address
                            existingVendor.ShippingAddress = new Address();
                            _context.Entry(existingVendor.ShippingAddress).CurrentValues.SetValues(existingVendor.BillingAddress);
                            _context.Addresses.Add(existingVendor.ShippingAddress);
                        }
                        else
                        {
                            // Update existing shipping address to match billing address
                            _context.Entry(existingVendor.ShippingAddress).CurrentValues.SetValues(existingVendor.BillingAddress);
                        }
                        existingVendor.ShippingAddressId = existingVendor.BillingAddressId;
                    }
                    else if (supplier.ShippingAddress != null)
                    {
                        if (existingVendor.ShippingAddress == null)
                        {
                            // Create new address
                            existingVendor.ShippingAddress = supplier.ShippingAddress;
                            _context.Addresses.Add(existingVendor.ShippingAddress);
                        }
                        else
                        {
                            // Update existing address without changing its ID
                            supplier.ShippingAddress.Id = existingVendor.ShippingAddress.Id;
                            _context.Entry(existingVendor.ShippingAddress).CurrentValues.SetValues(supplier.ShippingAddress);
                        }
                    }
                    else if (existingVendor.ShippingAddress != null)
                    {
                        // Remove existing shipping address if new one is null
                        _context.Addresses.Remove(existingVendor.ShippingAddress);
                        existingVendor.ShippingAddress = null;
                        existingVendor.ShippingAddressId = null;
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Supplier updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupplierExists(supplier.Id))
                    {
                        TempData["ErrorMessage"] = "Supplier not found.";
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Concurrency error occurred while updating supplier.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating supplier: {ex.Message}";
                    await PopulateViewData();
                    return View(supplier);
                }
            }

            TempData["ErrorMessage"] = "Failed to update supplier. Please check the input data.";
            await PopulateViewData();
            return View(supplier);
        }
        // GET: Supplier/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Supplier ID is required.";
                return NotFound();
            }

            var supplier = await _context.Vendors
                .Include(s => s.BillingAddress)
                .Include(s => s.ShippingAddress)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supplier == null)
            {
                TempData["ErrorMessage"] = "Supplier not found.";
                return NotFound();
            }

            return View(supplier);
        }

        // POST: Supplier/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _context.Vendors
                .Include(s => s.BillingAddress)
                .Include(s => s.ShippingAddress)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier != null)
            {
                if (supplier.BillingAddress != null)
                {
                    _context.Addresses.Remove(supplier.BillingAddress);
                }
                if (supplier.ShippingAddress != null && supplier.ShippingAddressId != supplier.BillingAddressId)
                {
                    _context.Addresses.Remove(supplier.ShippingAddress);
                }
                _context.Vendors.Remove(supplier);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Supplier deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Supplier not found.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SupplierExists(int id)
        {
            return _context.Vendors.Any(e => e.Id == id);
        }


        public async Task<JsonResult> GetStatesByCountry(int countryId)
        {
            var states = await _context.States
                .Where(s => s.CountryId == countryId)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();
            return Json(states);
        }

        public async Task<JsonResult> GetCitiesByState(string stateId)
        {
            var cities = await _context.Cities
                .Where(c => c.StateId == stateId)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return Json(cities);
        }

    }
}
