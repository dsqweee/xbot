namespace XBOT.DataBase
{
    public class User_Warn
    {
        public ulong Id { get; set; }
        public ulong Guild_WarnsId { get; set; } // delete _ 15.08
        public virtual Guild_Warn Guild_Warns { get; set; }

        public ulong? UnWarnId { get; set; }// delete _ 15.08
        public virtual User_UnWarn UnWarn { get; set; }

        public ulong UserId { get; set; }
        public virtual User User { get; set; }

        public ulong AdminId { get; set; }// delete _ 15.08
        public virtual User_Permission Admin { get; set; }
        public string Reason { get; set; }

        public DateTime TimeSetWarn { get; set; } // Время выдачи варна
        public DateTime ToTimeWarn { get; set; } // Время когда истекает варн если Guild_Warns.ReportTypes == TimeBan или Mute или TimeMute
        public bool WarnSkippedAfterUnban { get; set; } // Варн был скипнуть если пользователь достиг максимального кол-ва нарушений и его разбанили.
        public bool WarnSkippedBecauseNewTimeWarn { get; set; } // Варн был скипнут если пользователь получил новый варн. Если у пользователя был Time варн и он получил новый TimeWarn или получил Mute или Ban
    }
}
