namespace XBOT.DataBase.Models.Roles_data
{
    public class Roles_Reputation
    {
        public ulong Id { get; set; }
        public ulong RoleId { get; set; }
        public Roles Role { get; set; }
        public uint Reputation { get; set; }
    }
}
