using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XBOT.Migrations
{
    /// <inheritdoc />
    public partial class _3008up : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionUsers_Logs_User_RecipientId",
                table: "TransactionUsers_Logs");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionUsers_Logs_User_SenderId",
                table: "TransactionUsers_Logs");

            migrationBuilder.DropIndex(
                name: "IX_TransactionUsers_Logs_RecipientId",
                table: "TransactionUsers_Logs");

            migrationBuilder.DropIndex(
                name: "IX_TransactionUsers_Logs_SenderId",
                table: "TransactionUsers_Logs");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionUsers_Logs_RecipientId",
                table: "TransactionUsers_Logs",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionUsers_Logs_SenderId",
                table: "TransactionUsers_Logs",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionUsers_Logs_User_RecipientId",
                table: "TransactionUsers_Logs",
                column: "RecipientId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionUsers_Logs_User_SenderId",
                table: "TransactionUsers_Logs",
                column: "SenderId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionUsers_Logs_User_RecipientId",
                table: "TransactionUsers_Logs");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionUsers_Logs_User_SenderId",
                table: "TransactionUsers_Logs");

            migrationBuilder.DropIndex(
                name: "IX_TransactionUsers_Logs_RecipientId",
                table: "TransactionUsers_Logs");

            migrationBuilder.DropIndex(
                name: "IX_TransactionUsers_Logs_SenderId",
                table: "TransactionUsers_Logs");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionUsers_Logs_RecipientId",
                table: "TransactionUsers_Logs",
                column: "RecipientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionUsers_Logs_SenderId",
                table: "TransactionUsers_Logs",
                column: "SenderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionUsers_Logs_User_RecipientId",
                table: "TransactionUsers_Logs",
                column: "RecipientId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionUsers_Logs_User_SenderId",
                table: "TransactionUsers_Logs",
                column: "SenderId",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
