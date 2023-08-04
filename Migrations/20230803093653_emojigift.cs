using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XBOT.Migrations
{
    /// <inheritdoc />
    public partial class emojigift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmojiGift_emojiadded",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Factor = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmojiGift_emojiadded", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmojiGift",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    EmojiId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    PriceTrade = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmojiGift", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmojiGift_EmojiGift_emojiadded_EmojiId",
                        column: x => x.EmojiId,
                        principalTable: "EmojiGift_emojiadded",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmojiGift_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmojiGift_EmojiId",
                table: "EmojiGift",
                column: "EmojiId");

            migrationBuilder.CreateIndex(
                name: "IX_EmojiGift_UserId",
                table: "EmojiGift",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmojiGift");

            migrationBuilder.DropTable(
                name: "EmojiGift_emojiadded");
        }
    }
}
