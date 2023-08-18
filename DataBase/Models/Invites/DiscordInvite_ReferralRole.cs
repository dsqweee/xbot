using XBOT.DataBase.Models.Roles_data;

namespace XBOT.DataBase.Models.Invites
{
    public class DiscordInvite_ReferralRole
    {
        public ulong Id { get; set; }
        public ulong RoleId { get; set; }
        public Roles Role { get; set; } // Исправление 15.08 delete 's'
        public uint UserJoinedValue { get; set; }
        public uint UserWriteInWeekValue { get; set; }
        public uint UserUp5LevelValue { get;set; }
    }
}
