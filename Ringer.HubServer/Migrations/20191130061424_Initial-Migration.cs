using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ringer.HubServer.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
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
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    UserType = table.Column<string>(nullable: false, defaultValue: "Consumer"),
                    BirthDate = table.Column<DateTime>(nullable: false),
                    Gender = table.Column<string>(nullable: false),
                    PhoneNumber = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    IsOn = table.Column<bool>(nullable: false),
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
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(nullable: false),
                    RoomId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollment_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(nullable: true),
                    Sender = table.Column<string>(nullable: true),
                    SenderId = table.Column<int>(nullable: false),
                    Room = table.Column<string>(nullable: true),
                    RoomId = table.Column<int>(nullable: false),
                    DeviceId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Message_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Room",
                columns: new[] { "Id", "IsClosed", "Name" },
                values: new object[] { 1, false, "김순용" });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "BirthDate", "CreatedAt", "Email", "Gender", "IsOn", "Name", "Password", "PhoneNumber", "UserType" },
                values: new object[] { 1, new DateTime(1976, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 11, 30, 15, 14, 24, 297, DateTimeKind.Local).AddTicks(7170), null, "Male", false, "Admin", null, null, "Admin" });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "BirthDate", "CreatedAt", "Email", "Gender", "IsOn", "Name", "Password", "PhoneNumber" },
                values: new object[] { 2, new DateTime(1976, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 11, 30, 15, 14, 24, 299, DateTimeKind.Local).AddTicks(9970), null, "Male", false, "신모범", null, null });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "BirthDate", "CreatedAt", "Email", "Gender", "IsOn", "Name", "Password", "PhoneNumber" },
                values: new object[] { 3, new DateTime(1981, 6, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 11, 30, 15, 14, 24, 300, DateTimeKind.Local).AddTicks(10), null, "Female", false, "김은미", null, null });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "BirthDate", "CreatedAt", "Email", "Gender", "IsOn", "Name", "Password", "PhoneNumber" },
                values: new object[] { 4, new DateTime(1980, 7, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 11, 30, 15, 14, 24, 300, DateTimeKind.Local).AddTicks(20), null, "Male", false, "김순용", null, null });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "BirthDate", "CreatedAt", "Email", "Gender", "IsOn", "Name", "Password", "PhoneNumber" },
                values: new object[] { 5, new DateTime(1981, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 11, 30, 15, 14, 24, 300, DateTimeKind.Local).AddTicks(20), null, "Female", false, "함주희", null, null });

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
                name: "IX_Message_DeviceId",
                table: "Message",
                column: "DeviceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrollment");

            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "Room");

            migrationBuilder.DropTable(
                name: "Device");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
