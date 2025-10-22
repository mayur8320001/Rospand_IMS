
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;

namespace Rospand_IMS.Controllers
{

    public class PaymentTermsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentTermsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PaymentTerms
        public async Task<IActionResult> Index()
        {
            return View(await _context.PaymentTerms.ToListAsync());
        }

        // GET: PaymentTerms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentTerm = await _context.PaymentTerms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paymentTerm == null)
            {
                return NotFound();
            }

            return View(paymentTerm);
        }

        // GET: PaymentTerms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PaymentTerms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,NetDays,IsActive")] PaymentTerm paymentTerm)
        {
            if (ModelState.IsValid)
            {
                paymentTerm.CreatedDate = DateTime.UtcNow;
                _context.Add(paymentTerm);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(paymentTerm);
        }

        // GET: PaymentTerms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentTerm = await _context.PaymentTerms.FindAsync(id);
            if (paymentTerm == null)
            {
                return NotFound();
            }
            return View(paymentTerm);
        }

        // POST: PaymentTerms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,NetDays,IsActive,CreatedDate")] PaymentTerm paymentTerm)
        {
            if (id != paymentTerm.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    paymentTerm.ModifiedDate = DateTime.UtcNow;
                    _context.Update(paymentTerm);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentTermExists(paymentTerm.Id))
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
            return View(paymentTerm);
        }

        // GET: PaymentTerms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentTerm = await _context.PaymentTerms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paymentTerm == null)
            {
                return NotFound();
            }

            return View(paymentTerm);
        }

        // POST: PaymentTerms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paymentTerm = await _context.PaymentTerms.FindAsync(id);
            _context.PaymentTerms.Remove(paymentTerm);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentTermExists(int id)
        {
            return _context.PaymentTerms.Any(e => e.Id == id);
        }
    }
}
