using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ringer.HubServer.Migrations
{
    public partial class AddUrlColumntoTermsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Terms",
                nullable: true);

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
                column: "CreatedAt",
                value: new DateTime(2020, 5, 1, 2, 3, 36, 148, DateTimeKind.Utc).AddTicks(7480));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2020, 5, 1, 2, 3, 36, 148, DateTimeKind.Utc).AddTicks(7500));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2020, 5, 1, 2, 3, 36, 148, DateTimeKind.Utc).AddTicks(7510));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2020, 5, 1, 2, 3, 36, 148, DateTimeKind.Utc).AddTicks(7510));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "Terms");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 4, 24, 6, 14, 39, 909, DateTimeKind.Utc).AddTicks(6240), "Admin" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 4, 24, 6, 14, 39, 909, DateTimeKind.Utc).AddTicks(6980), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 4, 24, 6, 14, 39, 909, DateTimeKind.Utc).AddTicks(7000), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 4, 24, 6, 14, 39, 909, DateTimeKind.Utc).AddTicks(7000), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 4, 24, 6, 14, 39, 909, DateTimeKind.Utc).AddTicks(7010), "Consumer" });
        }
    }
}
