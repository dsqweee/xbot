using XBOT.Services;
using XBOT.Services.Attribute;
using XBOT.Services.Configuration;
using static XBOT.DataBase.Guild_Warn;

namespace XBOT.Modules.Command
{
    [Name("Moderator"), Summary("Модерирование\nсервером")]
    [UserPermission(UserPermission.RolePermission.Moder)]
    public class ModeratorModule : ModuleBase<SocketCommandContext>
    {
        private readonly TaskTimer _timer;
        private readonly Db _db;

        public ModeratorModule(TaskTimer timer, Db db)
        {
            _timer = timer;
            _db = db;
        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task warn(SocketGuildUser User, string Reason)
        {

            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor).WithAuthor("Warn");
            var GuildWarn = _db.Guild_Warn.ToList();
            if (!GuildWarn.Any())
            {
                var Prefix = _db.Settings.FirstOrDefault().Prefix;
                emb.WithDescription("У вас еще нет нарушений для варнов!")
                   .WithFooter($"Подробнее `{Prefix}i addwarn`");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
                return;
            }

            var UserDataBase = await _db.GetUser(User.Id);

            var warn = GuildWarn.FirstOrDefault(x => x.CountWarn == (1 + UserDataBase.CountWarns));
            if (warn == null)
            {
                emb.WithDescription("У пользователя максимальное кол-во нарушений.");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
                return;
            }

            var ActiveWarn = _db.User_Warn.Any(x => x.UserId == User.Id && x.ToTimeWarn > DateTime.Now);
            if (ActiveWarn)
            {
                emb.WithDescription("У пользователя уже есть активное нарушение!");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
                return;
            }

            emb.WithDescription($"Пользователь {User.Mention} получил {1 + UserDataBase.CountWarns} нарушение");
            var DateTimes = DateTime.Now + warn.Time;

            var TimeWarns = UserDataBase.User_Warn.Where(x => x.ToTimeWarn > DateTime.Now).ToList();
            if (warn.ReportTypes == ReportTypeEnum.Mute || warn.ReportTypes == ReportTypeEnum.TimeOut)
                DeniedTimeWarn(ReportTypeEnum.TimeOut);

            else if (warn.ReportTypes == ReportTypeEnum.Ban || warn.ReportTypes == ReportTypeEnum.TimeBan)
                DeniedTimeWarn(ReportTypeEnum.TimeBan);

            void DeniedTimeWarn(ReportTypeEnum Type)
            {
                var TimeMuteWarns = TimeWarns.Where(x => x.Guild_Warns.ReportTypes == Type).ToList();
                foreach (var Warn in TimeMuteWarns)
                    Warn.WarnSkippedBecauseNewTimeWarn = true;
            }


            var ThisWarn = new User_Warn { AdminId = Context.User.Id, UserId = User.Id, Guild_WarnsId = warn.Id, Reason = Reason, TimeSetWarn = DateTime.Now };
            switch (warn.ReportTypes)
            {
                case ReportTypeEnum.Ban:
                case ReportTypeEnum.TimeBan:
                    if (warn.ReportTypes == ReportTypeEnum.TimeBan)
                    {
                        ThisWarn.ToTimeWarn = DateTimes;
                        await _timer.StartWarnTimer(ThisWarn);
                    }
                    await User.BanAsync();
                    break;
                case ReportTypeEnum.Mute:
                    var TimeSpan = new TimeSpan(28, 0, 0, 0);
                    ThisWarn.ToTimeWarn = DateTime.Now.Add(TimeSpan);
                    await User.SetTimeOutAsync(TimeSpan);
                    await _timer.StartWarnTimer(ThisWarn);
                    break;
                case ReportTypeEnum.Kick:
                    await User.KickAsync();
                    break;
                case ReportTypeEnum.TimeOut:
                    ThisWarn.ToTimeWarn = DateTimes;
                    await User.SetTimeOutAsync(DateTimes - DateTime.Now);
                    break;
            }
            _db.User_Warn.Add(ThisWarn);
            await _db.SaveChangesAsync();

            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }
    }
}
