/*using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;
using Rospand_IMS.Models.LoginM;
using System.Linq;

namespace Rospand_IMS.Utilities
{
    public static class DataSeed
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Seed Roles if none exist
            if (!context.Roles.Any())
            {
                var adminRole = new Role { Name = "Admin" };
                var managerRole = new Role { Name = "Manager" };
                var userRole = new Role { Name = "User" };

                context.Roles.AddRange(adminRole, managerRole, userRole);
                context.SaveChanges();

                // Seed all pages
                var pages = new[]
                {
                    // Dashboard
                    new Page { Name = "Dashboard", Controller = "Home" },
                    
                    // Master Pages
                    new Page { Name = "Category", Controller = "Category" },
                    new Page { Name = "Product", Controller = "Product" },
                    new Page { Name = "Units", Controller = "Units" },
                    new Page { Name = "Payment Terms", Controller = "PaymentTerms" },
                    new Page { Name = "Tax", Controller = "Tax" },
                    new Page { Name = "Country", Controller = "Country" },
                    new Page { Name = "State", Controller = "State" },
                    new Page { Name = "City", Controller = "City" },
                    
                    // Inventory
                    new Page { Name = "Inventory", Controller = "Inventory" },
                    new Page { Name = "Stock Adjustment", Controller = "Inventory", Action = "Adjust" },
                    new Page { Name = "Low Stock", Controller = "Inventory", Action = "LowStock" },
                    
                    // Sales
                    new Page { Name = "Customer", Controller = "Customer" },
                    new Page { Name = "Sales Order", Controller = "SalesOrder" },
                    new Page { Name = "Create Sales Order", Controller = "SalesOrder", Action = "Create" },
                    
                    // Purchase
                    new Page { Name = "Vendor", Controller = "Vendor" },
                    new Page { Name = "Purchase Order", Controller = "PurchaseOrder" },
                    new Page { Name = "Create Purchase Order", Controller = "PurchaseOrder", Action = "Create" },
                    new Page { Name = "Receive Order", Controller = "PurchaseOrder", Action = "ReceiveOrder" },
                    
                    // Reports (if you have them)
                    new Page { Name = "Sales Report", Controller = "Reports", Action = "Sales" },
                    new Page { Name = "Inventory Report", Controller = "Reports", Action = "Inventory" },
                    
                    // User Management
                    new Page { Name = "Users", Controller = "Users" },
                    new Page { Name = "Roles", Controller = "Roles" }
                };

                context.Pages.AddRange(pages);
                context.SaveChanges();

                // Admin gets full permissions for all pages
                var adminPermissions = pages.Select(p => new RolePermission
                {
                    RoleId = adminRole.Id,
                    PageId = p.Id,
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                });

                // Manager gets read/create/update but no delete for most pages
                var managerPermissions = pages.Select(p => new RolePermission
                {
                    RoleId = managerRole.Id,
                    PageId = p.Id,
                    CanRead = true,
                    CanCreate = p.Controller != "Users" && p.Controller != "Roles",
                    CanUpdate = true,
                    CanDelete = false
                });

                // Regular user gets read-only access
                var userPermissions = pages.Select(p => new RolePermission
                {
                    RoleId = userRole.Id,
                    PageId = p.Id,
                    CanRead = true,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                });

                context.RolePermissions.AddRange(adminPermissions);
                context.RolePermissions.AddRange(managerPermissions);
                context.RolePermissions.AddRange(userPermissions);
                context.SaveChanges();

                // Create initial admin user
                if (!context.Users.Any(u => u.Username == "admin@rospand.com"))
                {
                    var adminUser = new User
                    {
                        Username = "admin@rospand.com",
                       
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                        RoleId = adminRole.Id
                    };

                    context.Users.Add(adminUser);
                    context.SaveChanges();
                }
            }
        }
    }
}*/