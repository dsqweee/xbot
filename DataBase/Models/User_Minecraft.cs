using System.ComponentModel.DataAnnotations.Schema;

namespace XBOT.DataBase.Models
{
    public class User_Minecraft
    {
        public ulong Id { get; set; }
        public string UserName { get; set; }
        public bool UserNameConfirmed { get; set; }
        public ulong UserId { get; set; }
        public User User { get; set; }
        public List<User_Minecraft_Subscribe> User_Minecraft_Subscribe { get; set; } = new ();

        [NotMapped]
        public int CountSubs
        {
            get => User_Minecraft_Subscribe.Count;
        }

        public bool RulesAccept { get; set; }
    }
}
