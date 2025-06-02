using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rospand_IMS.Migrations
{
    /// <inheritdoc />
    public partial class forVenOp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Roles_Name",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Pages_Controller_Action",
                table: "Pages");

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "SKU",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RatePer",
                table: "Products",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumStockLevel",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReorderQuantity",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Controller",
                table: "Pages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "Pages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecurringBills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    BillPrefix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: true),
                    DayOfMonth = table.Column<int>(type: "int", nullable: true),
                    MonthOfYear = table.Column<int>(type: "int", nullable: true),
                    PaymentTermId = table.Column<int>(type: "int", nullable: true),
                    Memo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringBills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringBills_PaymentTerms_PaymentTermId",
                        column: x => x.PaymentTermId,
                        principalTable: "PaymentTerms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RecurringBills_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecurringExpenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: true),
                    DayOfMonth = table.Column<int>(type: "int", nullable: true),
                    MonthOfYear = table.Column<int>(type: "int", nullable: true),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    ExpenseCategoryId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringExpenses_ExpenseCategories_ExpenseCategoryId",
                        column: x => x.ExpenseCategoryId,
                        principalTable: "ExpenseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecurringExpenses_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecurringExpenses_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RecurringExpenses_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    BillNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentTermId = table.Column<int>(type: "int", nullable: true),
                    Memo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    RecurringBillId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bills_PaymentTerms_PaymentTermId",
                        column: x => x.PaymentTermId,
                        principalTable: "PaymentTerms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Bills_RecurringBills_RecurringBillId",
                        column: x => x.RecurringBillId,
                        principalTable: "RecurringBills",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Bills_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecurringBillLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecurringBillId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxTypeId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringBillLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringBillLineItems_RecurringBills_RecurringBillId",
                        column: x => x.RecurringBillId,
                        principalTable: "RecurringBills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecurringBillLineItems_TaxTypes_TaxTypeId",
                        column: x => x.TaxTypeId,
                        principalTable: "TaxTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    ExpenseCategoryId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    RecurringExpenseId = table.Column<int>(type: "int", nullable: true),
                    AttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_ExpenseCategories_ExpenseCategoryId",
                        column: x => x.ExpenseCategoryId,
                        principalTable: "ExpenseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Expenses_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Expenses_RecurringExpenses_RecurringExpenseId",
                        column: x => x.RecurringExpenseId,
                        principalTable: "RecurringExpenses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Expenses_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Expenses_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxTypeId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillLineItems_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillLineItems_TaxTypes_TaxTypeId",
                        column: x => x.TaxTypeId,
                        principalTable: "TaxTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentsMade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Memo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BillId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsMade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentsMade_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentsMade_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentsMade_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
     name: "BillPayments",
     columns: table => new
     {
         Id = table.Column<int>(type: "int", nullable: false)
             .Annotation("SqlServer:Identity", "1, 1"),
         PaymentMadeId = table.Column<int>(type: "int", nullable: false),
         BillId = table.Column<int>(type: "int", nullable: false),
         Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
     },
     constraints: table =>
     {
         table.PrimaryKey("PK_BillPayments", x => x.Id);
         table.ForeignKey(
             name: "FK_BillPayments_Bills_BillId",
             column: x => x.BillId,
             principalTable: "Bills",
             principalColumn: "Id",
             onDelete: ReferentialAction.Cascade); // Keep cascade delete here
         table.ForeignKey(
             name: "FK_BillPayments_PaymentsMade_PaymentMadeId",
             column: x => x.PaymentMadeId,
             principalTable: "PaymentsMade",
             principalColumn: "Id",
             onDelete: ReferentialAction.NoAction); // Change to NoAction here
     });
            migrationBuilder.CreateIndex(
                name: "IX_BillLineItems_BillId",
                table: "BillLineItems",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_BillLineItems_TaxTypeId",
                table: "BillLineItems",
                column: "TaxTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BillPayments_BillId",
                table: "BillPayments",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_BillPayments_PaymentMadeId",
                table: "BillPayments",
                column: "PaymentMadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_PaymentTermId",
                table: "Bills",
                column: "PaymentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_RecurringBillId",
                table: "Bills",
                column: "RecurringBillId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_VendorId",
                table: "Bills",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ExpenseCategoryId",
                table: "Expenses",
                column: "ExpenseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_PaymentMethodId",
                table: "Expenses",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_RecurringExpenseId",
                table: "Expenses",
                column: "RecurringExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_TaxId",
                table: "Expenses",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_VendorId",
                table: "Expenses",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsMade_BillId",
                table: "PaymentsMade",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsMade_PaymentMethodId",
                table: "PaymentsMade",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsMade_VendorId",
                table: "PaymentsMade",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringBillLineItems_RecurringBillId",
                table: "RecurringBillLineItems",
                column: "RecurringBillId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringBillLineItems_TaxTypeId",
                table: "RecurringBillLineItems",
                column: "TaxTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringBills_PaymentTermId",
                table: "RecurringBills",
                column: "PaymentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringBills_VendorId",
                table: "RecurringBills",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringExpenses_ExpenseCategoryId",
                table: "RecurringExpenses",
                column: "ExpenseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringExpenses_PaymentMethodId",
                table: "RecurringExpenses",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringExpenses_TaxId",
                table: "RecurringExpenses",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringExpenses_VendorId",
                table: "RecurringExpenses",
                column: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillLineItems");

            migrationBuilder.DropTable(
                name: "BillPayments");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "RecurringBillLineItems");

            migrationBuilder.DropTable(
                name: "PaymentsMade");

            migrationBuilder.DropTable(
                name: "RecurringExpenses");

            migrationBuilder.DropTable(
                name: "Bills");

            migrationBuilder.DropTable(
                name: "ExpenseCategories");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "RecurringBills");

            migrationBuilder.DropColumn(
                name: "MinimumStockLevel",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReorderQuantity",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Roles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SKU",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "RatePer",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "Products",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Controller",
                table: "Pages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "Pages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "Pages",
                columns: new[] { "Id", "Action", "Controller", "MenuGroup", "Name" },
                values: new object[,]
                {
                    { 1, "Index", "Category", "Master Pages", "Category Master" },
                    { 2, "Index", "Product", "Master Pages", "Product Master" },
                    { 3, "Index", "Customer", "Master Pages", "Customer Master" },
                    { 4, "Index", "Supplier", "Master Pages", "Supplier Master" },
                    { 5, "Index", "InwardEntry", "Operation", "Inward Entries" },
                    { 6, "Index", "OutwardEntry", "Operation", "Outward Entries" },
                    { 7, "Index", "SalesOrder", "Operation", "Sales Orders" },
                    { 8, "Index", "PurchaseOrder", "Operation", "Purchase Orders" },
                    { 9, "Index", "User", "User Management", "Manage Users" },
                    { 10, "Index", "Role", "User Management", "Manage Roles" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pages_Controller_Action",
                table: "Pages",
                columns: new[] { "Controller", "Action" },
                unique: true);
        }
    }
}
