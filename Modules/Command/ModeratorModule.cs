using XBOT.DataBase;
using XBOT.Services;
using XBOT.Services.Attribute;
using XBOT.Services.Configuration;
using static XBOT.DataBase.Guild_Warn;

namespace XBOT.Modules.Command
{
    [Summary("Модерирование\nсервером"), Name("Moderator")]
    [UserPermission(UserPermission.RolePermission.Moder)]
    public class ModeratorModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IServiceProvider _provider;

        public ModeratorModule(CommandService service, IServiceProvider provider)
        {
            _service = service;
            _provider = provider;

        }

        [Aliases, Commands, Usage, Descriptions]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        public async Task warn(SocketGuildUser User, string Reason)
        {
            using (db _db = new())
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


                var ThisWarn = new User_Warn { Admin_Id = Context.User.Id, UserId = User.Id, Guild_Warns_Id = warn.Id, Reason = Reason, TimeSetWarn = DateTime.Now };
                switch (warn.ReportTypes)
                {
                    case ReportTypeEnum.Ban:
                    case ReportTypeEnum.TimeBan:
                        if (warn.ReportTypes == ReportTypeEnum.TimeBan)
                        {
                            ThisWarn.ToTimeWarn = DateTimes;
                            await TaskTimer.StartWarnTimer(ThisWarn);
                        }
                        await User.BanAsync();
                        break;
                    case ReportTypeEnum.Mute:
                        var TimeSpan = new TimeSpan(28, 0, 0, 0);
                        ThisWarn.ToTimeWarn = DateTime.Now.Add(TimeSpan);
                        await User.SetTimeOutAsync(TimeSpan);
                        await TaskTimer.StartWarnTimer(ThisWarn);
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

        //[Aliases, Commands, Usage, Descriptions]
        //public async Task Kick(SocketGuildUser user)
        //{
        //    using (var _db = new db())
        //    {
        //        var User = _db.User.FirstOrDefault(x=>x.Id == Context.User.Id);
        //        var UserGetter = await _db.GetUser(user.Id);

        //        //var Married = new User_Married { User = User, Spouse = UserGetter};
        //        //_db.User_Married.Add(Married);
        //        await _db.SaveChangesAsync();
        //    }
        //}
    }
}
