using XBOT.DataBase.Models.Roles_data;

namespace XBOT.DataBase.Models
{
    public class Settings
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public string Status { get; set; }

        public ulong? AdminRoleId { get; set; }
        public Roles AdminRole { get; set; }
        public ulong? ModeratorRoleId { get; set; }
        public Roles ModeratorRole { get; set; }
        public ulong? IventerRoleId { get; set; }
        public Roles IventerRole { get; set; }

        //public ulong? MediaRoleId { get; set; }
        //public Roles MediaRole { get; set; }


        public ulong PrivateVoiceChannelId { get; set; }
        public ulong? PrivateTextChannelId { get; set; }
        public TextChannel PrivateTextChannel { get; set; }
        public ulong PrivateMessageId { get; set; }

        public string LeaveMessage { get; set; }
        public ulong? LeaveTextChannelId { get; set; }
        public TextChannel LeaveTextChannel { get; set; }

        public string WelcomeMessage { get; set; }
        public string WelcomeDMmessage { get; set; }
        public bool WelcomeDMuser { get; set; }
        public ulong? WelcomeTextChannelId { get; set; }
        public TextChannel WelcomeTextChannel { get; set; }
        public ulong? WelcomeRoleId { get; set; }
        public Roles WelcomeRole { get; set; }

        public ushort minecraft_port { get; set; }
        public string minecraft_IP { get; set; }
        public string minecraft_Key { get; set; }
    }
}
