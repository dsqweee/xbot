namespace XBOT.DataBase.Models
{
    public class GiveAways
    {
        public ulong Id { get; set; }
        public ulong TextChannelId { get; set; }
        public TextChannel TextChannel { get; set; }
        public DateTime TimesEnd { get; set; }
        public string Surpice { get; set; }
        public uint WinnerCount { get; set; }
    }
}