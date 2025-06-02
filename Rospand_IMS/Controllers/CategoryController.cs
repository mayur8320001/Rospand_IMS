using global::Rospand_IMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;


namespace Rospand_IMS.Controllers
{
 
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            // Check for temp data messages to display with SweetAlert
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            return View(await _context.Categories.ToListAsync());
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Category ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Failed to create category. Please check the input data.";
            return View(category);
        }

        // GET: Category/Edit/5
       
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Category ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
        {
            if (id != category.Id)
            {
                TempData["ErrorMessage"] = "Invalid category ID.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Category updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.Id == category.Id))
                    {
                        TempData["ErrorMessage"] = "Category not found.";
                        return RedirectToAction(nameof(Index));
                    }
                    TempData["ErrorMessage"] = "Concurrency error occurred while updating category.";
                    throw;
                }
            }

            TempData["ErrorMessage"] = "Failed to update category. Please check the input data.";
            return View(category);
        }

        // GET: Category/Delete/5

       
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Category ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Category not found.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}