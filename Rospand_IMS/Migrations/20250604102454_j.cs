using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rospand_IMS.Migrations
{
    /// <inheritdoc />
    public partial class j : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OutwardEntryItems_SalesOrderItems_SalesOrderItemId",
                table: "OutwardEntryItems");

            migrationBuilder.DropIndex(
                name: "IX_OutwardEntryItems_SalesOrderItemId",
                table: "OutwardEntryItems");

            migrationBuilder.DropColumn(
                name: "SalesOrderItemId",
                table: "OutwardEntryItems");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "OutwardEntries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesOrderItemId",
                table: "OutwardEntryItems",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "OutwardEntries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_OutwardEntryItems_SalesOrderItemId",
                table: "OutwardEntryItems",
                column: "SalesOrderItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_OutwardEntryItems_SalesOrderItems_SalesOrderItemId",
                table: "OutwardEntryItems",
                column: "SalesOrderItemId",
                principalTable: "SalesOrderItems",
                principalColumn: "Id");
        }
    }
}
