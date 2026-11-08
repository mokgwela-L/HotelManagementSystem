using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagementSystem1.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Guests",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegistrationDate",
                value: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Guests",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PhoneNumber", "RegistrationDate" },
                values: new object[] { "+0987654321", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Guests",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegistrationDate",
                value: new DateTime(2025, 10, 11, 20, 37, 52, 860, DateTimeKind.Utc).AddTicks(6506));

            migrationBuilder.UpdateData(
                table: "Guests",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PhoneNumber", "RegistrationDate" },
                values: new object[] { "+277678543291", new DateTime(2025, 10, 11, 20, 37, 52, 860, DateTimeKind.Utc).AddTicks(9713) });
        }
    }
}
