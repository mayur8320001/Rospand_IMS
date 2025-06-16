using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rospand_IMS.Migrations
{
    /// <inheritdoc />
    public partial class upCSTab : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vendors_Addresses_BillingAddressId",
                table: "Vendors");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendors_Addresses_ShippingAddressId",
                table: "Vendors");

            migrationBuilder.AddColumn<string>(
                name: "StateCode",
                table: "Cities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendors_Addresses_BillingAddressId",
                table: "Vendors",
                column: "BillingAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendors_Addresses_ShippingAddressId",
                table: "Vendors",
                column: "ShippingAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vendors_Addresses_BillingAddressId",
                table: "Vendors");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendors_Addresses_ShippingAddressId",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "StateCode",
                table: "Cities");

            migrationBuilder.AddForeignKey(
                name: "FK_Vendors_Addresses_BillingAddressId",
                table: "Vendors",
                column: "BillingAddressId",
                principalTable: "Addresses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vendors_Addresses_ShippingAddressId",
                table: "Vendors",
                column: "ShippingAddressId",
                principalTable: "Addresses",
                principalColumn: "Id");
        }
    }
}
