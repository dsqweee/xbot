using System.ComponentModel.DataAnnotations.Schema;
using XBOT.DataBase.Models.Invites;
using XBOT.DataBase.Models.Roles_data;

namespace XBOT.DataBase.Models
{
    public class User
    {
        public ulong Id { get; set; }

        public ulong? RefferalInvite_Id { get; set; } // add _ 15.08
        public DiscordInvite_ReferralLink RefferalInvite { get; set; }

        public ulong? User_Permission_Id { get; set; }
        public User_Permission User_Permission { get; set; }

        public DateTime ReferalActivate { get; set; }
        public DateTime LastMessageTime { get; set; }
        public List<DiscordInvite> MyInvites { get; set; }
        public List<DiscordInvite_ConnectionAudit> MyConnectionAudits { get; set; }

        public string BlockReason { get; set; }

        public int CountWarns
        {
            get
            {
                int count = 0;
                foreach (var warn in User_Warn)
                {
                    if (warn.UnWarnId == null || warn.WarnSkippedAfterUnban || (warn.UnWarnId != null && warn.UnWarn.Status == User_UnWarn.WarnStatus.Rejected))
                        count++;
                }
                return count;
            }
        }

        public virtual ICollection<User_Warn> User_Warn { get; set; } = new List<User_Warn>();

        public virtual ICollection<EmojiGift> EmojiGift { get; set; } = new List<EmojiGift>();

        public List<Roles_User> Roles_User { get; set; }
        public ulong XP { get; set; }
        public ushort Level => (ushort)Math.Sqrt(XP / PointFactor);
        [NotMapped]
        public static double PointFactor = 100.0;
        [NotMapped]
        public static uint ExpPlus = 10;
        [NotMapped]
        public static int PeriodHours = 12; // Поставив меньше, нужно работать с коэфом выдаче бабла, иначе пользователи за это время просто не получат ничего.
        [NotMapped]
        public static uint DefaultMoney = 300;
        public ulong money { get; set; }

            
        public ulong reputation { get; set; }
        public ulong lastReputationUserId { get; set; }
        public DateTime reputation_Time { get; set; }

        public ushort streak { get; set; }
        public DateTime daily_Time { get; set; }
        public uint messageCounterForDaily { get; set; } // Во время написания Daily обнуляется, и считает количество написанных сообщений, благодаря этому деньги начисляются за активность

        
        public TimeSpan voiceActive_private { get; set; }
        public TimeSpan voiceActive_public { get; set; }

        public ulong? MarriageId { get; set; }
        public virtual User Marriage { get; set; }
        public DateTime MarriageTime { get; set; }
        public ulong CountSex { get; set; }


        public DateOnly BirthDate { get; set; }
        public int BirthDateComplete { get; set; }

        //public enum Role
        //{
        //    User,
        //    Iventer,
        //    Moderator,
        //    Admin
        //}
    }
}
