using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XBOT.Migrations
{
    /// <inheritdoc />
    public partial class _0808up : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "whitelistAdded",
                table: "User_MinecraftAccount",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "whitelistAdded",
                table: "User_MinecraftAccount");
        }
    }
}
