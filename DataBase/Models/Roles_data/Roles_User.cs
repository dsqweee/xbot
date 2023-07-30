namespace XBOT.DataBase.Models.Roles_data
{
    public class Roles_User
    {
        public ulong Id { get; set; }
        public ulong RoleId { get; set; }
        public Roles Role { get; set; }
        public ulong UserId { get; set; }
        public User User { get; set; }
    }
}
