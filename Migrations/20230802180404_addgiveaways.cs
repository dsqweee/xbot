using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XBOT.Migrations
{
    /// <inheritdoc />
    public partial class addgiveaways : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GiveAways",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TextChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    TimesEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Surpice = table.Column<string>(type: "TEXT", nullable: true),
                    WinnerCount = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiveAways", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiveAways_TextChannel_TextChannelId",
                        column: x => x.TextChannelId,
                        principalTable: "TextChannel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiveAways_TextChannelId",
                table: "GiveAways",
                column: "TextChannelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiveAways");
        }
    }
}
