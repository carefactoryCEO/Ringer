using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ringer.HubServer.Migrations
{
    public partial class MakeRoomIdRequiredToMessageAndEnrollment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_Room_RoomId",
                table: "Enrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_Room_RoomId",
                table: "Message");

            migrationBuilder.AlterColumn<string>(
                name: "RoomId",
                table: "Message",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RoomId",
                table: "Enrollment",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

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
                column: "CreatedAt",
                value: new DateTime(2020, 4, 24, 6, 14, 39, 909, DateTimeKind.Utc).AddTicks(6980));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2020, 4, 24, 6, 14, 39, 909, DateTimeKind.Utc).AddTicks(7000));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2020, 4, 24, 6, 14, 39, 909, DateTimeKind.Utc).AddTicks(7000));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2020, 4, 24, 6, 14, 39, 909, DateTimeKind.Utc).AddTicks(7010));

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_Room_RoomId",
                table: "Enrollment",
                column: "RoomId",
                principalTable: "Room",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_Room_RoomId",
                table: "Message",
                column: "RoomId",
                principalTable: "Room",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_Room_RoomId",
                table: "Enrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_Room_RoomId",
                table: "Message");

            migrationBuilder.AlterColumn<string>(
                name: "RoomId",
                table: "Message",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "RoomId",
                table: "Enrollment",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 4, 22, 10, 33, 20, 362, DateTimeKind.Utc).AddTicks(2820), "Admin" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 4, 22, 10, 33, 20, 362, DateTimeKind.Utc).AddTicks(3680), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 4, 22, 10, 33, 20, 362, DateTimeKind.Utc).AddTicks(3700), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 4, 22, 10, 33, 20, 362, DateTimeKind.Utc).AddTicks(3700), "Consumer" });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2020, 4, 22, 10, 33, 20, 362, DateTimeKind.Utc).AddTicks(3710), "Consumer" });

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_Room_RoomId",
                table: "Enrollment",
                column: "RoomId",
                principalTable: "Room",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_Room_RoomId",
                table: "Message",
                column: "RoomId",
                principalTable: "Room",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
