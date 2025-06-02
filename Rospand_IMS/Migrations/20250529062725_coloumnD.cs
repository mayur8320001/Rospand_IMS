using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rospand_IMS.Migrations
{
    /// <inheritdoc />
    public partial class coloumnD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LineTotal",
                table: "PurchaseOrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CancelledBy",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledDate",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderedBy",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "OrderedDate",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceivedBy",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedDate",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedBy",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedDate",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "PurchaseOrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CancelledDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "OrderedBy",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "OrderedDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ReceivedBy",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ReceivedDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "SubmittedBy",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "SubmittedDate",
                table: "PurchaseOrders");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "PurchaseOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "PurchaseOrderItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "LineTotal",
                table: "PurchaseOrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
