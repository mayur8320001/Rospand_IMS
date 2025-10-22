
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Rospand_IMS.Data;
using Rospand_IMS.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Rospand_IMS.Controllers
{

    public class StateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StateController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: State
        public async Task<IActionResult> Index()
        {
            var states = _context.States.Include(s => s.Country);
            return View(await states.ToListAsync());
        }

        // GET: State/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var state = await _context.States
                .Include(s => s.Country)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (state == null)
            {
                return NotFound();
            }

            return View(state);
        }

        // GET: State/Create
        public IActionResult Create()
        {
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name");
            return View();
        }

        // POST: State/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CountryId")] State state)
        {
            if (ModelState.IsValid)
            {
                _context.Add(state);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name", state.CountryId);
            return View(state);
        }

        // GET: State/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var state = await _context.States.FindAsync(id);
            if (state == null)
            {
                return NotFound();
            }
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name", state.CountryId);
            return View(state);
        }

        // POST: State/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CountryId")] State state)
        {
            if (id != state.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(state);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StateExists(state.Id))
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
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name", state.CountryId);
            return View(state);
        }

        // GET: State/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var state = await _context.States
                .Include(s => s.Country)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (state == null)
            {
                return NotFound();
            }

            return View(state);
        }

        [HttpPost]
        public async Task<IActionResult> UploadStates(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length <= 0)
                return BadRequest("Invalid file.");

            if (!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Not a valid Excel file.");

            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);

            var worksheet = package.Workbook.Worksheets[0]; // First sheet
            var rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++) // Skipping header row
            {
                var stateId = worksheet.Cells[row, 3].Text?.Trim();  // Column 3 is StateId
                var name = worksheet.Cells[row, 1].Text?.Trim();      // Column 1 is Name
                var countryIdStr = worksheet.Cells[row, 2].Text?.Trim(); // Column 2 is CountryId

                if (string.IsNullOrEmpty(stateId) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(countryIdStr))
                    continue;

                if (!int.TryParse(countryIdStr, out int countryId))
                    continue;

                // Check if already exists in DB
                bool exists = _context.States.Any(s => s.StateId == stateId && s.CountryId == countryId);

                if (exists)
                    continue; // Skip if already exists

                var state = new State
                {
                    Name = name,
                    CountryId = countryId,
                    StateId = stateId
                };

                _context.States.Add(state);
            }

            await _context.SaveChangesAsync();
            return Ok("States uploaded successfully.");
        }



        // POST: State/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var state = await _context.States.FindAsync(id);
            _context.States.Remove(state);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StateExists(int id)
        {
            return _context.States.Any(e => e.Id == id);
        }
    }
}
