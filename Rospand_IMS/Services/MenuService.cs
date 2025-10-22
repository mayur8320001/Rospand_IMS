using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;

namespace Rospand_IMS.Services
{
    public class MenuService : IMenuService
    {
        private readonly ApplicationDbContext _context;

        public MenuService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Menu>> GetAllMenusAsync()
        {
            return await _context.Menus
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
        }

        public async Task<Menu?> GetMenuByIdAsync(int id)
        {
            return await _context.Menus.FindAsync(id);
        }

        public async Task<Menu> CreateMenuAsync(Menu menu)
        {
            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();
            return menu;
        }

        public async Task UpdateMenuAsync(Menu menu)
        {
            _context.Menus.Update(menu);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMenuAsync(int id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu != null)
            {
                _context.Menus.Remove(menu);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SeedDefaultMenusAsync()
        {
            // Check if menus already exist
            if (await _context.Menus.AnyAsync())
                return;

            var defaultMenus = new List<Menu>
            {
                // Dashboard
                new Menu { Name = "Dashboard", ControllerName = "Home", ActionName = "Index", IconClass = "fas fa-tachometer-alt", DisplayOrder = 1 },
                
                // Master Pages
                new Menu { Name = "Master Pages", ControllerName = "", ActionName = "", IconClass = "fas fa-cubes", DisplayOrder = 2 },
                new Menu { Name = "Category Master", ControllerName = "Category", ActionName = "Index", IconClass = "fas fa-tags", DisplayOrder = 3, ParentId = 2 },
                new Menu { Name = "Product Master", ControllerName = "Product", ActionName = "Index", IconClass = "fas fa-boxes", DisplayOrder = 4, ParentId = 2 },
                new Menu { Name = "Unit Master", ControllerName = "Units", ActionName = "Index", IconClass = "fas fa-weight-hanging", DisplayOrder = 5, ParentId = 2 },
                new Menu { Name = "Payment Terms", ControllerName = "PaymentTerms", ActionName = "Index", IconClass = "fas fa-file-invoice-dollar", DisplayOrder = 6, ParentId = 2 },
                new Menu { Name = "Tax Master", ControllerName = "Tax", ActionName = "Index", IconClass = "fas fa-percentage", DisplayOrder = 7, ParentId = 2 },
                new Menu { Name = "Countries", ControllerName = "Country", ActionName = "Index", IconClass = "fas fa-globe", DisplayOrder = 8, ParentId = 2 },
                new Menu { Name = "States", ControllerName = "State", ActionName = "Index", IconClass = "fas fa-map-marked", DisplayOrder = 9, ParentId = 2 },
                new Menu { Name = "Cities", ControllerName = "City", ActionName = "Index", IconClass = "fas fa-city", DisplayOrder = 10, ParentId = 2 },
                
                // Inventory
                new Menu { Name = "Inventory", ControllerName = "", ActionName = "", IconClass = "fas fa-warehouse", DisplayOrder = 11 },
                new Menu { Name = "Stock Overview", ControllerName = "Inventory", ActionName = "Index", IconClass = "fas fa-clipboard-list", DisplayOrder = 12, ParentId = 11 },
                new Menu { Name = "Adjust Stock", ControllerName = "Inventory", ActionName = "Adjust", IconClass = "fas fa-sliders-h", DisplayOrder = 13, ParentId = 11 },
                new Menu { Name = "Low Stock Alert", ControllerName = "Inventory", ActionName = "LowStock", IconClass = "fas fa-exclamation-triangle", DisplayOrder = 14, ParentId = 11 },
                
                // Sales
                new Menu { Name = "Sales", ControllerName = "", ActionName = "", IconClass = "fas fa-shopping-cart", DisplayOrder = 15 },
                new Menu { Name = "Customer Master", ControllerName = "Customer", ActionName = "Index", IconClass = "fas fa-users", DisplayOrder = 16, ParentId = 15 },
                new Menu { Name = "Sales Orders", ControllerName = "SalesOrder", ActionName = "Index", IconClass = "fas fa-file-invoice", DisplayOrder = 17, ParentId = 15 },
                new Menu { Name = "Create Order", ControllerName = "SalesOrder", ActionName = "Create", IconClass = "fas fa-plus-circle", DisplayOrder = 18, ParentId = 15 },
                
                // Purchase
                new Menu { Name = "Purchase", ControllerName = "", ActionName = "", IconClass = "fas fa-shopping-basket", DisplayOrder = 19 },
                new Menu { Name = "Vendor Master", ControllerName = "Vendor", ActionName = "Index", IconClass = "fas fa-truck", DisplayOrder = 20, ParentId = 19 },
                new Menu { Name = "Purchase Orders", ControllerName = "PurchaseOrder", ActionName = "Index", IconClass = "fas fa-file-invoice", DisplayOrder = 21, ParentId = 19 },
                new Menu { Name = "Create Order", ControllerName = "PurchaseOrder", ActionName = "Create", IconClass = "fas fa-plus-circle", DisplayOrder = 22, ParentId = 19 },
                new Menu { Name = "Receive Order", ControllerName = "PurchaseOrder", ActionName = "ReceiveOrder", IconClass = "fas fa-check-circle", DisplayOrder = 23, ParentId = 19 },
                
                // User Management
                new Menu { Name = "User Management", ControllerName = "", ActionName = "", IconClass = "fas fa-users-cog", DisplayOrder = 24 },
                new Menu { Name = "Users", ControllerName = "User", ActionName = "Index", IconClass = "fas fa-user", DisplayOrder = 25, ParentId = 24 },
                new Menu { Name = "Role Management", ControllerName = "Role", ActionName = "Index", IconClass = "fas fa-user-shield", DisplayOrder = 26, ParentId = 24 },
                
                // Accounting
                new Menu { Name = "Accounting", ControllerName = "", ActionName = "", IconClass = "fas fa-file-invoice-dollar", DisplayOrder = 27 },
                new Menu { Name = "Dashboard", ControllerName = "Accounting", ActionName = "Index", IconClass = "fas fa-tachometer-alt", DisplayOrder = 28, ParentId = 27 },
                new Menu { Name = "Ledgers", ControllerName = "Accounting", ActionName = "Ledgers", IconClass = "fas fa-book", DisplayOrder = 29, ParentId = 27 },
                new Menu { Name = "Transactions", ControllerName = "Accounting", ActionName = "Transactions", IconClass = "fas fa-exchange-alt", DisplayOrder = 30, ParentId = 27 },
                new Menu { Name = "Sales Invoices", ControllerName = "Accounting", ActionName = "SalesInvoices", IconClass = "fas fa-file-invoice", DisplayOrder = 31, ParentId = 27 },
                new Menu { Name = "Daily Expenses", ControllerName = "Accounting", ActionName = "DailyExpenses", IconClass = "fas fa-money-bill-wave", DisplayOrder = 32, ParentId = 27 },
                new Menu { Name = "Financial Reports", ControllerName = "Accounting", ActionName = "Reports", IconClass = "fas fa-chart-bar", DisplayOrder = 33, ParentId = 27 }
            };

            _context.Menus.AddRange(defaultMenus);
            await _context.SaveChangesAsync();
        }
    }
}