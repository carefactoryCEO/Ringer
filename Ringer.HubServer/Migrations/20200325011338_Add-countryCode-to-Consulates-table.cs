using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ringer.HubServer.Migrations
{
    public partial class AddcountryCodetoConsulatestable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Consulate",
                nullable: true);

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
                column: "CreatedAt",
                value: new DateTime(2020, 3, 25, 1, 13, 38, 109, DateTimeKind.Utc).AddTicks(6520));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2020, 3, 25, 1, 13, 38, 109, DateTimeKind.Utc).AddTicks(6540));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2020, 3, 25, 1, 13, 38, 109, DateTimeKind.Utc).AddTicks(6550));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2020, 3, 25, 1, 13, 38, 109, DateTimeKind.Utc).AddTicks(6550));

            migrationBuilder.CreateIndex(
                name: "IX_Consulate_CountryCode",
                table: "Consulate",
                column: "CountryCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consulate_CountryCode",
                table: "Consulate");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Consulate");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 2, 28, 16, 59, 17, 148, DateTimeKind.Utc).AddTicks(3200), "Admin" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 2, 28, 16, 59, 17, 148, DateTimeKind.Utc).AddTicks(3980), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 2, 28, 16, 59, 17, 148, DateTimeKind.Utc).AddTicks(3990), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 2, 28, 16, 59, 17, 148, DateTimeKind.Utc).AddTicks(4000), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 2, 28, 16, 59, 17, 148, DateTimeKind.Utc).AddTicks(4000), "Consumer" });
        }
    }
}
