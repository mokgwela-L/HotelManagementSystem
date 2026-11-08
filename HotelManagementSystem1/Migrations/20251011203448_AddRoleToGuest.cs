using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagementSystem1.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleToGuest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Guests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Guests");
        }
    }
}
