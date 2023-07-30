using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XBOT.Migrations
{
    /// <inheritdoc />
    public partial class Test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscordInvite_ReferralRole",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    RolesId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    Level = table.Column<byte>(type: "INTEGER", nullable: false),
                    UserJoinedValue = table.Column<uint>(type: "INTEGER", nullable: false),
                    UserWriteInWeekValue = table.Column<uint>(type: "INTEGER", nullable: false),
                    UserUp5LevelValue = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordInvite_ReferralRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordInvite_ReferralRole_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Roles_Buy",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Price = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles_Buy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Buy_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles_Gived",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    MessageId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles_Gived", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Gived_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles_Level",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Level = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles_Level", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Level_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles_Reputation",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Reputation = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles_Reputation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Reputation_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConnectionAudits",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    InviteId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ConnectionTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionAudits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscordInvite",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InviteKey = table.Column<string>(type: "TEXT", nullable: true),
                    AuthorId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DiscordUsesCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordInvite", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Guild_Logs",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TextChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Type = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guild_Logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReferralLinks",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    InviteId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferralLinks_DiscordInvite_InviteId",
                        column: x => x.InviteId,
                        principalTable: "DiscordInvite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RefferalInviteId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    RefferalInviteId1 = table.Column<ulong>(type: "INTEGER", nullable: true),
                    ReferalActivate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    XP = table.Column<ulong>(type: "INTEGER", nullable: false),
                    money = table.Column<ulong>(type: "INTEGER", nullable: false),
                    reputation = table.Column<ulong>(type: "INTEGER", nullable: false),
                    lastReputationUserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    reputation_Time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    streak = table.Column<ushort>(type: "INTEGER", nullable: false),
                    daily_Time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    messageCounterForDaily = table.Column<uint>(type: "INTEGER", nullable: false),
                    VoiceActive = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    MarriageId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    MarriageTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CountSex = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_ReferralLinks_RefferalInviteId1",
                        column: x => x.RefferalInviteId1,
                        principalTable: "ReferralLinks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_User_MarriageId",
                        column: x => x.MarriageId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Roles_User",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_User_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Roles_User_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionUsers_Logs",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Amount = table.Column<ulong>(type: "INTEGER", nullable: false),
                    TimeTransaction = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SenderId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    RecipientId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Type = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionUsers_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionUsers_Logs_User_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionUsers_Logs_User_SenderId",
                        column: x => x.SenderId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Prefix = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    AdminRoleId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    ModeratorRoleId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    IventerRoleId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    PrivateVoiceChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    PrivateTextChannelId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    PrivateMessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    LeaveMessage = table.Column<string>(type: "TEXT", nullable: true),
                    LeaveTextChannelId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    WelcomeMessage = table.Column<string>(type: "TEXT", nullable: true),
                    WelcomeDMmessage = table.Column<string>(type: "TEXT", nullable: true),
                    WelcomeDMuser = table.Column<bool>(type: "INTEGER", nullable: false),
                    WelcomeTextChannelId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    WelcomeRoleId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Settings_Roles_AdminRoleId",
                        column: x => x.AdminRoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Settings_Roles_IventerRoleId",
                        column: x => x.IventerRoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Settings_Roles_ModeratorRoleId",
                        column: x => x.ModeratorRoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Settings_Roles_WelcomeRoleId",
                        column: x => x.WelcomeRoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TextChannel",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SettingsId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextChannel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextChannel_Settings_SettingsId",
                        column: x => x.SettingsId,
                        principalTable: "Settings",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "AdminRoleId", "IventerRoleId", "LeaveMessage", "LeaveTextChannelId", "ModeratorRoleId", "Prefix", "PrivateMessageId", "PrivateTextChannelId", "PrivateVoiceChannelId", "Status", "WelcomeDMmessage", "WelcomeDMuser", "WelcomeMessage", "WelcomeRoleId", "WelcomeTextChannelId" },
                values: new object[] { 1ul, null, null, null, null, null, "v.", 0ul, null, 0ul, "Prefix: `v.`", null, false, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionAudits_InviteId",
                table: "ConnectionAudits",
                column: "InviteId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionAudits_UserId",
                table: "ConnectionAudits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordInvite_AuthorId",
                table: "DiscordInvite",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordInvite_ReferralRole_RolesId",
                table: "DiscordInvite_ReferralRole",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_Guild_Logs_TextChannelId",
                table: "Guild_Logs",
                column: "TextChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralLinks_InviteId",
                table: "ReferralLinks",
                column: "InviteId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralLinks_UserId",
                table: "ReferralLinks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Buy_RoleId",
                table: "Roles_Buy",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Gived_RoleId",
                table: "Roles_Gived",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Level_RoleId",
                table: "Roles_Level",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Reputation_RoleId",
                table: "Roles_Reputation",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_User_RoleId",
                table: "Roles_User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_User_UserId",
                table: "Roles_User",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_AdminRoleId",
                table: "Settings",
                column: "AdminRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_IventerRoleId",
                table: "Settings",
                column: "IventerRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_LeaveTextChannelId",
                table: "Settings",
                column: "LeaveTextChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_ModeratorRoleId",
                table: "Settings",
                column: "ModeratorRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_PrivateTextChannelId",
                table: "Settings",
                column: "PrivateTextChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_WelcomeRoleId",
                table: "Settings",
                column: "WelcomeRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_WelcomeTextChannelId",
                table: "Settings",
                column: "WelcomeTextChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_TextChannel_SettingsId",
                table: "TextChannel",
                column: "SettingsId");

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

            migrationBuilder.CreateIndex(
                name: "IX_User_MarriageId",
                table: "User",
                column: "MarriageId");

            migrationBuilder.CreateIndex(
                name: "IX_User_RefferalInviteId1",
                table: "User",
                column: "RefferalInviteId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ConnectionAudits_DiscordInvite_InviteId",
                table: "ConnectionAudits",
                column: "InviteId",
                principalTable: "DiscordInvite",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConnectionAudits_User_UserId",
                table: "ConnectionAudits",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordInvite_User_AuthorId",
                table: "DiscordInvite",
                column: "AuthorId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Guild_Logs_TextChannel_TextChannelId",
                table: "Guild_Logs",
                column: "TextChannelId",
                principalTable: "TextChannel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReferralLinks_User_UserId",
                table: "ReferralLinks",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Settings_TextChannel_LeaveTextChannelId",
                table: "Settings",
                column: "LeaveTextChannelId",
                principalTable: "TextChannel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Settings_TextChannel_PrivateTextChannelId",
                table: "Settings",
                column: "PrivateTextChannelId",
                principalTable: "TextChannel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Settings_TextChannel_WelcomeTextChannelId",
                table: "Settings",
                column: "WelcomeTextChannelId",
                principalTable: "TextChannel",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReferralLinks_DiscordInvite_InviteId",
                table: "ReferralLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_ReferralLinks_User_UserId",
                table: "ReferralLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_Settings_Roles_AdminRoleId",
                table: "Settings");

            migrationBuilder.DropForeignKey(
                name: "FK_Settings_Roles_IventerRoleId",
                table: "Settings");

            migrationBuilder.DropForeignKey(
                name: "FK_Settings_Roles_ModeratorRoleId",
                table: "Settings");

            migrationBuilder.DropForeignKey(
                name: "FK_Settings_Roles_WelcomeRoleId",
                table: "Settings");

            migrationBuilder.DropForeignKey(
                name: "FK_Settings_TextChannel_LeaveTextChannelId",
                table: "Settings");

            migrationBuilder.DropForeignKey(
                name: "FK_Settings_TextChannel_PrivateTextChannelId",
                table: "Settings");

            migrationBuilder.DropForeignKey(
                name: "FK_Settings_TextChannel_WelcomeTextChannelId",
                table: "Settings");

            migrationBuilder.DropTable(
                name: "ConnectionAudits");

            migrationBuilder.DropTable(
                name: "DiscordInvite_ReferralRole");

            migrationBuilder.DropTable(
                name: "Guild_Logs");

            migrationBuilder.DropTable(
                name: "Roles_Buy");

            migrationBuilder.DropTable(
                name: "Roles_Gived");

            migrationBuilder.DropTable(
                name: "Roles_Level");

            migrationBuilder.DropTable(
                name: "Roles_Reputation");

            migrationBuilder.DropTable(
                name: "Roles_User");

            migrationBuilder.DropTable(
                name: "TransactionUsers_Logs");

            migrationBuilder.DropTable(
                name: "DiscordInvite");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "ReferralLinks");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "TextChannel");

            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
