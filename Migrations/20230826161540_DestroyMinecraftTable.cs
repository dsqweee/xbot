using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XBOT.Migrations
{
    /// <inheritdoc />
    public partial class DestroyMinecraftTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        //    migrationBuilder.DropTable(
        //        name: "QiwiTransactions");

            //migrationBuilder.DropTable(
            //    name: "User_MinecraftAccount");

            //migrationBuilder.DropColumn(
            //    name: "MinecraftAccountId",
            //    table: "User");

            migrationBuilder.DropColumn(
                name: "MinecraftOpen",
                table: "Settings");

            //migrationBuilder.DropColumn(
            //    name: "minecraft_IP",
            //    table: "Settings");

            //migrationBuilder.DropColumn(
            //    name: "minecraft_Key",
            //    table: "Settings");

            //migrationBuilder.DropColumn(
            //    name: "minecraft_port",
            //    table: "Settings");

            //migrationBuilder.AddColumn<bool>(
            //    name: "IsDisable",
            //    table: "EmojiGift_emojiadded",
            //    type: "INTEGER",
            //    nullable: false,
            //    defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDisable",
                table: "EmojiGift_emojiadded");

            migrationBuilder.AddColumn<ulong>(
                name: "MinecraftAccountId",
                table: "User",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MinecraftOpen",
                table: "Settings",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "minecraft_IP",
                table: "Settings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "minecraft_Key",
                table: "Settings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<ushort>(
                name: "minecraft_port",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: (ushort)0);

            migrationBuilder.CreateTable(
                name: "QiwiTransactions",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    discord_id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    invoice_ammount = table.Column<double>(type: "REAL", nullable: false),
                    invoice_date_add = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QiwiTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User_MinecraftAccount",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    LicenceTo = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MinecraftName = table.Column<string>(type: "TEXT", nullable: true),
                    whitelistAdded = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_MinecraftAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_MinecraftAccount_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1ul,
                columns: new[] { "MinecraftOpen", "minecraft_IP", "minecraft_Key", "minecraft_port" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, (ushort)0 });

            migrationBuilder.CreateIndex(
                name: "IX_User_MinecraftAccount_UserId",
                table: "User_MinecraftAccount",
                column: "UserId",
                unique: true);
        }
    }
}
