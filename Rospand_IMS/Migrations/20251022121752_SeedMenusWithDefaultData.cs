using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rospand_IMS.Migrations
{
    /// <inheritdoc />
    public partial class SeedMenusWithDefaultData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed default menus
            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "Name", "ControllerName", "ActionName", "IconClass", "ParentId", "DisplayOrder", "IsActive" },
                values: new object[,]
                {
                    // Dashboard
                    { 1, "Dashboard", "Home", "Index", "fas fa-tachometer-alt", null, 1, true },
                    
                    // Master Pages
                    { 2, "Master Pages", "", "", "fas fa-cubes", null, 2, true },
                    { 3, "Category Master", "Category", "Index", "fas fa-tags", 2, 3, true },
                    { 4, "Product Master", "Product", "Index", "fas fa-boxes", 2, 4, true },
                    { 5, "Unit Master", "Units", "Index", "fas fa-weight-hanging", 2, 5, true },
                    { 6, "Payment Terms", "PaymentTerms", "Index", "fas fa-file-invoice-dollar", 2, 6, true },
                    { 7, "Tax Master", "Tax", "Index", "fas fa-percentage", 2, 7, true },
                    { 8, "Countries", "Country", "Index", "fas fa-globe", 2, 8, true },
                    { 9, "States", "State", "Index", "fas fa-map-marked", 2, 9, true },
                    { 10, "Cities", "City", "Index", "fas fa-city", 2, 10, true },
                    
                    // Inventory
                    { 11, "Inventory", "", "", "fas fa-warehouse", null, 11, true },
                    { 12, "Stock Overview", "Inventory", "Index", "fas fa-clipboard-list", 11, 12, true },
                    { 13, "Adjust Stock", "Inventory", "Adjust", "fas fa-sliders-h", 11, 13, true },
                    { 14, "Low Stock Alert", "Inventory", "LowStock", "fas fa-exclamation-triangle", 11, 14, true },
                    
                    // Sales
                    { 15, "Sales", "", "", "fas fa-shopping-cart", null, 15, true },
                    { 16, "Customer Master", "Customer", "Index", "fas fa-users", 15, 16, true },
                    { 17, "Sales Orders", "SalesOrder", "Index", "fas fa-file-invoice", 15, 17, true },
                    { 18, "Create Order", "SalesOrder", "Create", "fas fa-plus-circle", 15, 18, true },
                    
                    // Purchase
                    { 19, "Purchase", "", "", "fas fa-shopping-basket", null, 19, true },
                    { 20, "Vendor Master", "Vendor", "Index", "fas fa-truck", 19, 20, true },
                    { 21, "Purchase Orders", "PurchaseOrder", "Index", "fas fa-file-invoice", 19, 21, true },
                    { 22, "Create Order", "PurchaseOrder", "Create", "fas fa-plus-circle", 19, 22, true },
                    { 23, "Receive Order", "PurchaseOrder", "ReceiveOrder", "fas fa-check-circle", 19, 23, true },
                    
                    // User Management
                    { 24, "User Management", "", "", "fas fa-users-cog", null, 24, true },
                    { 25, "Users", "User", "Index", "fas fa-user", 24, 25, true },
                    { 26, "Role Management", "Role", "Index", "fas fa-user-shield", 24, 26, true },
                    
                    // Accounting
                    { 27, "Accounting", "", "", "fas fa-file-invoice-dollar", null, 27, true },
                    { 28, "Dashboard", "Accounting", "Index", "fas fa-tachometer-alt", 27, 28, true },
                    { 29, "Ledgers", "Accounting", "Ledgers", "fas fa-book", 27, 29, true },
                    { 30, "Transactions", "Accounting", "Transactions", "fas fa-exchange-alt", 27, 30, true },
                    { 31, "Sales Invoices", "Accounting", "SalesInvoices", "fas fa-file-invoice", 27, 31, true },
                    { 32, "Daily Expenses", "Accounting", "DailyExpenses", "fas fa-money-bill-wave", 27, 32, true },
                    { 33, "Financial Reports", "Accounting", "Reports", "fas fa-chart-bar", 27, 33, true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove all seeded menus
            for (int i = 1; i <= 33; i++)
            {
                migrationBuilder.DeleteData(
                    table: "Menus",
                    keyColumn: "Id",
                    keyValue: i);
            }
        }
    }
}