using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ringer.HubServer.Migrations
{
    public partial class AddadditionalcolumnstoConsulatetable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Consulate",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Consulate",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkingTime",
                table: "Consulate",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 5, 2, 1, 35, 22, 370, DateTimeKind.Utc).AddTicks(4470), "Admin" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2020, 5, 2, 1, 35, 22, 370, DateTimeKind.Utc).AddTicks(5400));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2020, 5, 2, 1, 35, 22, 370, DateTimeKind.Utc).AddTicks(5420));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2020, 5, 2, 1, 35, 22, 370, DateTimeKind.Utc).AddTicks(5420));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2020, 5, 2, 1, 35, 22, 370, DateTimeKind.Utc).AddTicks(5430));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Consulate");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Consulate");

            migrationBuilder.DropColumn(
                name: "WorkingTime",
                table: "Consulate");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 5, 1, 2, 3, 36, 148, DateTimeKind.Utc).AddTicks(6540), "Admin" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 5, 1, 2, 3, 36, 148, DateTimeKind.Utc).AddTicks(7480), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 5, 1, 2, 3, 36, 148, DateTimeKind.Utc).AddTicks(7500), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 5, 1, 2, 3, 36, 148, DateTimeKind.Utc).AddTicks(7510), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 5, 1, 2, 3, 36, 148, DateTimeKind.Utc).AddTicks(7510), "Consumer" });
        }
    }
}
