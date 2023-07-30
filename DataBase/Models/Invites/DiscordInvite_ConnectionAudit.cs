namespace XBOT.DataBase.Models.Invites
{
    public class DiscordInvite_ConnectionAudit
    {
        public ulong Id { get; set; }
        public ulong UserId { get; set; } // ID пользователя
        public User User { get; set; }
        public ulong InviteId { get; set; } // ID ссылки приглашения
        public DiscordInvite Invite { get; set; }
        public DateTime ConnectionTime { get; set; } // Время подключения
    }
}
