using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rospand_IMS.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rospand_IMS.Controllers
{
 
    public class RoleController : Controller
    {
        private readonly IPermissionService _permissionService;

        public RoleController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _permissionService.GetRolesWithPermissionsAsync();
            var pages = await _permissionService.GetAllPagesAsync();
            ViewBag.Pages = pages;
            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, Dictionary<int, Dictionary<string, bool>> permissions)
        {
            await _permissionService.CreateRoleWithPermissionsAsync(name, permissions);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            var permissions = await _permissionService.GetRolePermissionsAsync(roleId);
            return Json(permissions);
        }

        public class UpdateRolePermissionsModel
        {
            public int RoleId { get; set; }
            public Dictionary<int, Dictionary<string, bool>> Permissions { get; set; }
        }

        // Then update your controller action:
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateRolePermissionsModel model)
        {
            await _permissionService.UpdateRolePermissionsAsync(model.RoleId, model.Permissions);
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int roleId)
        {
            await _permissionService.DeleteRoleAsync(roleId);
            return RedirectToAction("Index");
        }
    }
}