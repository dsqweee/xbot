namespace XBOT.DataBase.Models.Roles_data
{
    public class Roles_Level
    {
        public ulong Id { get; set; }
        public ulong RoleId { get; set; }
        public Roles Role { get; set; }
        public uint Level { get; set; }
    }
}
