using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ringer.HubServer.Migrations
{
    public partial class InitialCreate : Migration
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
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    UserType = table.Column<string>(nullable: false, defaultValue: "Consumer"),
                    BirthDate = table.Column<DateTime>(nullable: false),
                    Gender = table.Column<string>(nullable: false),
                    PhoneNumber = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<byte[]>(nullable: true),
                    PasswordSalt = table.Column<byte[]>(nullable: true),
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
                        .Annotation("Sqlite:Autoincrement", true),
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
                columns: new[] { "Id", "BirthDate", "CreatedAt", "Email", "Gender", "Name", "Password", "PasswordHash", "PasswordSalt", "PhoneNumber", "UserType" },
                values: new object[] { 1, new DateTime(1976, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 12, 30, 15, 2, 0, 922, DateTimeKind.Utc).AddTicks(7170), null, "Male", "Admin", null, null, null, null, "Admin" });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "BirthDate", "CreatedAt", "Email", "Gender", "Name", "Password", "PasswordHash", "PasswordSalt", "PhoneNumber" },
                values: new object[] { 2, new DateTime(1976, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 12, 30, 15, 2, 0, 922, DateTimeKind.Utc).AddTicks(7990), null, "Male", "신모범", null, null, null, null });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "BirthDate", "CreatedAt", "Email", "Gender", "Name", "Password", "PasswordHash", "PasswordSalt", "PhoneNumber" },
                values: new object[] { 3, new DateTime(1981, 6, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 12, 30, 15, 2, 0, 922, DateTimeKind.Utc).AddTicks(8010), null, "Female", "김은미", null, null, null, null });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "BirthDate", "CreatedAt", "Email", "Gender", "Name", "Password", "PasswordHash", "PasswordSalt", "PhoneNumber" },
                values: new object[] { 4, new DateTime(1980, 7, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 12, 30, 15, 2, 0, 922, DateTimeKind.Utc).AddTicks(8020), null, "Male", "김순용", null, null, null, null });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "BirthDate", "CreatedAt", "Email", "Gender", "Name", "Password", "PasswordHash", "PasswordSalt", "PhoneNumber" },
                values: new object[] { 5, new DateTime(1981, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 12, 30, 15, 2, 0, 922, DateTimeKind.Utc).AddTicks(8020), null, "Female", "함주희", null, null, null, null });

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
