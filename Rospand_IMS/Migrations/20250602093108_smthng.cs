using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rospand_IMS.Migrations
{
    /// <inheritdoc />
    public partial class smthng : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInventoryTracked",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInventoryTracked",
                table: "Products");
        }
    }
}
