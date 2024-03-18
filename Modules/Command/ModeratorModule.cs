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
            //using var _db = new Db();
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

            var Permission = _db.User_Permission.FirstOrDefault(x=>x.User_Id == Context.User.Id);
            var ThisWarn = new User_Warn { AdminId = Permission.Id, UserId = User.Id, Guild_WarnsId = warn.Id, Reason = Reason, TimeSetWarn = DateTime.Now };
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

        [Aliases, Commands, Usage, Descriptions]
        public async Task mute(SocketGuildUser user, string time)
        {
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"Mute {user}");

            bool Success = TimeSpan.TryParse(time, out TimeSpan result);
            if (!Success)
                emb.WithDescription("Время введено неверно, возможно вы ввели слишком больше число?\nФормат: 01:00:00 [ч:м:с]\nФормат 2: 07:00:00:00 [д:ч:с:м]");
            else if (result.TotalSeconds < 30)
                emb.WithDescription("Время мута не может быть меньше 30 секунд!");
            else if (result.TotalHours > 12)
                emb.WithDescription("Больше чем на 12 часов замутить пользователя нельзя!");
            else
            {
                if (result.TotalDays > 28)
                    result = new TimeSpan(28, 0, 0, 0);

                var text = $"Вы успешно выдали нарушение на ";
                if (result.TotalSeconds > 86400)
                    text += $"{result.Days} дней и {result.Hours} часов";
                else if (result.TotalSeconds > 3600)
                    text += $"{result.Hours} часов и {result.Minutes} минут";
                if (result.TotalSeconds > 60)
                    text += $"{result.Minutes} минут и {result.Seconds} секунд";
                else
                    text += $"{result.Seconds} секунд";

                emb.WithDescription(text);
                await user.SetTimeOutAsync(result);
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task unmute(SocketGuildUser user)
        {
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"unMute {user}");

            if (user.TimedOutUntil == null)
                emb.WithDescription("У пользователя нету мута!");
            else
            {
                emb.WithDescription("Вы успешно сняли мут с пользователя.");
                await user.RemoveTimeOutAsync();
            }

            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task clear(uint CountMessage)
        {
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor).
                WithAuthor("Чистка сообщений");
            if (CountMessage > 100)
                emb.WithFooter("Удалить больше 100 сообщений нельзя!");

            var messages = await Context.Message.Channel.GetMessagesAsync((int)CountMessage + 1).FlattenAsync();
            await ((SocketTextChannel)Context.Channel).DeleteMessagesAsync(messages);
            emb.WithDescription($"Удалено {messages.Count()} сообщений");
            var x = await Context.Channel.SendMessageAsync("", false, emb.Build());
            await Task.Delay(5000);
            await x.DeleteAsync();
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task userclear(SocketGuildUser User, uint CountMessage)
        {
            var messages = await Context.Message.Channel.GetMessagesAsync((int)CountMessage).FlattenAsync();
            var result = messages.Where(x => x.Author.Id == User.Id);

            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"Чиста сообщений {User}")
                .WithDescription($"Удалено {result.Count()} сообщений от {User.Mention}");
            if (CountMessage > 100)
                emb.WithFooter("Удалить больше 100 сообщений нельзя!");
            else
                emb.WithFooter("Сообщения которым больше 14 дней не удаляются!");

            if (User == Context.User)
                await Context.Message.DeleteAsync();

            await ((SocketTextChannel)Context.Message.Channel).DeleteMessagesAsync(result);
            var x = await Context.Channel.SendMessageAsync("", false, emb.Build());
            await Task.Delay(5000);
            await x.DeleteAsync();
        }
    }
}
