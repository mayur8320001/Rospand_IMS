using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rospand_IMS.Migrations
{
    /// <inheritdoc />
    public partial class Siup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_States_StateCode",
                table: "Cities");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_States_StateCode",
                table: "States");

            migrationBuilder.RenameColumn(
                name: "StateCode",
                table: "States",
                newName: "StateId");

            migrationBuilder.RenameIndex(
                name: "IX_States_StateCode",
                table: "States",
                newName: "IX_States_StateId");

            migrationBuilder.RenameColumn(
                name: "StateCode",
                table: "Cities",
                newName: "StateId");

            migrationBuilder.RenameIndex(
                name: "IX_Cities_StateCode",
                table: "Cities",
                newName: "IX_Cities_StateId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_States_StateId",
                table: "States",
                column: "StateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_States_StateId",
                table: "Cities",
                column: "StateId",
                principalTable: "States",
                principalColumn: "StateId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_States_StateId",
                table: "Cities");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_States_StateId",
                table: "States");

            migrationBuilder.RenameColumn(
                name: "StateId",
                table: "States",
                newName: "StateCode");

            migrationBuilder.RenameIndex(
                name: "IX_States_StateId",
                table: "States",
                newName: "IX_States_StateCode");

            migrationBuilder.RenameColumn(
                name: "StateId",
                table: "Cities",
                newName: "StateCode");

            migrationBuilder.RenameIndex(
                name: "IX_Cities_StateId",
                table: "Cities",
                newName: "IX_Cities_StateCode");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_States_StateCode",
                table: "States",
                column: "StateCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_States_StateCode",
                table: "Cities",
                column: "StateCode",
                principalTable: "States",
                principalColumn: "StateCode",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
