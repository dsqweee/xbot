using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XBOT.Migrations
{
    /// <inheritdoc />
    public partial class test2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlockReason",
                table: "User",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "User_Permission_Id",
                table: "User",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "delUrl",
                table: "TextChannel",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "delUrlImage",
                table: "TextChannel",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "giveXp",
                table: "TextChannel",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "inviteLink",
                table: "TextChannel",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "useAdminCommand",
                table: "TextChannel",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "useCommand",
                table: "TextChannel",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "useRPcommand",
                table: "TextChannel",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Guild_Warn",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CountWarn = table.Column<byte>(type: "INTEGER", nullable: false),
                    Time = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    ReportTypes = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guild_Warn", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User_Permission",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Unlimited = table.Column<bool>(type: "INTEGER", nullable: false),
                    CountWarn = table.Column<byte>(type: "INTEGER", nullable: false),
                    User_Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Permission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Permission_User_User_Id",
                        column: x => x.User_Id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User_UnWarn",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Warn_Id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Admin_Id = table.Column<ulong>(type: "INTEGER", nullable: true),
                    AdminId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    ReviewAdd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndStatusSet = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_UnWarn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_UnWarn_User_Permission_AdminId",
                        column: x => x.AdminId,
                        principalTable: "User_Permission",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "User_Warn",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Guild_Warns_Id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Guild_WarnsId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    UnWarn_Id = table.Column<ulong>(type: "INTEGER", nullable: true),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Admin_Id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    AdminId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    TimeSetWarn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ToTimeWarn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WarnSkippedAfterUnban = table.Column<bool>(type: "INTEGER", nullable: false),
                    WarnSkippedBecauseNewTimeWarn = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Warn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Warn_Guild_Warn_Guild_WarnsId",
                        column: x => x.Guild_WarnsId,
                        principalTable: "Guild_Warn",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_Warn_User_Permission_AdminId",
                        column: x => x.AdminId,
                        principalTable: "User_Permission",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_Warn_User_UnWarn_UnWarn_Id",
                        column: x => x.UnWarn_Id,
                        principalTable: "User_UnWarn",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_Warn_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_Permission_User_Id",
                table: "User_Permission",
                column: "User_Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_UnWarn_AdminId",
                table: "User_UnWarn",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Warn_AdminId",
                table: "User_Warn",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Warn_Guild_WarnsId",
                table: "User_Warn",
                column: "Guild_WarnsId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Warn_UnWarn_Id",
                table: "User_Warn",
                column: "UnWarn_Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Warn_UserId",
                table: "User_Warn",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "User_Warn");

            migrationBuilder.DropTable(
                name: "Guild_Warn");

            migrationBuilder.DropTable(
                name: "User_UnWarn");

            migrationBuilder.DropTable(
                name: "User_Permission");

            migrationBuilder.DropColumn(
                name: "BlockReason",
                table: "User");

            migrationBuilder.DropColumn(
                name: "User_Permission_Id",
                table: "User");

            migrationBuilder.DropColumn(
                name: "delUrl",
                table: "TextChannel");

            migrationBuilder.DropColumn(
                name: "delUrlImage",
                table: "TextChannel");

            migrationBuilder.DropColumn(
                name: "giveXp",
                table: "TextChannel");

            migrationBuilder.DropColumn(
                name: "inviteLink",
                table: "TextChannel");

            migrationBuilder.DropColumn(
                name: "useAdminCommand",
                table: "TextChannel");

            migrationBuilder.DropColumn(
                name: "useCommand",
                table: "TextChannel");

            migrationBuilder.DropColumn(
                name: "useRPcommand",
                table: "TextChannel");
        }
    }
}
