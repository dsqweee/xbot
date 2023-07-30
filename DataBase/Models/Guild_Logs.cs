namespace XBOT.DataBase
{
    public class Guild_Logs
    {
        public ulong Id { get; set; }
        public ulong TextChannelId { get; set; }
        public TextChannel TextChannel { get; set; }
        public ChannelsTypeEnum Type { get; set; }
        public enum ChannelsTypeEnum : byte
        {
            Ban,
            UnBan,
            Kick,
            Left,
            Join,
            MessageEdit,
            MessageDelete,
            VoiceAction,
            BirthDay
        }
    }
}
