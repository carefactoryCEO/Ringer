using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ringer.HubServer.Migrations
{
    public partial class AddindexoncountryCodeiOSandcountryCodeAndroidtoConsulatestable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consulate_CountryCode",
                table: "Consulate");

            migrationBuilder.AlterColumn<string>(
                name: "CountryCodeiOS",
                table: "Consulate",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CountryCodeAndroid",
                table: "Consulate",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 3, 25, 1, 29, 49, 520, DateTimeKind.Utc).AddTicks(6100), "Admin" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2020, 3, 25, 1, 29, 49, 520, DateTimeKind.Utc).AddTicks(6950));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2020, 3, 25, 1, 29, 49, 520, DateTimeKind.Utc).AddTicks(6970));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2020, 3, 25, 1, 29, 49, 520, DateTimeKind.Utc).AddTicks(6970));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2020, 3, 25, 1, 29, 49, 520, DateTimeKind.Utc).AddTicks(6980));

            migrationBuilder.CreateIndex(
                name: "IX_Consulate_CountryCode_CountryCodeiOS_CountryCodeAndroid",
                table: "Consulate",
                columns: new[] { "CountryCode", "CountryCodeiOS", "CountryCodeAndroid" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consulate_CountryCode_CountryCodeiOS_CountryCodeAndroid",
                table: "Consulate");

            migrationBuilder.AlterColumn<string>(
                name: "CountryCodeiOS",
                table: "Consulate",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CountryCodeAndroid",
                table: "Consulate",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

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
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 3, 25, 1, 26, 31, 472, DateTimeKind.Utc).AddTicks(7810), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 3, 25, 1, 26, 31, 472, DateTimeKind.Utc).AddTicks(7830), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 3, 25, 1, 26, 31, 472, DateTimeKind.Utc).AddTicks(7840), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 3, 25, 1, 26, 31, 472, DateTimeKind.Utc).AddTicks(7840), "Consumer" });

            migrationBuilder.CreateIndex(
                name: "IX_Consulate_CountryCode",
                table: "Consulate",
                column: "CountryCode");
        }
    }
}
