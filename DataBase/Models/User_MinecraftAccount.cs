namespace XBOT.DataBase.Models
{
    public class User_MinecraftAccount
    {
        public ulong Id { get; set; }
        public string MinecraftName { get; set; }
        public DateTime LicenceTo { get; set; }
        public ulong UserId { get; set; }
        public User User { get; set; }
        public bool whitelistAdded { get; set; }
    }
}
