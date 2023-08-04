namespace XBOT.DataBase
{
    public class User_Permission
    {
        public ulong Id { get; set; }
        public bool Unlimited { get; set; }
        public byte CountWarn { get; set; } // CountMembers * 0.045 = CountWarnMax
        public ulong User_Id { get; set; }
        public User User { get; set; }
        public ICollection<User_Warn> warnlist { get; set; }
        public ICollection<User_UnWarn> unwarnlist { get; set; }
        public bool Active { get; set; }
    }
}