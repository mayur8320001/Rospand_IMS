using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rospand_IMS.Migrations
{
    /// <inheritdoc />
    public partial class userSup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "RoleId", "Username" },
                values: new object[] { 1, "$2a$11$G2wB8fX77/HZ8mGafpvTKupeQBdNt6D4HKPnQve69KIcXOwO1/SsO", 1, "superadmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
