using System.ComponentModel.DataAnnotations;

namespace XBOT.DataBase.Models.Invites
{
    public class DiscordInvite
    {
        [Key]
        public ulong Id { get; set; }
        public string InviteKey { get; set; }
        public ulong AuthorId { get; set; }
        public User Author { get; set; }
        public int DiscordUsesCount { get; set; }
        public List<DiscordInvite_ConnectionAudit> ConnectionAudits { get; set; }
        public List<DiscordInvite_ReferralLink> ReferralLinks { get; set; } // Список реферальных ссылок, связанных с этим приглашением
        //public List<User> UsersInThisInvite { get; set; } // Использовалось когда была связь с User, но теперь там ссылка на DiscordInvite_RefferalLink а не discordInvite
    }
}