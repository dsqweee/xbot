namespace XBOT.DataBase.Models.Roles_data
{
    public class Roles_Gived
    {
        public ulong Id { get; set; }
        public ulong RoleId { get; set; }
        public Roles Role { get; set; }
        public ulong MessageId { get; set; }
    }
}
