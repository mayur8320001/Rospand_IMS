using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rospand_IMS.Migrations
{
    /// <inheritdoc />
    public partial class addCust : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salutation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPersonName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxTypeId = table.Column<int>(type: "int", nullable: true),
                    TRNNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTermId = table.Column<int>(type: "int", nullable: true),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BillingAddressId = table.Column<int>(type: "int", nullable: true),
                    ShippingAddressId = table.Column<int>(type: "int", nullable: true),
                    ShippingSameAsBilling = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Addresses_BillingAddressId",
                        column: x => x.BillingAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Customers_Addresses_ShippingAddressId",
                        column: x => x.ShippingAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Customers_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Customers_PaymentTerms_PaymentTermId",
                        column: x => x.PaymentTermId,
                        principalTable: "PaymentTerms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Customers_TaxTypes_TaxTypeId",
                        column: x => x.TaxTypeId,
                        principalTable: "TaxTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_BillingAddressId",
                table: "Customers",
                column: "BillingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CurrencyId",
                table: "Customers",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PaymentTermId",
                table: "Customers",
                column: "PaymentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ShippingAddressId",
                table: "Customers",
                column: "ShippingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TaxTypeId",
                table: "Customers",
                column: "TaxTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
