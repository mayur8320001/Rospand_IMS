using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;

namespace Rospand_IMS.Controllers
{
    [Authorize]
    public class TaxController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaxController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Taxes.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tax = await _context.Taxes.FirstOrDefaultAsync(m => m.Id == id);
            return tax == null ? NotFound() : View(tax);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Rate,Code,Description,IsActive")] Tax tax)
        {
            if (ModelState.IsValid)
            {
                tax.CreatedDate = DateTime.UtcNow;
                _context.Add(tax);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tax);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tax = await _context.Taxes.FindAsync(id);
            return tax == null ? NotFound() : View(tax);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Rate,Code,Description,IsActive,CreatedDate")] Tax tax)
        {
            if (id != tax.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    tax.ModifiedDate = DateTime.UtcNow;
                    _context.Update(tax);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaxExists(tax.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tax);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tax = await _context.Taxes.FirstOrDefaultAsync(m => m.Id == id);
            return tax == null ? NotFound() : View(tax);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tax = await _context.Taxes.FindAsync(id);
            _context.Taxes.Remove(tax);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaxExists(int id)
        {
            return _context.Taxes.Any(e => e.Id == id);
        }
    }
}
