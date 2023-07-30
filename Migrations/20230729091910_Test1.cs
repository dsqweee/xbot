using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XBOT.Migrations
{
    /// <inheritdoc />
    public partial class Test1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscordInvite_ReferralRole_Roles_RolesId",
                table: "DiscordInvite_ReferralRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscordInvite_ReferralRole",
                table: "DiscordInvite_ReferralRole");

            migrationBuilder.RenameTable(
                name: "DiscordInvite_ReferralRole",
                newName: "ReferralRole");

            migrationBuilder.RenameColumn(
                name: "VoiceActive",
                table: "User",
                newName: "voiceActive_public");

            migrationBuilder.RenameIndex(
                name: "IX_DiscordInvite_ReferralRole_RolesId",
                table: "ReferralRole",
                newName: "IX_ReferralRole_RolesId");

            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMessageTime",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "voiceActive_private",
                table: "User",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReferralRole",
                table: "ReferralRole",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PrivateChannel",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateChannel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivateChannel_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1ul,
                columns: new[] { "Prefix", "Status" },
                values: new object[] { "x.", "Prefix: `x.`" });

            migrationBuilder.CreateIndex(
                name: "IX_PrivateChannel_UserId",
                table: "PrivateChannel",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReferralRole_Roles_RolesId",
                table: "ReferralRole",
                column: "RolesId",
                principalTable: "Roles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReferralRole_Roles_RolesId",
                table: "ReferralRole");

            migrationBuilder.DropTable(
                name: "PrivateChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReferralRole",
                table: "ReferralRole");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "User");

            migrationBuilder.DropColumn(
                name: "LastMessageTime",
                table: "User");

            migrationBuilder.DropColumn(
                name: "voiceActive_private",
                table: "User");

            migrationBuilder.RenameTable(
                name: "ReferralRole",
                newName: "DiscordInvite_ReferralRole");

            migrationBuilder.RenameColumn(
                name: "voiceActive_public",
                table: "User",
                newName: "VoiceActive");

            migrationBuilder.RenameIndex(
                name: "IX_ReferralRole_RolesId",
                table: "DiscordInvite_ReferralRole",
                newName: "IX_DiscordInvite_ReferralRole_RolesId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscordInvite_ReferralRole",
                table: "DiscordInvite_ReferralRole",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1ul,
                columns: new[] { "Prefix", "Status" },
                values: new object[] { "v.", "Prefix: `v.`" });

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordInvite_ReferralRole_Roles_RolesId",
                table: "DiscordInvite_ReferralRole",
                column: "RolesId",
                principalTable: "Roles",
                principalColumn: "Id");
        }
    }
}
