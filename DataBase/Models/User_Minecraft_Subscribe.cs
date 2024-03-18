using System.ComponentModel.DataAnnotations.Schema;

namespace XBOT.DataBase.Models
{
    public class User_Minecraft_Subscribe
    {
        public ulong Id { get; set; }
        public ulong User_MinecraftId { get; set; }
        public User_Minecraft User_Minecraft { get; set; }
        public DateTime SubscribeAt { get; set; }
        public DateTime SubscribeTo { get; set; }

        public ulong? GiveByAdminId { get; set; } // Админ выдал подписку
        public virtual User_Minecraft GiveByAdmin { get; set; }

        [NotMapped]
        public bool ActiveSubscribe
        {
            get
            {
                var timeNow = DateTime.Now;
                return SubscribeAt < timeNow && SubscribeTo > timeNow;
            }
        }

        [NotMapped]
        public bool SubscribeFuture
        {
            get => SubscribeAt >= DateTime.Now;
        }
    }
}
