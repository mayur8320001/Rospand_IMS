using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;

namespace Rospand_IMS.Controllers
{
    public class CostCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CostCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.CostCategories.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.CostCategories.FirstOrDefaultAsync(m => m.Id == id);
            return category == null ? NotFound() : View(category);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,IsDirectCost,IsActive")] CostCategory costCategory)
        {
            if (ModelState.IsValid)
            {
                costCategory.CreatedDate = DateTime.UtcNow;
                _context.Add(costCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(costCategory);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.CostCategories.FindAsync(id);
            return category == null ? NotFound() : View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsDirectCost,IsActive,CreatedDate")] CostCategory costCategory)
        {
            if (id != costCategory.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    costCategory.ModifiedDate = DateTime.UtcNow;
                    _context.Update(costCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CostCategoryExists(costCategory.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(costCategory);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.CostCategories.FirstOrDefaultAsync(m => m.Id == id);
            return category == null ? NotFound() : View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.CostCategories.FindAsync(id);
            _context.CostCategories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CostCategoryExists(int id)
        {
            return _context.CostCategories.Any(e => e.Id == id);
        }
    }
}
