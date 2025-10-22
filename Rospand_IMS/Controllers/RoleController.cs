using Microsoft.AspNetCore.Mvc;
using Rospand_IMS.Models;
using Rospand_IMS.Services;

namespace Rospand_IMS.Controllers
{
    public class RoleController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMenuService _menuService;

        public RoleController(IUserService userService, IMenuService menuService)
        {
            _userService = userService;
            _menuService = menuService;
        }

        // GET: Role
        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var roles = await _userService.GetAllRolesAsync();
            var viewModel = roles.Select(r => new RoleViewModel
            {
                Id = r.Id,
                RoleName = r.RoleName,
                PageAccesses = r.PageAccesses.Select(pa => new PageAccessViewModel
                {
                    Id = pa.Id,
                    RoleId = pa.RoleId,
                    PageName = pa.PageName,
                    IsAdd = pa.IsAdd,
                    IsEdit = pa.IsEdit,
                    IsDelete = pa.IsDelete
                }).ToList()
            }).ToList();
            
            return View(viewModel);
        }

        // GET: Role/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var role = await _userService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var viewModel = new RoleViewModel
            {
                Id = role.Id,
                RoleName = role.RoleName,
                PageAccesses = role.PageAccesses.Select(pa => new PageAccessViewModel
                {
                    Id = pa.Id,
                    RoleId = pa.RoleId,
                    PageName = pa.PageName,
                    IsAdd = pa.IsAdd,
                    IsEdit = pa.IsEdit,
                    IsDelete = pa.IsDelete
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Role/Create
        public IActionResult Create()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            return View();
        }

        // POST: Role/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleViewModel model)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            if (ModelState.IsValid)
            {
                var role = new RoleMaster
                {
                    RoleName = model.RoleName
                };

                await _userService.CreateRoleAsync(role);
                return RedirectToAction(nameof(Index));
            }
            
            return View(model);
        }

        // GET: Role/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var role = await _userService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var viewModel = new RoleViewModel
            {
                Id = role.Id,
                RoleName = role.RoleName
            };

            return View(viewModel);
        }

        // POST: Role/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleViewModel model)
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
                    var role = await _userService.GetRoleByIdAsync(id);
                    if (role == null)
                    {
                        return NotFound();
                    }

                    role.RoleName = model.RoleName;

                    await _userService.UpdateRoleAsync(role);
                }
                catch (Exception)
                {
                    if (!await RoleExists(id))
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
            
            return View(model);
        }

        // GET: Role/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var role = await _userService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var viewModel = new RoleViewModel
            {
                Id = role.Id,
                RoleName = role.RoleName
            };

            return View(viewModel);
        }

        // POST: Role/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            await _userService.DeleteRoleAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Role/ManagePageAccess/5
        public async Task<IActionResult> ManagePageAccess(int id)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var role = await _userService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Get all menus
            var menus = await _menuService.GetAllMenusAsync();
            
            // Get existing page accesses for this role
            var existingPageAccesses = await _userService.GetPageAccessesByRoleAsync(id);
            
            var pageAccessViewModels = new List<PageAccessViewModel>();
            
            foreach (var menu in menus)
            {
                var existingAccess = existingPageAccesses.FirstOrDefault(pa => pa.PageName == menu.Name);
                
                pageAccessViewModels.Add(new PageAccessViewModel
                {
                    Id = existingAccess?.Id ?? 0,
                    RoleId = id,
                    PageName = menu.Name,
                    IsAdd = existingAccess?.IsAdd ?? false,
                    IsEdit = existingAccess?.IsEdit ?? false,
                    IsDelete = existingAccess?.IsDelete ?? false
                });
            }
            
            var viewModel = new RoleViewModel
            {
                Id = role.Id,
                RoleName = role.RoleName,
                PageAccesses = pageAccessViewModels
            };

            return View(viewModel);
        }

        // POST: Role/ManagePageAccess/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagePageAccess(int id, RoleViewModel model)
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
                    // Get existing page accesses for this role
                    var existingPageAccesses = await _userService.GetPageAccessesByRoleAsync(id);
                    
                    // Update or create page accesses
                    foreach (var pageAccessViewModel in model.PageAccesses)
                    {
                        var existingAccess = existingPageAccesses.FirstOrDefault(pa => pa.PageName == pageAccessViewModel.PageName);
                        
                        if (existingAccess != null)
                        {
                            // Update existing page access
                            existingAccess.IsAdd = pageAccessViewModel.IsAdd;
                            existingAccess.IsEdit = pageAccessViewModel.IsEdit;
                            existingAccess.IsDelete = pageAccessViewModel.IsDelete;
                            await _userService.UpdatePageAccessAsync(existingAccess);
                        }
                        else if (pageAccessViewModel.IsAdd || pageAccessViewModel.IsEdit || pageAccessViewModel.IsDelete)
                        {
                            // Create new page access only if at least one permission is granted
                            var pageAccess = new PageAccess
                            {
                                RoleId = pageAccessViewModel.RoleId,
                                PageName = pageAccessViewModel.PageName,
                                IsAdd = pageAccessViewModel.IsAdd,
                                IsEdit = pageAccessViewModel.IsEdit,
                                IsDelete = pageAccessViewModel.IsDelete
                            };
                            await _userService.CreatePageAccessAsync(pageAccess);
                        }
                    }
                }
                catch (Exception)
                {
                    if (!await RoleExists(id))
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
            
            return View(model);
        }

        private async Task<bool> RoleExists(int id)
        {
            var role = await _userService.GetRoleByIdAsync(id);
            return role != null;
        }
    }
}