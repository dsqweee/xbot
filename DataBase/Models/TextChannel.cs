namespace XBOT.DataBase
{
    public class TextChannel
    {
        public ulong Id { get; set; }
        public Settings Settings { get; set; }
        public List<Guild_Logs> Guild_Logs { get; set; }
        public List<GiveAways> GiveAways { get; set; }

        public bool useCommand { get; set; }
        public bool useAdminCommand { get; set; }
        public bool useRPcommand { get; set; }
        public bool giveXp { get; set; }
        public bool delUrl { get; set; }
        public bool delUrlImage { get; set; }
        public bool inviteLink { get; set; }
    }
}
