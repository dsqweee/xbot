namespace XBOT.DataBase.Models.Invites
{
    public class DiscordInvite_ReferralLink
    {
        public ulong Id { get; set; }
        public ulong UserId { get; set; }
        public User User { get; set; }
        public ulong InviteId { get; set; } // Ссылка на DiscordInvite
        public DiscordInvite Invite { get; set; }
        public DateTime CreationTime { get; set; }
        //public List<User> ReferredUsers { get; set; }
    }
}
