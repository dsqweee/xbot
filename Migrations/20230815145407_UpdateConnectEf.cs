using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XBOT.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConnectEf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReferralRole_Roles_RolesId",
                table: "ReferralRole");

            migrationBuilder.DropForeignKey(
                name: "FK_User_ReferralLinks_RefferalInviteId1",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Warn_Guild_Warn_Guild_WarnsId",
                table: "User_Warn");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Warn_User_Permission_AdminId",
                table: "User_Warn");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Warn_User_UnWarn_UnWarn_Id",
                table: "User_Warn");

            migrationBuilder.DropIndex(
                name: "IX_User_RefferalInviteId1",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_ReferralRole_RolesId",
                table: "ReferralRole");

            migrationBuilder.DropColumn(
                name: "Admin_Id",
                table: "User_Warn");

            migrationBuilder.DropColumn(
                name: "Guild_Warns_Id",
                table: "User_Warn");

            migrationBuilder.DropColumn(
                name: "Admin_Id",
                table: "User_UnWarn");

            migrationBuilder.DropColumn(
                name: "RefferalInviteId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "ReferralRole");

            migrationBuilder.DropColumn(
                name: "RolesId",
                table: "ReferralRole");

            migrationBuilder.RenameColumn(
                name: "UnWarn_Id",
                table: "User_Warn",
                newName: "UnWarnId");

            migrationBuilder.RenameIndex(
                name: "IX_User_Warn_UnWarn_Id",
                table: "User_Warn",
                newName: "IX_User_Warn_UnWarnId");

            migrationBuilder.RenameColumn(
                name: "RefferalInviteId1",
                table: "User",
                newName: "RefferalInvite_Id");

            migrationBuilder.AlterColumn<ulong>(
                name: "Guild_WarnsId",
                table: "User_Warn",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "AdminId",
                table: "User_Warn",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "UserId1",
                table: "ReferralLinks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRole_RoleId",
                table: "ReferralRole",
                column: "RoleId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ReferralRole_Roles_RoleId",
                table: "ReferralRole",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Warn_Guild_Warn_Guild_WarnsId",
                table: "User_Warn",
                column: "Guild_WarnsId",
                principalTable: "Guild_Warn",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Warn_User_Permission_AdminId",
                table: "User_Warn",
                column: "AdminId",
                principalTable: "User_Permission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Warn_User_UnWarn_UnWarnId",
                table: "User_Warn",
                column: "UnWarnId",
                principalTable: "User_UnWarn",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReferralLinks_User_UserId1",
                table: "ReferralLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_ReferralRole_Roles_RoleId",
                table: "ReferralRole");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Warn_Guild_Warn_Guild_WarnsId",
                table: "User_Warn");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Warn_User_Permission_AdminId",
                table: "User_Warn");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Warn_User_UnWarn_UnWarnId",
                table: "User_Warn");

            migrationBuilder.DropIndex(
                name: "IX_ReferralRole_RoleId",
                table: "ReferralRole");

            migrationBuilder.DropIndex(
                name: "IX_ReferralLinks_UserId1",
                table: "ReferralLinks");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "ReferralLinks");

            migrationBuilder.RenameColumn(
                name: "UnWarnId",
                table: "User_Warn",
                newName: "UnWarn_Id");

            migrationBuilder.RenameIndex(
                name: "IX_User_Warn_UnWarnId",
                table: "User_Warn",
                newName: "IX_User_Warn_UnWarn_Id");

            migrationBuilder.RenameColumn(
                name: "RefferalInvite_Id",
                table: "User",
                newName: "RefferalInviteId1");

            migrationBuilder.AlterColumn<ulong>(
                name: "Guild_WarnsId",
                table: "User_Warn",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<ulong>(
                name: "AdminId",
                table: "User_Warn",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<ulong>(
                name: "Admin_Id",
                table: "User_Warn",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "Guild_Warns_Id",
                table: "User_Warn",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "Admin_Id",
                table: "User_UnWarn",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "RefferalInviteId",
                table: "User",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Level",
                table: "ReferralRole",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<ulong>(
                name: "RolesId",
                table: "ReferralRole",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_RefferalInviteId1",
                table: "User",
                column: "RefferalInviteId1");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRole_RolesId",
                table: "ReferralRole",
                column: "RolesId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReferralRole_Roles_RolesId",
                table: "ReferralRole",
                column: "RolesId",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_ReferralLinks_RefferalInviteId1",
                table: "User",
                column: "RefferalInviteId1",
                principalTable: "ReferralLinks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Warn_Guild_Warn_Guild_WarnsId",
                table: "User_Warn",
                column: "Guild_WarnsId",
                principalTable: "Guild_Warn",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Warn_User_Permission_AdminId",
                table: "User_Warn",
                column: "AdminId",
                principalTable: "User_Permission",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Warn_User_UnWarn_UnWarn_Id",
                table: "User_Warn",
                column: "UnWarn_Id",
                principalTable: "User_UnWarn",
                principalColumn: "Id");
        }
    }
}
