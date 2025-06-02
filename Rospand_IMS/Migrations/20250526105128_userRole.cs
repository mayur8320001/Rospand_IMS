using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rospand_IMS.Migrations
{
    /// <inheritdoc />
    public partial class userRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false)
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
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
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
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePagePermissions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Pages");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Sections");
        }
    }
}
