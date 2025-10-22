using Microsoft.AspNetCore.Mvc;
using Rospand_IMS.Models;
using Rospand_IMS.Services;

namespace Rospand_IMS.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var users = await _userService.GetAllUsersAsync();
            var viewModel = users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate,
                LastLoginDate = u.LastLoginDate
            }).ToList();
            
            return View(viewModel);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate
            };

            return View(viewModel);
        }

        // GET: User/Create
        public async Task<IActionResult> Create()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Get roles for dropdown
            var roles = await _userService.GetAllRolesAsync();
            ViewBag.Roles = roles.Select(r => r.RoleName).ToList();
            
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUser = await _userService.GetUserByUsernameAsync(model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    var roles = await _userService.GetAllRolesAsync();
                    ViewBag.Roles = roles.Select(r => r.RoleName).ToList();
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Role = model.Role,
                    IsActive = model.IsActive
                };

                await _userService.CreateUserAsync(user, model.Password);
                return RedirectToAction(nameof(Index));
            }
            
            // If we got this far, something failed, redisplay form
            var allRoles = await _userService.GetAllRolesAsync();
            ViewBag.Roles = allRoles.Select(r => r.RoleName).ToList();
            return View(model);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate
            };

            // Get roles for dropdown
            var roles = await _userService.GetAllRolesAsync();
            ViewBag.Roles = roles.Select(r => r.RoleName).ToList();

            return View(model);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserViewModel model)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userService.GetUserByIdAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    user.Username = model.Username;
                    user.Email = model.Email;
                    user.Role = model.Role;
                    user.IsActive = model.IsActive;

                    await _userService.UpdateUserAsync(user);
                }
                catch (Exception)
                {
                    if (!await UserExists(id))
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
            
            // Get roles for dropdown
            var roles = await _userService.GetAllRolesAsync();
            ViewBag.Roles = roles.Select(r => r.RoleName).ToList();
            
            return View(model);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate
            };

            return View(viewModel);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            await _userService.DeleteUserAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> UserExists(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return user != null;
        }
    }
}