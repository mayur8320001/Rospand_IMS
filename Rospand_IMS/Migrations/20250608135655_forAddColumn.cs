using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rospand_IMS.Migrations
{
    /// <inheritdoc />
    public partial class forAddColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "OutwardEntries");

            migrationBuilder.AddColumn<int>(
                name: "NewReservedQuantity",
                table: "InventoryTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousReservedQuantity",
                table: "InventoryTransactions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewReservedQuantity",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "PreviousReservedQuantity",
                table: "InventoryTransactions");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "OutwardEntries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
