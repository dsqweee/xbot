using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XBOT.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConnectEf2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReferralLinks_User_UserId1",
                table: "ReferralLinks");

            migrationBuilder.DropIndex(
                name: "IX_ReferralLinks_UserId",
                table: "ReferralLinks");

            migrationBuilder.DropIndex(
                name: "IX_ReferralLinks_UserId1",
                table: "ReferralLinks");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "ReferralLinks");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralLinks_UserId",
                table: "ReferralLinks",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReferralLinks_UserId",
                table: "ReferralLinks");

            migrationBuilder.AddColumn<ulong>(
                name: "UserId1",
                table: "ReferralLinks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferralLinks_UserId",
                table: "ReferralLinks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralLinks_UserId1",
                table: "ReferralLinks",
                column: "UserId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReferralLinks_User_UserId1",
                table: "ReferralLinks",
                column: "UserId1",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
