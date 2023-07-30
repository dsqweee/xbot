namespace XBOT.DataBase
{
    public class Guild_Warn
    {
        public ulong Id { get; set; }
        public byte CountWarn { get; set; }
        public TimeSpan Time { get; set; }
        public ReportTypeEnum ReportTypes { get; set; }
        public virtual ICollection<User_Warn> User_Warns { get; set; }

        public enum ReportTypeEnum : byte
        {
            TimeBan, // ToTimeWarn in User_Warn
            Mute,
            TimeOut, 
            Kick,
            Ban
        }
    }
}
