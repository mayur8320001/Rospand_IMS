using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models.LoginM;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rospand_IMS.Services
{
    public interface IPermissionService
    {
        Task<List<Role>> GetRolesWithPermissionsAsync();
        Task<List<Page>> GetAllPagesAsync();
        Task CreateRoleWithPermissionsAsync(string name, Dictionary<int, Dictionary<string, bool>> permissions);
        Task UpdateRolePermissionsAsync(int roleId, Dictionary<int, Dictionary<string, bool>> permissions);
        Task<object> GetRolePermissionsAsync(int roleId);
        Task DeleteRoleAsync(int roleId);
    }

    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Role>> GetRolesWithPermissionsAsync()
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Page)
                .ToListAsync();
        }

        public async Task<List<Page>> GetAllPagesAsync()
        {
            return await _context.Pages.ToListAsync();
        }

        public async Task CreateRoleWithPermissionsAsync(string name, Dictionary<int, Dictionary<string, bool>> permissions)
        {
            var role = new Role { Name = name };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            await AddPermissionsToRole(role.Id, permissions);
        }

        public async Task UpdateRolePermissionsAsync(int roleId, Dictionary<int, Dictionary<string, bool>> permissions)
        {
            // Debugging - log the incoming data
            Console.WriteLine($"Updating permissions for role {roleId}");
            foreach (var (pageId, pagePermissions) in permissions)
            {
                Console.WriteLine($"Page ID: {pageId}");
                foreach (var (permName, permValue) in pagePermissions)
                {
                    Console.WriteLine($"  {permName}: {permValue}");
                }
            }

            // Rest of your existing code...
            var existingPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            _context.RolePermissions.RemoveRange(existingPermissions);
            await _context.SaveChangesAsync();

            // Add new permissions
            await AddPermissionsToRole(roleId, permissions);
        }

        public async Task<object> GetRolePermissionsAsync(int roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return null;

            var allPages = await _context.Pages.ToListAsync();
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            var result = new
            {
                roleName = role.Name,
                pages = allPages.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    controller = p.Controller,
                    permissions = new
                    {
                        read = rolePermissions.FirstOrDefault(rp => rp.PageId == p.Id)?.CanRead ?? false,
                        create = rolePermissions.FirstOrDefault(rp => rp.PageId == p.Id)?.CanCreate ?? false,
                        update = rolePermissions.FirstOrDefault(rp => rp.PageId == p.Id)?.CanUpdate ?? false,
                        delete = rolePermissions.FirstOrDefault(rp => rp.PageId == p.Id)?.CanDelete ?? false,
                        adjust = rolePermissions.FirstOrDefault(rp => rp.PageId == p.Id)?.CanAdjust ?? false,
                        viewlowstock = rolePermissions.FirstOrDefault(rp => rp.PageId == p.Id)?.CanViewLowStock ?? false,
                        receive = rolePermissions.FirstOrDefault(rp => rp.PageId == p.Id)?.CanReceive ?? false
                    }
                }).ToList()
            };

            return result;
        }

        public async Task DeleteRoleAsync(int roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
            }
        }

        private async Task AddPermissionsToRole(int roleId, Dictionary<int, Dictionary<string, bool>> permissions)
        {
            var rolePermissions = new List<RolePermission>();

            foreach (var (pageId, pagePermissions) in permissions)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PageId = pageId,
                    CanRead = pagePermissions.GetValueOrDefault("read", false),
                    CanCreate = pagePermissions.GetValueOrDefault("create", false),
                    CanUpdate = pagePermissions.GetValueOrDefault("update", false),
                    CanDelete = pagePermissions.GetValueOrDefault("delete", false),
                    CanAdjust = pagePermissions.GetValueOrDefault("adjust", false),
                    CanViewLowStock = pagePermissions.GetValueOrDefault("viewlowstock", false),
                    CanReceive = pagePermissions.GetValueOrDefault("receive", false)
                });
            }

            await _context.RolePermissions.AddRangeAsync(rolePermissions);
            await _context.SaveChangesAsync();
        }
    }
}