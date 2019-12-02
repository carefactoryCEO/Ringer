using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ringer.HubServer.Migrations
{
    public partial class Initialmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    IsClosed = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    UserType = table.Column<string>(nullable: false, defaultValue: "Consumer"),
                    BirthDate = table.Column<DateTime>(nullable: false),
                    Gender = table.Column<string>(nullable: false),
                    PhoneNumber = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Device",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DeviceType = table.Column<string>(nullable: false),
                    IsOn = table.Column<bool>(nullable: false),
                    ConnectionId = table.Column<string>(nullable: true),
                    OwnerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Device", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Device_User_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrollment",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    RoomId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollment_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollment_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Body = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    SenderId = table.Column<int>(nullable: false),
                    RoomId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Message_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Message_User_SenderId",
                        column: x => x.SenderId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "BirthDate", "CreatedAt", "Email", "Gender", "Name", "Password", "PhoneNumber", "UserType" },
                values: new object[,]
                {
                    { 1, new DateTime(1976, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 12, 2, 4, 59, 5, 883, DateTimeKind.Local).AddTicks(9520), null, "Male", "Admin", null, null, "Admin" },
                    { 2, new DateTime(1976, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 12, 2, 4, 59, 5, 885, DateTimeKind.Local).AddTicks(7960), null, "Male", "신모범", null, null, "Consumer" },
                    { 3, new DateTime(1981, 6, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 12, 2, 4, 59, 5, 885, DateTimeKind.Local).AddTicks(7980), null, "Female", "김은미", null, null, "Consumer" },
                    { 4, new DateTime(1980, 7, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 12, 2, 4, 59, 5, 885, DateTimeKind.Local).AddTicks(8040), null, "Male", "김순용", null, null, "Consumer" },
                    { 5, new DateTime(1981, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 12, 2, 4, 59, 5, 885, DateTimeKind.Local).AddTicks(8040), null, "Female", "함주희", null, null, "Consumer" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Device_OwnerId",
                table: "Device",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_RoomId",
                table: "Enrollment",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_UserId",
                table: "Enrollment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_RoomId",
                table: "Message",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_SenderId",
                table: "Message",
                column: "SenderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Device");

            migrationBuilder.DropTable(
                name: "Enrollment");

            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "Room");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
