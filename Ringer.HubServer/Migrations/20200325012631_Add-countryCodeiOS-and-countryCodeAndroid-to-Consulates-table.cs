using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ringer.HubServer.Migrations
{
    public partial class AddcountryCodeiOSandcountryCodeAndroidtoConsulatestable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryCodeAndroid",
                table: "Consulate",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCodeiOS",
                table: "Consulate",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 3, 25, 1, 26, 31, 472, DateTimeKind.Utc).AddTicks(6950), "Admin" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2020, 3, 25, 1, 26, 31, 472, DateTimeKind.Utc).AddTicks(7810));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2020, 3, 25, 1, 26, 31, 472, DateTimeKind.Utc).AddTicks(7830));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2020, 3, 25, 1, 26, 31, 472, DateTimeKind.Utc).AddTicks(7840));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2020, 3, 25, 1, 26, 31, 472, DateTimeKind.Utc).AddTicks(7840));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryCodeAndroid",
                table: "Consulate");

            migrationBuilder.DropColumn(
                name: "CountryCodeiOS",
                table: "Consulate");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 3, 25, 1, 13, 38, 109, DateTimeKind.Utc).AddTicks(5680), "Admin" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 3, 25, 1, 13, 38, 109, DateTimeKind.Utc).AddTicks(6520), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 3, 25, 1, 13, 38, 109, DateTimeKind.Utc).AddTicks(6540), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 3, 25, 1, 13, 38, 109, DateTimeKind.Utc).AddTicks(6550), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 3, 25, 1, 13, 38, 109, DateTimeKind.Utc).AddTicks(6550), "Consumer" });
        }
    }
}
