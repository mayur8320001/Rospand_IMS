using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models.Employee;

namespace Rospand_IMS.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Employee
        public async Task<IActionResult> Index(string searchString, EmployeeStatus? status, int? departmentId)
        {
            var employees = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(e => 
                    e.FirstName.Contains(searchString) || 
                    e.LastName.Contains(searchString) || 
                    e.EmployeeId.Contains(searchString) ||
                    e.Email.Contains(searchString));
            }

            if (status.HasValue)
            {
                employees = employees.Where(e => e.Status == status.Value);
            }

            if (departmentId.HasValue)
            {
                employees = employees.Where(e => e.DepartmentId == departmentId.Value);
            }

            // Get departments for filter dropdown
            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.SearchString = searchString;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedDepartmentId = departmentId;

            return View(await employees.ToListAsync());
        }

        // GET: Employee/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employee/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Positions = await _context.Positions.ToListAsync();
            return View();
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                // Generate employee ID
                employee.EmployeeId = GenerateEmployeeId();
                employee.CreatedDate = DateTime.Now;
                
                _context.Add(employee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employee created successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Positions = await _context.Positions.ToListAsync();
            TempData["ErrorMessage"] = "Error creating employee. Please check the form.";
            return View(employee);
        }

        // GET: Employee/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            
            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Positions = await _context.Positions.ToListAsync();
            return View(employee);
        }

        // POST: Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    employee.ModifiedDate = DateTime.Now;
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Employee updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
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
            
            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Positions = await _context.Positions.ToListAsync();
            TempData["ErrorMessage"] = "Error updating employee. Please check the form.";
            return View(employee);
        }

        // GET: Employee/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employee deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Employee not found.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // Departments
        public async Task<IActionResult> Departments()
        {
            return View(await _context.Departments.ToListAsync());
        }

        // GET: Employee/CreateDepartment
        public IActionResult CreateDepartment()
        {
            return View();
        }

        // POST: Employee/CreateDepartment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Department created successfully.";
                return RedirectToAction(nameof(Departments));
            }
            
            TempData["ErrorMessage"] = "Error creating department. Please check the form.";
            return View(department);
        }

        // Positions
        public async Task<IActionResult> Positions()
        {
            var positions = await _context.Positions
                .Include(p => p.Department)
                .ToListAsync();
            return View(positions);
        }

        // GET: Employee/CreatePosition
        public async Task<IActionResult> CreatePosition()
        {
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View();
        }

        // POST: Employee/CreatePosition
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePosition(Position position)
        {
            if (ModelState.IsValid)
            {
                _context.Positions.Add(position);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Position created successfully.";
                return RedirectToAction(nameof(Positions));
            }
            
            ViewBag.Departments = await _context.Departments.ToListAsync();
            TempData["ErrorMessage"] = "Error creating position. Please check the form.";
            return View(position);
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

        private string GenerateEmployeeId()
        {
            var lastEmployee = _context.Employees
                .OrderByDescending(e => e.Id)
                .FirstOrDefault();

            int lastNumber = 0;
            if (lastEmployee != null && !string.IsNullOrEmpty(lastEmployee.EmployeeId))
            {
                var numberPart = lastEmployee.EmployeeId.Replace("EMP-", "");
                if (int.TryParse(numberPart, out int parsedNumber))
                {
                    lastNumber = parsedNumber;
                }
            }

            return $"EMP-{(lastNumber + 1).ToString("D4")}";
        }
    }
}