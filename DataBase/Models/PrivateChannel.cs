namespace XBOT.DataBase.Models
{
    public class PrivateChannel
    {
        public ulong Id { get; set; }
        public ulong UserId { get; set; }
        public User User { get; set; }
    }
}
