using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rospand_IMS.Migrations
{
    /// <inheritdoc />
    public partial class outE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutwardEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OutwardNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SalesOrderId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    OutwardDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutwardEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutwardEntries_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OutwardEntries_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutwardEntryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OutwardEntryId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutwardEntryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutwardEntryItems_OutwardEntries_OutwardEntryId",
                        column: x => x.OutwardEntryId,
                        principalTable: "OutwardEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OutwardEntryItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutwardEntries_SalesOrderId",
                table: "OutwardEntries",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OutwardEntries_WarehouseId",
                table: "OutwardEntries",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_OutwardEntryItems_OutwardEntryId",
                table: "OutwardEntryItems",
                column: "OutwardEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_OutwardEntryItems_ProductId",
                table: "OutwardEntryItems",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutwardEntryItems");

            migrationBuilder.DropTable(
                name: "OutwardEntries");
        }
    }
}
