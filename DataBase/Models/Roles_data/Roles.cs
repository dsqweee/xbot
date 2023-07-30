using XBOT.DataBase.Models.Invites;

namespace XBOT.DataBase.Models.Roles_data
{
    public class Roles
    {
        public ulong Id { get; set; }
        public List<Roles_Level> Roles_Level { get; set; }
        public List<Roles_Reputation> Roles_Reputation { get; set; }
        public List<Roles_Buy> Roles_Buy { get; set; }
        public List<Roles_Gived> Roles_Gived { get; set; }
        public List<Roles_User> Roles_User { get; set; }

        public List<DiscordInvite_ReferralRole> DiscordInvite_ReferralRole { get; set; }
    }
}