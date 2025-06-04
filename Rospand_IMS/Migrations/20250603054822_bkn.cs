using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rospand_IMS.Migrations
{
    /// <inheritdoc />
    public partial class bkn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "OutwardEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NewQuantity",
                table: "InventoryTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OutwardEntryId",
                table: "InventoryTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousQuantity",
                table: "InventoryTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastTransactionId",
                table: "Inventories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_OutwardEntryId",
                table: "InventoryTransactions",
                column: "OutwardEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_OutwardEntries_OutwardEntryId",
                table: "InventoryTransactions",
                column: "OutwardEntryId",
                principalTable: "OutwardEntries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_OutwardEntries_OutwardEntryId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_OutwardEntryId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OutwardEntries");

            migrationBuilder.DropColumn(
                name: "NewQuantity",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "OutwardEntryId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "PreviousQuantity",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "LastTransactionId",
                table: "Inventories");
        }
    }
}
