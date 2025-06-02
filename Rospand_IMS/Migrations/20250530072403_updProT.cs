using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rospand_IMS.Migrations
{
    /// <inheritdoc />
    public partial class updProT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DimensionUnit",
                table: "Products",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DimensionUnit",
                table: "Products");
        }
    }
}
