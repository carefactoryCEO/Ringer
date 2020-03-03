using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ringer.HubServer.Migrations
{
    public partial class modifycontrynametocountry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryName",
                table: "Consulate");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Consulate",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2020, 2, 28, 16, 59, 17, 148, DateTimeKind.Utc).AddTicks(3200));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2020, 2, 28, 16, 59, 17, 148, DateTimeKind.Utc).AddTicks(3980));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2020, 2, 28, 16, 59, 17, 148, DateTimeKind.Utc).AddTicks(3990));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2020, 2, 28, 16, 59, 17, 148, DateTimeKind.Utc).AddTicks(4000));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2020, 2, 28, 16, 59, 17, 148, DateTimeKind.Utc).AddTicks(4000));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Consulate");

            migrationBuilder.AddColumn<string>(
                name: "CountryName",
                table: "Consulate",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2020, 2, 28, 13, 46, 50, 94, DateTimeKind.Utc).AddTicks(5620));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2020, 2, 28, 13, 46, 50, 94, DateTimeKind.Utc).AddTicks(6360));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2020, 2, 28, 13, 46, 50, 94, DateTimeKind.Utc).AddTicks(6370));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2020, 2, 28, 13, 46, 50, 94, DateTimeKind.Utc).AddTicks(6380));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2020, 2, 28, 13, 46, 50, 94, DateTimeKind.Utc).AddTicks(6380));
        }
    }
}
