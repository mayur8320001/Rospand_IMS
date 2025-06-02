using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rospand_IMS.Migrations
{
    /// <inheritdoc />
    public partial class updaAll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_CostCategories_CostCategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Units_UnitId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "PurchaseOrderItems");

            migrationBuilder.DropTable(
                name: "RolePagePermissions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "Pages");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropColumn(
                name: "MinimumStockLevel",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OpeningStock",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RatePer",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Products",
                newName: "Width");

            migrationBuilder.RenameColumn(
                name: "ReorderQuantity",
                table: "Products",
                newName: "WeightUnit");
            migrationBuilder.AddColumn<int>(
            name: "TypeTemp",
            table: "Products",
            nullable: false,
            defaultValue: 0);

            // Convert data from string to int (you'll need to map your string values to integers)
            migrationBuilder.Sql(@"
            UPDATE Products 
            SET TypeTemp = CASE 
                WHEN Type = 'Physical' THEN 0
                WHEN Type = 'Service' THEN 1
                ELSE 0
            END");

            // Drop old column
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Products");

            // Rename new column
            migrationBuilder.RenameColumn(
                name: "TypeTemp",
                table: "Products",
                newName: "Type");
            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                table: "Products",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "CostCategoryId",
                table: "Products",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "Height",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGroupedProduct",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Length",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceDuration",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductComponent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentProductId = table.Column<int>(type: "int", nullable: false),
                    ComponentProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductComponent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductComponent_Products_ComponentProductId",
                        column: x => x.ComponentProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductComponent_Products_ParentProductId",
                        column: x => x.ParentProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponent_ComponentProductId",
                table: "ProductComponent",
                column: "ComponentProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponent_ParentProductId",
                table: "ProductComponent",
                column: "ParentProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_CostCategories_CostCategoryId",
                table: "Products",
                column: "CostCategoryId",
                principalTable: "CostCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Units_UnitId",
                table: "Products",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_CostCategories_CostCategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Units_UnitId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductComponent");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsGroupedProduct",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ServiceDuration",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Width",
                table: "Products",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "WeightUnit",
                table: "Products",
                newName: "ReorderQuantity");

            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CostCategoryId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumStockLevel",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OpeningStock",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RatePer",
                table: "Products",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PONumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pages_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePagePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PageId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePagePermissions", x => new { x.RoleId, x.PageId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePagePermissions_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePagePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePagePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "View" },
                    { 2, "Edit" },
                    { 3, "Delete" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "IsEditable", "Name" },
                values: new object[] { 1, false, "SuperAdmin" });

            migrationBuilder.InsertData(
                table: "Sections",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Dashboard" },
                    { 2, "Master Pages" },
                    { 3, "Sales" },
                    { 4, "Purchase" },
                    { 5, "User Management" },
                    { 6, "Reports" },
                    { 7, "Marketing" }
                });

            migrationBuilder.InsertData(
                table: "Pages",
                columns: new[] { "Id", "Name", "SectionId", "Url" },
                values: new object[,]
                {
                    { 1, "Dashboard", 1, "/Home/Index" },
                    { 2, "Category Master", 2, "/Category/Index" },
                    { 3, "Product Master", 2, "/Product/Index" },
                    { 4, "Unit Master", 2, "/Units/Index" },
                    { 5, "Payment Terms", 2, "/PaymentTerms/Index" },
                    { 6, "Tax Master", 2, "/Tax/Index" },
                    { 7, "Countries", 2, "/Country/Index" },
                    { 8, "States", 2, "/State/Index" },
                    { 9, "Cities", 2, "/City/Index" },
                    { 10, "Customer Master", 3, "/Customer/Index" },
                    { 11, "Vendor Master", 4, "/Vendor/Index" },
                    { 12, "Purchase Order", 4, "/PurchaseOrder/Index" },
                    { 13, "Manage Users", 5, "/User/ManageUsers" },
                    { 14, "Roles", 5, "/User/Roles" },
                    { 15, "Summary Report", 6, "/Report/Summary" },
                    { 16, "Detailed Report", 6, "/Report/Detailed" },
                    { 17, "Campaigns", 7, "/Marketing/Campaigns" },
                    { 18, "Analytics", 7, "/Marketing/Analytics" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "RoleId", "Username" },
                values: new object[] { 1, "$2a$11$G2wB8fX77/HZ8mGafpvTKupeQBdNt6D4HKPnQve69KIcXOwO1/SsO", 1, "superadmin" });

            migrationBuilder.InsertData(
                table: "RolePagePermissions",
                columns: new[] { "PageId", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 1, 2, 1 },
                    { 1, 3, 1 },
                    { 2, 1, 1 },
                    { 2, 2, 1 },
                    { 2, 3, 1 },
                    { 3, 1, 1 },
                    { 3, 2, 1 },
                    { 3, 3, 1 },
                    { 4, 1, 1 },
                    { 4, 2, 1 },
                    { 4, 3, 1 },
                    { 5, 1, 1 },
                    { 5, 2, 1 },
                    { 5, 3, 1 },
                    { 6, 1, 1 },
                    { 6, 2, 1 },
                    { 6, 3, 1 },
                    { 7, 1, 1 },
                    { 7, 2, 1 },
                    { 7, 3, 1 },
                    { 8, 1, 1 },
                    { 8, 2, 1 },
                    { 8, 3, 1 },
                    { 9, 1, 1 },
                    { 9, 2, 1 },
                    { 9, 3, 1 },
                    { 10, 1, 1 },
                    { 10, 2, 1 },
                    { 10, 3, 1 },
                    { 11, 1, 1 },
                    { 11, 2, 1 },
                    { 11, 3, 1 },
                    { 12, 1, 1 },
                    { 12, 2, 1 },
                    { 12, 3, 1 },
                    { 13, 1, 1 },
                    { 13, 2, 1 },
                    { 13, 3, 1 },
                    { 14, 1, 1 },
                    { 14, 2, 1 },
                    { 14, 3, 1 },
                    { 15, 1, 1 },
                    { 15, 2, 1 },
                    { 15, 3, 1 },
                    { 16, 1, 1 },
                    { 16, 2, 1 },
                    { 16, 3, 1 },
                    { 17, 1, 1 },
                    { 17, 2, 1 },
                    { 17, 3, 1 },
                    { 18, 1, 1 },
                    { 18, 2, 1 },
                    { 18, 3, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_SectionId",
                table: "Pages",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_ProductId",
                table: "PurchaseOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId",
                table: "PurchaseOrderItems",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_VendorId",
                table: "PurchaseOrders",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePagePermissions_PageId",
                table: "RolePagePermissions",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePagePermissions_PermissionId",
                table: "RolePagePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_CostCategories_CostCategoryId",
                table: "Products",
                column: "CostCategoryId",
                principalTable: "CostCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Units_UnitId",
                table: "Products",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
