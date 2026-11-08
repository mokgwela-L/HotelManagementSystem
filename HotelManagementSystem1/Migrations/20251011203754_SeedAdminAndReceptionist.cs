using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HotelManagementSystem1.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminAndReceptionist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Guests",
                columns: new[] { "Id", "Address", "Email", "FirstName", "LastName", "PasswordHash", "PhoneNumber", "RegistrationDate", "Role" },
                values: new object[,]
                {
                    { 1, "Hotel Office", "admin@hotel.com", "Admin", "User", "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=", "+1234567890", new DateTime(2025, 10, 11, 20, 37, 52, 860, DateTimeKind.Utc).AddTicks(6506), "Admin" },
                    { 2, "Hotel Front Desk", "reception@hotel.com", "Reception", "Desk", "UUXbo7a9otYQ0sXENaHCSB7v0xRran4AStc/eUOG4DE=", "+277678543291", new DateTime(2025, 10, 11, 20, 37, 52, 860, DateTimeKind.Utc).AddTicks(9713), "Receptionist" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Guests",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Guests",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
