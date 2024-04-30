using XBOT.Services.Configuration;
using XBOT.Services;
using XBOT.Services.Attribute;
using XBOT.DataBase.Models.Roles_data;
using System.Text;
using Microsoft.EntityFrameworkCore;
using XBOT.DataBase.Models.Invites;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using System.Data;
using static XBOT.DataBase.Guild_Warn;
using Pcg;

namespace XBOT.Modules.Command
{
    [Summary("Пользовательские\nкоманды"), Name("User")]
    public class UserModule : ModuleBase<SocketCommandContext>
    {
        private readonly InteractiveService _interactive;
        private readonly Refferal_Service _refferal_Service;
        private readonly Db _db;
        public UserModule(InteractiveService interactive, Db db, Refferal_Service refferal_Service)
        {
            _interactive = interactive;
            _db = db;
            _refferal_Service = refferal_Service;
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task usertop()
        {
            //using var _db = new Db();
            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor)
                                            .WithAuthor($"Топ пользователей", Context.User.GetAvatarUrl());

            var users = _db.User.AsEnumerable();

            var userLevel = users.OrderByDescending(x => x.XP).Take(5);
            var userRep = users.OrderByDescending(x => x.reputation).Take(5);
            var userMoney = users.OrderByDescending(x => x.money).Take(5);


            string GetTopUsersText(IEnumerable<ulong> Ids, IEnumerable<ulong> Value, string ValueName)
            {
                if (!Ids.Any())
                    return "Недостаточно данных";

                var stringBuilder = new StringBuilder();
                for (int i = 0; i < Ids.Count(); i++)
                {
                    stringBuilder.AppendLine($"{i+1}.<@{Ids.ElementAt(i)}> - {Value.ElementAt(i)} {ValueName}");
                }

                return stringBuilder.ToString();
            }

            string Text = GetTopUsersText(userLevel.Select(x => x.Id), userLevel.Select(x => (ulong)x.Level), "уровень");
            emb.AddField("ТОП УРОВЕНЬ 📈", Text, true);

            Text = GetTopUsersText(userRep.Select(x => x.Id), userRep.Select(x => x.reputation), "репутации");
            emb.AddField("ТОП РЕПУТАЦИИ ⚜️", Text, true);

            Text = GetTopUsersText(userMoney.Select(x => x.Id), userMoney.Select(x => x.money), "coins");
            emb.AddField("ТОП COINS 🏧", Text, true);


            await ReplyAsync(embed: emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task myloves(SocketGuildUser user)
        {
            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor)
                                        .WithAuthor($"Совместимость пары", Context.User.GetAvatarUrl());

            if (user == Context.User)
                emb.WithDescription("ЗРЯ...");
            else
            {
                var percent = new Random().Next(0, 101);
                emb.WithDescription($"{user.Mention} и {Context.User.Mention} совместимы на {percent}%");
            }

            await ReplyAsync(embed: emb.Build());
        }

        //[Aliases, Commands, Usage, Descriptions]
        //public async Task test()
        //{
        //    var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor);
        //    await Context.Channel.SendMessageAsync("", false, emb.Build());
        //}


        [Aliases, Commands, Usage, Descriptions]
        public async Task sex(SocketGuildUser user)
        {
            //using var _db = new Db();
            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor)
                                        .WithAuthor($"Чпокан чпокан чпокан", Context.User.GetAvatarUrl());

            var thisuserDb = await _db.GetUser(Context.User.Id);

            var prefix = _db.Settings.FirstOrDefault().Prefix;

            if (thisuserDb.BirthDate.Year != 1 || DateTime.Now.Year - thisuserDb.BirthDate.Year <= 18)
            {
                emb.WithDescription($"Какой секс, в школу пора...\nУ вас не указана дата рождения: {prefix}bds [пример - 01.01.2001]").WithFooter("Паспорт покажи...");
                await ReplyAsync(embed: emb.Build());
                return;
            }
            var mentionuserDb = await _db.GetUser(user.Id);

            if (mentionuserDb.BirthDate.Year != 1 || DateTime.Now.Year - mentionuserDb.BirthDate.Year <= 18)
            {
                emb.WithDescription($"Ей нет 18... Fbi open up\nУ пользователя не указана дата рождения: {prefix}bds [пример - 01.01.2001]").WithFooter("Спроси паспорт...");
                await ReplyAsync(embed: emb.Build());
                return;
            }


            var TimeoutMessage = new TimeSpan(0, 1, 0);
            var options = new ButtonOption<string>[]
            {
                new("Принять", ButtonStyle.Success),
                new("Отклонить", ButtonStyle.Danger)
            };

            var pageBuilder = new PageBuilder()
                       .WithColor(emb.Color.Value)
                       .WithAuthor($"{Context.User} 💞 {user}")
                       .WithDescription($"{Context.User.Mention} предлагает хубабубу (жевачку), {user.Mention} хочешь?")
                       .WithFooter($"Заявка активна {TimeoutMessage.TotalMinutes} минуту.")
                       .WithThumbnailUrl(user.GetAvatarUrl());

            var buttonSelection = new ButtonSelectionBuilder<string>()
                .WithActionOnSuccess(ActionOnStop.DeleteInput)
                .WithActionOnTimeout(ActionOnStop.DeleteInput)
                .WithOptions(options)
                .WithStringConverter(x => x.Option)
                .WithSelectionPage(pageBuilder)
                .AddUser(user)
                .Build();

            var result = await _interactive.SendSelectionAsync(buttonSelection, Context.Channel, TimeoutMessage);


            if (result.IsTimeout)
                emb.WithDescription($"{user.Mention} не успел(а) принять хабубабубу!");
            else
            {
                if (result.Value.Option == "Принять")
                {
                    thisuserDb.CountSex += 1;
                    mentionuserDb.CountSex += 1;
                    _db.User.UpdateRange(new[] { thisuserDb, mentionuserDb });
                    await _db.SaveChangesAsync();
                    emb.WithDescription($"{user.Mention} 💕 {Context.User.Mention} захубабубились!");
                }
                else
                    emb.WithDescription($"{user.Mention} отказался(лась) от чпоканья!");
            }

            await result.Message.ModifyAsync(x => x.Embed = emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [BirthDatePermission]
        public async Task BirthDateSet(string date)
        {
            //using var _db = new Db();
            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor)
                                        .WithAuthor($"День рождения", Context.User.GetAvatarUrl());
            DateOnly dateConvert;

            if (!DateOnly.TryParseExact(date, "dd.MM.yyyy", out dateConvert))
            {
                emb.WithDescription($"Введит дату в формате: {DateTime.Now.ToString("dd.MM.yyyy")}");
                await ReplyAsync(embed: emb.Build());
                return;
            }


            var TimeoutMessage = new TimeSpan(0, 1, 0);
            var options = new ButtonOption<string>[]
            {
                    new("Принять", ButtonStyle.Success),
                    new("Отклонить", ButtonStyle.Danger)
            };

            var pageBuilder = new PageBuilder()
                       .WithAuthor(emb.Author)
                       .WithColor(emb.Color.Value)
                       .WithDescription($"Для выставления даты [{date}], вам необходимо подтвердить ее, так как в будущем сменить ее будет невозможно.")
                       .WithFooter($"Заявка активна {TimeoutMessage.TotalMinutes} минуту.")
                       .WithThumbnailUrl(Context.User.GetAvatarUrl());

            var buttonSelection = new ButtonSelectionBuilder<string>()
                .WithActionOnSuccess(ActionOnStop.DeleteInput)
                .WithActionOnTimeout(ActionOnStop.DeleteInput)
                .WithOptions(options)
                .WithStringConverter(x => x.Option)
                .WithSelectionPage(pageBuilder)
                .AddUser(Context.User)
                .Build();

            var result = await _interactive.SendSelectionAsync(buttonSelection, Context.Channel, TimeoutMessage);


            if (result.IsTimeout || result.Value.Option == "Отклонить")
            {
                await result.Message.DeleteAsync();
                return;
            }

            emb.WithDescription($"Вы успешно выставили дату рождения.\nДата: {date}");
            var user = await _db.GetUser(Context.User.Id);
            user.BirthDate = dateConvert;
            _db.User.Update(user);
            await _db.SaveChangesAsync();
            await result.Message.ModifyAsync(x => x.Embed = emb.Build());

        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task warns()
        {
            //using var _db = new Db();
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor("⚜️ Варны сервера")
                .WithDescription("На сервере еще нет варнов!");
            var Warns = _db.Guild_Warn.OrderBy(x => x.CountWarn);


            if (Warns.Any())
            {
                emb.WithDescription("");
                foreach (var warn in Warns)
                {
                    string text = string.Empty;
                    switch (warn.ReportTypes)
                    {
                        case ReportTypeEnum.TimeBan:
                            text = $"Временный бан на " + TimeToStringConverter(warn.Time);
                            break;
                        case ReportTypeEnum.Mute:
                            text = "Мут";
                            break;
                        case ReportTypeEnum.TimeOut:
                            text = $"Мут на " + TimeToStringConverter(warn.Time);
                            break;
                        case ReportTypeEnum.Kick:
                            text = "Кик";
                            break;
                        case ReportTypeEnum.Ban:
                            text = "Бан";
                            break;
                    }

                    emb.Description += $"{warn.CountWarn}.{text}\n";
                }
            }


            string TimeToStringConverter(TimeSpan time)
            {
                if (time.Days > 0)
                    return $"{time.Days} дней ";
                if (time.Hours > 0)
                    return $"{time.Hours} часов ";

                return $"{time.Minutes} минут ";
            }

            await ReplyAsync(embed: emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task userinfo(SocketGuildUser user = null)
        {
            //using var _db = new Db();
            if (user == null)
                user = Context.User as SocketGuildUser;


            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"{user}", user.GetAvatarUrl())
                .WithThumbnailUrl(user.GetAvatarUrl());

            var UserDataBase = await _db.GetUser(user.Id);

            string DailyCoin = null;
            TimeSpan TimeToDaily = UserDataBase.daily_Time - DateTime.Now;
            if (TimeToDaily.TotalSeconds < new TimeSpan(-User.PeriodHours, 0, 0).TotalSeconds)
                DailyCoin = "Получение - доступно сейчас!";
            else
            {
                if (TimeToDaily.TotalSeconds > 0)
                    DailyCoin = $"До получения - {TimeToDaily.Hours}:{TimeToDaily.Minutes}:{TimeToDaily.Seconds}";
                else
                {
                    DailyCoin = $"До сброса комбо получения - {TimeToDaily.Hours + 5}:{TimeToDaily.Minutes + 60}:{TimeToDaily.Seconds + 60}";
                }
            }

            TimeToDaily = UserDataBase.reputation_Time - DateTime.Now;

            string dailyRep;
            if (TimeToDaily.TotalSeconds < -1)
                dailyRep = "Репутация - доступна сейчас!";
            else
                dailyRep = $"До репутации - {TimeToDaily.Hours}:{TimeToDaily.Minutes}:{TimeToDaily.Seconds}";

            string Marryed = "Не состоит";
            var marryedId = Convert.ToUInt64(UserDataBase.MarriageId);
            if (marryedId != 0)
            {
                var Time = DateTime.Now - UserDataBase.MarriageTime;
                string timeMarryed;
                if (Time.TotalSeconds < 60)
                    timeMarryed = "Меньше минуты";
                else if (Time.TotalMinutes <= 60)
                    timeMarryed = $"{Math.Round(Time.TotalMinutes)} минут";
                else if (Time.TotalHours <= 24)
                    timeMarryed = $"{Math.Round(Time.TotalHours)} часов";
                else
                    timeMarryed = $"{Math.Round(Time.TotalDays)} дней";

                Marryed = $"Половинка: <@{UserDataBase.MarriageId}>\nВ браке: {timeMarryed}";
            }

            if (UserDataBase.BirthDate.Year != 1 && UserDataBase.BirthDate.Year >= 18)
                Marryed += $"\nКол-во половых актов: {UserDataBase.CountSex}";

            emb.AddField("Отношения", Marryed, true);

            emb.AddField("Репутация", $"Количество: {UserDataBase.reputation}\n{dailyRep}", true);

            emb.AddField("Coins", $"Количество: {UserDataBase.money}\nКомбо: {UserDataBase.streak}\n{DailyCoin}", true);

            var Settings = _db.Settings.FirstOrDefault();
            string birthday;
            if (UserDataBase.BirthDate.Year != 1)
                birthday = $"{UserDataBase.BirthDate.ToString("dd.MM.yyyy")}";
            else
                birthday = $"{Settings.Prefix}birthdateset 01.01.2005";

            var WarnsCount = _db.Guild_Warn.Count();

            emb.AddField("Другое", $"Реферальная система: {Settings.Prefix}refferal\nНарушений: {UserDataBase.CountWarns}/{WarnsCount}\nДень рождения: {birthday}", true);

            var TimePublic = ConvertTime(UserDataBase.voiceActive_public);
            var TimePrivate = ConvertTime(UserDataBase.voiceActive_private);

            string ConvertTime(TimeSpan Time)
                => $"{(int)Time.TotalHours:00}:{Time.Minutes:00}:{Time.Seconds:00}";


            uint count = Convert.ToUInt32(UserDataBase.Level * User.PointFactor * UserDataBase.Level);
            uint countNext = Convert.ToUInt32((UserDataBase.Level + 1) * User.PointFactor * (UserDataBase.Level + 1));
            emb.AddField("Опыт", $"Уровень: {UserDataBase.Level}\nОпыт: {UserDataBase.XP - count}/{countNext - count}\nАктивность в голосовых чатах: {TimePublic}\nАктивность в приватных чатах: {TimePrivate}", false);

            await ReplyAsync(embed: emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task refferal()
        {
            //using var _db = new Db();
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"Реферальная система");
            var userDs = Context.User as SocketGuildUser;
            var userDb = _db.User.Include(x => x.MyInvites).ThenInclude(x => x.ReferralLinks).ThenInclude(x => x.User).FirstOrDefault(x => x.Id == userDs.Id);
            var UserValue = await _refferal_Service.GetRefferalValue(userDb);

            var RefferalRoles = _db.ReferralRole.OrderBy(x => x.UserJoinedValue).ToList();
            if (RefferalRoles.Count == 0)
            {
                emb.WithDescription("Рефферальные роли отсутствуют.");
            }
            else
            {
                var ThisRole = RefferalRoles.LastOrDefault(x => UserValue.UserJoinedValue >= x.UserJoinedValue && UserValue.WriteInWeek >= x.UserWriteInWeekValue && UserValue.Level5up >= x.UserUp5LevelValue);
                
                var indexThisRole = RefferalRoles.IndexOf(ThisRole);
                DiscordInvite_ReferralRole NextRole = null;

                if (indexThisRole >= 0)
                    NextRole = RefferalRoles?.ElementAt(indexThisRole + 1);
                else if ((indexThisRole + 1) != RefferalRoles.Count)
                    NextRole = RefferalRoles.FirstOrDefault();

                string thisroletext = "";

                if (ThisRole is null)
                    thisroletext = "Отсутствует";
                else
                    thisroletext = $"<@&{ThisRole.RoleId}>";

                string nextroletext = "";
                if (NextRole is null)
                    nextroletext = "Максимум";
                else
                    nextroletext = $"<@&{NextRole?.RoleId}>";

                string prefix = _db.Settings.FirstOrDefault().Prefix;

                emb.WithDescription("Приглашая на сервер друга, вы получаете за него баллы, которые преобразуются в роли. Чем выше роль, тем больше ваш престиж.\n" +
                                    $"Список реферальных ролей: `{prefix}refferalrole`\n\n" +
                                    $"Ваша текущая роль: {thisroletext} -> {nextroletext}\n" +
                                    $"Приведенных участников: `{UserValue.UserJoinedValue}/{(NextRole is not null ? NextRole.UserJoinedValue : "max")}`\n" +
                                    $"Писали в течении недели: `{UserValue.WriteInWeek}/{(NextRole is not null ? NextRole.UserWriteInWeekValue : "max")}`\n" +
                                    $"Достигли 5 уровня: `{UserValue.Level5up}/{(NextRole is not null ? NextRole.UserUp5LevelValue : "max")}`")
                    .WithFooter("Роль может выдаваться в течении часа.");

            }
            await ReplyAsync(embed: emb.Build());
        }

        //[Aliases, Commands, Usage, Descriptions]
        //[ActivityPermission]
        //public async Task warnappilation(ulong warnid = 0)
        //{
        //    using (Db _db = new ())
        //    {
        //        var emb = new EmbedBuilder()
        //            .WithColor(BotSettings.DiscordColor)
        //            .WithAuthor($"Аппеляция на нарушение");


        //        if(warnid == 0)
        //        {
        //            emb.WithDescription("");
        //            var user = db.User.Include(x => x.User_Warn).ThenInclude(x => x.UnWarn).FirstOrDefault(x => x.Id == Context.User.Id);
        //            var OldDate = DateTime.Now.AddDays(-3);
        //            var ActiveWarns = user.User_Warn.Where(x => x.TimeSetWarn >= OldDate);
        //            if (ActiveWarns.Any())
        //            {
        //                foreach (var warn in ActiveWarns)
        //                {
        //                    emb.Description += $"{warn.Id}.<@{warn.Admin_Id}> [{warn.TimeSetWarn:dd.MM HH:mm}] Причина: {warn.Reason}\n";
        //                }
        //                var Settings = db.Settings.FirstOrDefault();
        //                emb.WithFooter($"Чтоб подать аппеляция - {Settings.Prefix}warnappilation [number]");
        //            }
        //            else
        //                emb.WithDescription("Нарушений за 3 дня не найдено!")
        //                   .WithColor(BotSettings.DiscordColorError);

        //        }
        //        else
        //        {
        //            var warninfo = db.User_Warn.Include(x => x.User).FirstOrDefault(x=>x.Id == warnid);
        //            if (warninfo is null)
        //                emb.WithDescription($"Нарушение под номером [{warnid}] не найдено.").WithColor(BotSettings.DiscordColorError);
        //            else
        //            {
        //                var Settings = db.Settings.Include(x=>x.AdminRole).FirstOrDefault();
        //                List<SocketGuildUser> users = new();
        //                foreach (var user_perm in db.User_Permission)
        //                {
        //                    var User = Context.Guild.GetUser(user_perm.User_Id);
        //                    if(User != null && User.Roles.Any(x=>x.Id == Settings.AdminRole.Id))
        //                    {
        //                        users.Add(User);
        //                    }
        //                }

        //                var PinAdmin = users.ElementAt(new Random().Next(0, users.Count));

        //                var Appilation = new User_UnWarn {Admin_Id = PinAdmin.Id, Warn_Id = warnid,ReviewAdd = DateTime.Now,Status = User_UnWarn.WarnStatus.review};
        //                db.User_UnWarn.Add(Appilation);
        //                await db.SaveChangesAsync();

        //                emb.WithDescription($"Пользователь: {Context.User.Mention}\n" +
        //                                    $"Id: {Context.User.Id}\n" +
        //                                    $"Модератор/администратор: <@{warninfo.Admin_Id}>\n" +
        //                                    $"Чтобы принять аппиляцию, перейдите в {Settings.Prefix}appilationlist");
        //                await PinAdmin.SendMessageAsync("",false,emb.Build());
        //                emb.WithDescription("Вы успешно подали аппиляцию, ожидайте ответа админа.");

        //            }
        //        }
        //        await Context.Channel.SendMessageAsync("",false, emb.Build());
        //    }
        //}


        [Aliases, Commands, Usage, Descriptions]
        [ActivityPermission]
        public async Task transfer(SocketGuildUser User, ushort coin)
        {
            //using var _db = new Db();
            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor).WithAuthor($"{Context.User} 💱 {User}");
            const ushort maxSum = 25000;

            if (User.Id == Context.User.Id)
            {
                SendMessage("Переводить деньги самому себе нельзя!");
                return;
            }

            if (coin > maxSum)
            {
                SendMessage($"Перевести больше {maxSum} StarCoin нельзя.");
                return;
            }

            var currentUser = await _db.GetUser(Context.User.Id);

            if (currentUser.money < coin)
            {
                SendMessage($"У вас недостаточно средств для перевода.\nВам нехватает {coin - currentUser.money} coins");
                return;
            }

            var transfUser = await _db.GetUser(User.Id);
            var TransferLog = new TransactionUsers_Logs { SenderId = currentUser.Id, RecipientId = transfUser.Id, Amount = coin, TimeTransaction = DateTime.Now, Type = TransactionUsers_Logs.TypeTransation.Transfer };
            _db.TransactionUsers_Logs.Add(TransferLog);

            if (transfUser.money + coin >= BotSettings.CoinsMaxUser)
            {
                var MinimalMoneyTransfer = (currentUser.money + coin) - BotSettings.CoinsMaxUser;
                currentUser.money -= MinimalMoneyTransfer;
                transfUser.money = BotSettings.CoinsMaxUser;
                SendMessage($"Перевод в размере {MinimalMoneyTransfer} StarCoin успешно прошел.");
            }
            else
            {
                currentUser.money -= coin;
                transfUser.money += coin;
                SendMessage($"Перевод в размере {coin} StarCoin успешно прошел.");
            }

            _db.User.UpdateRange(new User[] { currentUser, transfUser });
            await _db.SaveChangesAsync();


            async void SendMessage(string description)
            {
                emb.WithDescription(description);
                await ReplyAsync(embed: emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [ActivityPermission]
        public async Task reputation(SocketGuildUser RepUser)
        {
            //using var _db = new Db();
            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor)
                                        .WithAuthor($" - Репутация 🏧", Context.User.GetAvatarUrl());
            var userDB = await _db.GetUser(Context.User.Id);
            var DateNow = DateTime.Now;
            string message;

            DateTime Daily = userDB.reputation_Time;

            if (Daily.Year == 1)
                Daily = DateNow;

            if (DateNow >= Daily)
            {
                if (RepUser.Id == Context.User.Id)
                    message = "Повысить репутацию самому себе нельзя.";
                else
                {
                    if (userDB.lastReputationUserId == 0 || RepUser.Id != userDB.lastReputationUserId)
                    {
                        var UserDb = await _db.GetUser(RepUser.Id);
                        await RepRole(RepUser, UserDb.reputation);
                        UserDb.reputation += 1;
                        var TransferLog = new TransactionUsers_Logs { SenderId = userDB.Id, RecipientId = UserDb.Id, Amount = 1, TimeTransaction = DateTime.Now, Type = TransactionUsers_Logs.TypeTransation.Reputation };

                        userDB.lastReputationUserId = UserDb.Id;
                        userDB.reputation_Time = DateNow.AddHours(User.PeriodHours);

                        message = $"{Context.User.Mention} повысил репутацию {RepUser.Mention}\nРепутация: +{UserDb.reputation}\nСледующая репутация через {User.PeriodHours} часов"; // {Math.Round((UserThis.DailyRep - DateTime.Now).TotalHours)}
                        _db.TransactionUsers_Logs.Add(TransferLog);
                        _db.User.UpdateRange(new[] { UserDb, userDB });
                        await _db.SaveChangesAsync();
                    }
                    else
                        message = "Вы не можете выдать репутацию одному и тому же пользователю 2 раза подряд.";
                }
            }
            else
            {
                var TimeToDaily = Daily - DateNow;
                if (TimeToDaily.TotalSeconds >= 3600)
                    message = $"Дождитесь {TimeToDaily.Hours} часов и {TimeToDaily.Minutes} минут чтобы выдать репутацию!";
                else
                    message = $"Дождитесь {(TimeToDaily.TotalSeconds > 60 ? $"{TimeToDaily.Minutes} минут и " : "")} {TimeToDaily.Seconds} секунд чтобы выдать репутацию!";
            }

            emb.WithDescription(message);
            await ReplyAsync(embed: emb.Build());
        }

        public async Task RepRole(SocketGuildUser UserDiscord, ulong Reputation)
        {
            //using var _db = new Db();
            var Roles = _db.Roles_Reputation.AsEnumerable().OrderBy(x => x.Reputation);
            var ThisRole = Roles.LastOrDefault(x => x.Reputation <= Reputation);

            Reputation++;
            var NextRole = Roles.FirstOrDefault(x => x.Reputation == Reputation);
            if (NextRole != null)
            {
                if (ThisRole != null)
                    await UserDiscord.RemoveRoleAsync(ThisRole.RoleId);

                await UserDiscord.AddRoleAsync(NextRole.RoleId);
            }
            else if (ThisRole != null)
                await UserDiscord.AddRoleAsync(ThisRole.RoleId);

        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task daily()
        {
            //using var _db = new Db();
            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor)
                                        .WithAuthor($"Ежедневное пособие Coins 🏧", Context.User.GetAvatarUrl());

            var userDB = await _db.GetUser(Context.User.Id);
            var DateNow = DateTime.Now;

            DateTime Daily = userDB.daily_Time;

            if (Daily.Year == 1)
                Daily = DateNow;

            if (DateNow >= Daily)
            {
                if ((DateNow - Daily).TotalSeconds >= new TimeSpan(User.PeriodHours, 0, 0).TotalSeconds)
                    userDB.streak = 1;
                else
                    userDB.streak++;

                var result = MathDaily(User.DefaultMoney, userDB.Level, userDB.messageCounterForDaily, userDB.streak);


                userDB.messageCounterForDaily = 1;
                userDB.daily_Time = DateNow.AddHours(User.PeriodHours);

                if (userDB.money + result >= BotSettings.CoinsMaxUser)
                {
                    userDB.money = BotSettings.CoinsMaxUser;
                    emb.WithDescription($"Монет получено: Вы превысили лимит.");
                }
                else
                {
                    userDB.money += result;
                    var TransferLog = new TransactionUsers_Logs { RecipientId = Context.User.Id, Amount = result, TimeTransaction = DateTime.Now, Type = TransactionUsers_Logs.TypeTransation.daily };
                    _db.TransactionUsers_Logs.Add(TransferLog);
                    emb.WithDescription($"Монет получено: {result}");
                }
                emb.Description += $"\nКомбо: {userDB.streak}\nСледующее получение coins через {User.PeriodHours} часов";
                emb.WithFooter("Кол-во coins зависит от активности в чате <3");
                _db.User.Update(userDB);
                await _db.SaveChangesAsync(); // SQLite Error 19: 'UNIQUE constraint failed: TransactionUsers_Logs.RecipientId'
            }
            else
            {
                var TimeToDaily = Daily - DateNow;
                if (TimeToDaily.TotalSeconds >= 3600)
                    emb.WithDescription($"Дождитесь {TimeToDaily.Hours} часов и {TimeToDaily.Minutes} минут чтобы получить coins!");
                else
                    emb.WithDescription($"Дождитесь {(TimeToDaily.TotalSeconds > 60 ? $"{TimeToDaily.Minutes} минут и " : "")} {TimeToDaily.Seconds} секунд чтобы получить coins!");
            }

            await ReplyAsync(embed: emb.Build());

        }

        // (500 * 0.055 + Streak) * (5 + 1) * (0.355 / (0.10 + 0.03))

        // (500 * 0.055) = default sum * множитель
        // (5 + 1) = Level + next level
        // (0.10 + 0.03) = 0.05 + level + сотая сообщений (155 будет 1)
        // (0.355 / (0.10 + 0.03)) = кол-во смс / множитель
        private static ulong MathDaily(uint defaultmoney, ushort Level, ulong messageCounterForDaily, ushort streak)
        {
            double first = defaultmoney * (0.055 + streak / 1000.0);

            double second = Level + 1;

            double Pointfirst = messageCounterForDaily / 10000.0;

            double Pointsecond = Math.Truncate(Pointfirst * 100) / 100;

            double Pointthird = Level / 100.0;

            double third = 0.05 + Pointthird + Pointsecond;
            double fifth = messageCounterForDaily / 1000.0;

            return Convert.ToUInt64(first * second * (fifth / third));
        }

        [Aliases, Commands, Usage, Descriptions]
        [MarryPermission]
        public async Task marry(SocketGuildUser user)
        {
            //using var _db = new Db();
            var emb = new EmbedBuilder()
                        .WithColor(BotSettings.DiscordColor)
                        .WithAuthor($"💞 Женидьба - Ошибка");

            if (Context.User.Id == user.Id)
                emb.WithDescription("Я знаю что ты любишь себя, но к сожалению на себе жениться нельзя!");

            var marryuser = await _db.GetUser(user.Id);

            if (marryuser.MarriageId != null)
            {
                var Prefix = _db.Settings.FirstOrDefault();
                emb.WithDescription($"{user.Mention} женат(а), этому пользователю сначала нужно развестись!")
                   .WithFooter($"Развестить - {Prefix.Prefix}divorce");
            }

            if (emb.Description != null)
            {
                await ReplyAsync(embed: emb.Build());
                return;
            }

            var ContextUser = await _db.GetUser(Context.User.Id);

            var TimeoutMessage = new TimeSpan(0, 1, 0);
            var options = new ButtonOption<string>[]
            {
                    new("Принять", ButtonStyle.Success),
                    new("Отклонить", ButtonStyle.Danger)
            };

            var pageBuilder = new PageBuilder()
                       .WithAuthor($"{Context.User} 💞 {user}")
                       .WithDescription($"{Context.User.Mention} отправил заявку на помолвку {user.Mention}.\n\n{user.Username}, хочешь пожениться?")
                       .WithFooter("Заявка активна 60 секунд.")
                       .WithThumbnailUrl(user.GetAvatarUrl());

            var buttonSelection = new ButtonSelectionBuilder<string>()
                .WithActionOnSuccess(ActionOnStop.DeleteInput)
                .WithActionOnTimeout(ActionOnStop.DeleteInput)
                .WithOptions(options)
                .WithStringConverter(x => x.Option)
                .WithSelectionPage(pageBuilder)
                .AddUser(user)
                .Build();

            var result = await _interactive.SendSelectionAsync(buttonSelection, Context.Channel, TimeoutMessage);


            if (result.IsTimeout)
            {
                emb.WithDescription($"{user.Mention} не успел(а) принять заявку!");
            }
            else
            {
                if (result.Value.Option == "Принять")
                {
                    ContextUser.MarriageId = marryuser.Id;
                    marryuser.MarriageId = ContextUser.Id;
                    marryuser.MarriageTime = DateTime.Now;
                    ContextUser.MarriageTime = DateTime.Now;
                    _db.User.UpdateRange(new[] { ContextUser, marryuser });
                    await _db.SaveChangesAsync();
                    emb.WithDescription($"Я, данной мне властью, обьявляю парой {user.Mention} и {Context.User.Mention}!");
                }
                else
                {
                    emb.WithDescription($"{user.Mention} отказался(лась) от свадьбы!");
                }
            }

            await result.Message.ModifyAsync(x => x.Embed = emb.Build());





            //var AcceptButton = Guid.NewGuid().ToString();
            //var DeclineButton = Guid.NewGuid().ToString();
            //var builder = new ComponentBuilder()
            //                .WithButton("Принять", AcceptButton, ButtonStyle.Success)
            //                .WithButton("Отклонить", DeclineButton, ButtonStyle.Danger);
            //var mes = await Context.Channel.SendMessageAsync("", false, emb.Build(), components: builder.Build());

            //var userInteraction = new ComponentEvent<SocketMessageComponent>(new HashSet<string> { AcceptButton, DeclineButton });
            //_componentEventService.AddInteraction(new HashSet<string> { AcceptButton, DeclineButton }, userInteraction);

            //var selectedOption = await userInteraction.WaitForInteraction();
            //if (selectedOption == null)
            //{
            //    emb.WithDescription($"{user.Mention} не успел(а) принять заявку!");
            //}
            //else if (selectedOption.Data.CustomId == AcceptButton)
            //{
            //    ContextUser.MarriageId = marryuser.Id;
            //    marryuser.MarriageId = ContextUser.Id;
            //    marryuser.MarriageTime = DateTime.Now;
            //    ContextUser.MarriageTime = DateTime.Now;
            //    db.User.UpdateRange(new[] { ContextUser, marryuser });
            //    await db.SaveChangesAsync();
            //    emb.WithDescription($"Я, данной мне властью, обьявляю парой {user.Mention} и {Context.User.Mention}!");
            //}
            //else
            //{
            //    emb.WithDescription($"{user.Mention} отказался(лась) от свадьбы!");
            //}
            //_componentEventService.RemoveInteraction(AcceptButton);
            //_componentEventService.RemoveInteraction(DeclineButton);

            //emb.Footer.Text = null;
            //await mes.ModifyAsync(x => { x.Components = new ComponentBuilder().Build(); x.Embed = emb.Build(); });

        }

        [Aliases, Commands, Usage, Descriptions]
        [MarryPermission]
        public async Task divorce()
        {
            //using var _db = new Db();
            var ContextUser = await _db.GetUser(Context.User.Id);
            var MarryedUserId = Convert.ToUInt64(ContextUser.MarriageId);
            var userDs = Context.Guild.GetUser(MarryedUserId);
            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor).WithAuthor($" - Развод {userDs.Nickname}", Context.User.GetAvatarUrl());
            emb.WithDescription($"Вы успешно развелись с <@{MarryedUserId}>!");
            var MarryedUser = await _db.GetUser(MarryedUserId);
            ContextUser.MarriageId = null;
            MarryedUser.MarriageId = null;
            _db.User.UpdateRange(new[] { ContextUser, MarryedUser });
            await _db.SaveChangesAsync();

            await ReplyAsync(embed: emb.Build());
        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task reprole()
        {
            //using var _db = new Db();
            var RepRoles = _db.Roles_Reputation.OrderBy(u => u.Reputation).ToList();

            if (RepRoles.Any())
            {
                List<PageBuilder> pages = new();

                int groupSize = 5;

                for (int i = 0; i < RepRoles.Count; i += groupSize)
                {
                    int remaining = Math.Min(groupSize, RepRoles.Count - i);
                    var Roles = RepRoles.GetRange(i, remaining).ToList();
                    var page = new PageBuilder()
                        .WithAuthor($"🔨 Репутационные роли")
                        .WithColor(BotSettings.DiscordColor)
                        .WithDescription("");

                    foreach (var Role in Roles)
                    {
                        page.Description += $"{Role.Reputation} репутации - <@&{Role.RoleId}>\n";
                    }
                    pages.Add(page);
                }

                var paginator = new StaticPaginatorBuilder()
                    .WithActionOnTimeout(ActionOnStop.DeleteInput)
                    .WithActionOnCancellation(ActionOnStop.DeleteInput)
                    .AddUser(Context.User)
                    .WithPages(pages);

                var message = await _interactive.SendPaginatorAsync(paginator.Build(), Context.Channel, new TimeSpan(0, 0, 60));
            }
            else
            {
                var emb = new EmbedBuilder()
                        .WithAuthor($"🔨 Репутационные роли")
                        .WithColor(BotSettings.DiscordColor);
                emb.Author.Name += "отсутствуют ⚠️";
                var prefix = _db.Settings.FirstOrDefault().Prefix;
                RoleForOwnerMessage(RoleType.Reputation, prefix, ref emb);

                await ReplyAsync(embed: emb.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task levelrole()
        {
            //using var _db = new Db();
            var LvlRoles = _db.Roles_Level.OrderBy(u => u.Level).ToList();

            if (LvlRoles.Any())
            {
                List<PageBuilder> pages = new();

                int groupSize = 5;
                var TimeoutMessage = new TimeSpan(0, 0, 60);


                for (int i = 0; i < LvlRoles.Count; i += groupSize)
                {
                    int remaining = Math.Min(groupSize, LvlRoles.Count - i);
                    var Roles = LvlRoles.GetRange(i, remaining).ToList();

                    var page = new PageBuilder()
                        .WithAuthor($"🔨 Уровневые роли")
                        .WithColor(BotSettings.DiscordColor)
                        .WithDescription("")
                        .WithFooter($"Выбор активен {TimeoutMessage.TotalMinutes} минут.");


                    string RoleString = "";
                    string RolePriceString = "";
                    List<MultiSelectionOption<string>> options = new();
                    foreach (var thisrole in Roles)
                    {
                        var role = Context.Guild.GetRole(thisrole.RoleId);
                        options.Add(new MultiSelectionOption<string>(role.Name, 0, role.Id));
                        RoleString += $"<@&{thisrole.RoleId}>\n";
                        RolePriceString += $"{thisrole.Level}\n";
                    }

                    page.AddField("Role", RoleString, true);
                    page.AddField("Level", RolePriceString, true);



                    //foreach (var Role in Roles)
                    //{
                    //    page.Description += $"{Role.Level} уровень - <@&{Role.RoleId}>\n";
                    //}
                    pages.Add(page);
                }

                var paginator = new StaticPaginatorBuilder()
                    .WithActionOnTimeout(ActionOnStop.DeleteInput)
                    .WithActionOnCancellation(ActionOnStop.DeleteInput)
                    .AddUser(Context.User)
                    .WithPages(pages);

                var message = await _interactive.SendPaginatorAsync(paginator.Build(), Context.Channel, TimeoutMessage);
            }
            else
            {
                var emb = new EmbedBuilder()
                        .WithAuthor($"🔨 Уровневые роли")
                        .WithColor(BotSettings.DiscordColor);
                emb.Author.Name += "отсутствуют ⚠️";
                var prefix = _db.Settings.FirstOrDefault().Prefix;
                RoleForOwnerMessage(RoleType.Level, prefix, ref emb);

                await ReplyAsync(embed: emb.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task refferalrole()
        {
            //using var _db = new Db();
            var RefRoles = _db.ReferralRole.OrderBy(u => u.UserJoinedValue).ToList();

            if (RefRoles.Any())
            {
                List<PageBuilder> pages = new();

                int groupSize = 5;

                for (int i = 0; i < RefRoles.Count; i += groupSize)
                {
                    int remaining = Math.Min(groupSize, RefRoles.Count - i);
                    var Roles = RefRoles.GetRange(i, remaining).ToList();
                    var page = new PageBuilder()
                        .WithAuthor($"🔨 Рефферальные роли")
                        .WithColor(BotSettings.DiscordColor)
                        .WithDescription("");

                    foreach (var Role in Roles)
                    {
                        page.Description += $"<@&{Role.RoleId}>\n・[Зашло **{Role.UserJoinedValue}**] [Актив в теч. недели **{Role.UserWriteInWeekValue}**] [Имеют пятый lvl **{Role.UserUp5LevelValue}**]\n"; ;
                    }
                    pages.Add(page);
                }

                var paginator = new StaticPaginatorBuilder()
                    .WithActionOnTimeout(ActionOnStop.DeleteInput)
                    .WithActionOnCancellation(ActionOnStop.DeleteInput)
                    .AddUser(Context.User)
                    .WithPages(pages);

                var message = await _interactive.SendPaginatorAsync(paginator.Build(), Context.Channel, new TimeSpan(0, 0, 60));
            }
            else
            {
                var emb = new EmbedBuilder()
                        .WithAuthor($"🔨 Рефферальные роли")
                        .WithColor(BotSettings.DiscordColor);
                emb.Author.Name += "отсутствуют ⚠️";
                var prefix = _db.Settings.FirstOrDefault().Prefix;
                RoleForOwnerMessage(RoleType.Refferal, prefix, ref emb);

                await ReplyAsync(embed: emb.Build());
            }


            //var RefRoles = _db.ReferralRole.OrderBy(u => u.UserJoinedValue);

            //var embed = new EmbedBuilder()
            //    .WithAuthor($"🔨 Реферальные роли {(RefRoles.Any() ? "" : "отсутствуют ⚠️")}")
            //    .WithColor(BotSettings.DiscordColor);

            //foreach (var Role in RefRoles)
            //    embed.Description += $"<@&{Role.RoleId}> - [{Role.UserJoinedValue} приглашенных] [{Role.UserWriteInWeekValue} писали в течении недели] [{Role.UserUp5LevelValue} получили 5lvl]\n";

            //var prefix = _db.Settings.FirstOrDefault().Prefix;
            //RoleForOwnerMessage(RoleType.Refferal, prefix, ref embed);

            //await Context.Channel.SendMessageAsync("", false, embed.Build());

        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task buyrole()
        {
            //using var _db = new Db();
            var User = Context.User as SocketGuildUser;
            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor);

            var DBroles = _db.Roles_Buy;

            if (!DBroles.Any())
            {
                var prefix = _db.Settings.FirstOrDefault().Prefix;
                RoleForOwnerMessage(RoleType.Buy, prefix, ref emb);
                emb.WithAuthor($"🔨BuyRole - Роли не выставлены на продажу ⚠️");
                await ReplyAsync(embed: emb.Build());
            }
            else
            {
                var TimeoutMessage = new TimeSpan(0, 5, 0);

                var pageBuilder = new PageBuilder()
                    .WithColor(BotSettings.DiscordColor)
                    .WithAuthor("Покупка ролей")
                    .WithDescription($"")
                    .WithFooter($"Выбор активен {TimeoutMessage.TotalMinutes} минут.");


                string RoleString = "";
                string RolePriceString = "";
                List<MultiSelectionOption<string>> options = new();
                foreach (var thisrole in DBroles)
                {
                    var role = Context.Guild.GetRole(thisrole.RoleId);
                    options.Add(new MultiSelectionOption<string>(role.Name, 0, role.Id));
                    RoleString += $"<@&{thisrole.RoleId}>\n";
                    RolePriceString += $"{thisrole.Price}\n";
                }

                pageBuilder.AddField("Role", RoleString, true);
                pageBuilder.AddField("Price", RolePriceString, true);

                var multiSelection = new MultiSelectionBuilder<string>()
                    .WithSelectionPage(pageBuilder)
                    .WithActionOnTimeout(ActionOnStop.DeleteInput)
                    .WithOptions(options)
                    .WithStringConverter(x => x.Option)
                    .AddUser(Context.User)
                    .Build();

                var result = await _interactive.SendSelectionAsync(multiSelection, Context.Channel, TimeoutMessage);

                if (result.IsSuccess)
                {
                    var ThisRole = _db.Roles_Buy.FirstOrDefault(x => x.RoleId == result.Value.SelectId);
                    var ThisRoleDs = Context.Guild.GetRole(ThisRole.RoleId);
                    var userDb = _db.User.Include(x => x.Roles_User).FirstOrDefault(x => x.Id == User.Id);
                    if (userDb.money >= ThisRole.Price)
                    {
                        if (!userDb.Roles_User.Any(x => x.RoleId == ThisRole.Id))
                        {
                            _db.Roles_User.Add(new Roles_User { RoleId = ThisRole.RoleId, UserId = Context.User.Id });
                            userDb.money -= ThisRole.Price;
                            _db.User.Update(userDb);
                            await _db.SaveChangesAsync();
                            emb.WithDescription($"Вы успешно купили {ThisRoleDs.Mention} за {ThisRole.Price} coins");
                        }
                        else
                            emb.WithDescription($"Вы уже купили роль {ThisRoleDs.Mention}");

                        if (!User.Roles.Contains(ThisRoleDs))
                            await User.AddRoleAsync(ThisRoleDs.Id);
                    }
                    else
                        emb.WithDescription($"У вас недостаточно средств на счете!\nВаш баланс: {userDb.money} coins");


                    await result.Message.ModifyAsync(x =>
                    {
                        x.Embed = emb.Build();
                        x.Components = new ComponentBuilder().Build();
                    });
                }
            }
        }



        private enum RoleType : byte
        {
            Level,
            Reputation,
            Buy,
            Refferal
        }

        private void RoleForOwnerMessage(RoleType Type, string prefix, ref EmbedBuilder emb)
        {
            var ThisUser = Context.User as SocketGuildUser;
            if (!ThisUser.GuildPermissions.Administrator)
            {
                emb.WithDescription($"Попросите администратора сервера выставить роли <3");
                return;
            }

            string commandtype = "";
            string valuetype = "";
            switch (Type)
            {
                case RoleType.Level:
                    commandtype = "lra";
                    valuetype = "level";
                    break;
                case RoleType.Reputation:
                    commandtype = "rra";
                    valuetype = "reputation";
                    break;
                case RoleType.Buy:
                    commandtype = "bra";
                    valuetype = "price";
                    break;
                case RoleType.Refferal:
                    commandtype = "rfa";
                    valuetype = "values";
                    break;
            }

            emb.AddField("Добавить", $"{prefix}{commandtype} [ROLE] [{valuetype}]");
            emb.AddField("Удалить", $"{prefix}{commandtype} [ROLE]");
        }




        public enum KazinoChipEnum : byte
        {
            Red,
            Black
        }


        [Aliases, Commands, Usage, Descriptions]
        [ActivityPermission]
        public async Task kazino(KazinoChipEnum Fishka, ushort money)
        {
            //using var _db = new Db();
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor(" - Казино", Context.User.GetAvatarUrl());

            var account = await _db.GetUser(Context.User.Id);

            bool returnmessage = false;

            if (!(money >= 100 && money <= 30000))
            {
                returnmessage = true;
                emb.WithDescription($"Ставка может быть не меньше 100 и не больше 30000");
            }

            if (account.money < money)
            {
                emb.WithDescription($"Недостаточно средств для ставки.\nВаш баланс: {account.money} coins");
                returnmessage = true;
            }

            if (returnmessage)
            {
                await ReplyAsync(embed: emb.Build());
                return;
            }


            var RandomNumber = new PcgRandom().Next(0, 100);
            string WinnerText = Fishka.ToString();
            if (RandomNumber > 50)
            {
                emb.Author.Name += "✔️ Выигрыш";
                account.money += money;
                var transaction = new TransactionUsers_Logs { Amount = money, RecipientId = account.Id, TimeTransaction = DateTime.Now, Type = TransactionUsers_Logs.TypeTransation.Kazino };
                _db.TransactionUsers_Logs.Add(transaction);
            }
            else
            {
                WinnerText = $"{(Fishka == 0 ? KazinoChipEnum.Black : KazinoChipEnum.Red)}";
                account.money -= money;
                emb.Author.Name += "❌ Проигрыш";
            }
            emb.WithDescription($"Выпало: {WinnerText}\nZeroCoin: {account.money}");
            _db.User.Update(account);
            await _db.SaveChangesAsync();

            await ReplyAsync(embed: emb.Build());
        }



        //[Aliases, Commands, Usage, Descriptions]
        //public async Task emojigiftshop(ulong number = 0, ulong price = 0)
        //{
        //    var emb = new EmbedBuilder()
        //        .WithColor(BotSettings.DiscordColor)
        //        .WithAuthor("Магазин эмодзи");

        //    if (number == 0 && price == 0)
        //    {
        //        var gifts = _db.EmojiGift.Where(x => x.PriceTrade != 0).ToList();
        //        var giftsOrdered = gifts.OrderBy(x => x.Emoji.Factor).ThenBy(x => x.PriceTrade).ToList();
        //        if (giftsOrdered.Count == 0)
        //        {
        //            var prefix = _db.Settings.FirstOrDefault().Prefix;
        //            emb.WithDescription("Эмодзи на продажу еще не выставлены!").WithFooter($"Выставить - {prefix}emojigiftshop [number] [price]");
        //            await Context.Channel.SendMessageAsync("", false, emb.Build());
        //        }
        //        else
        //        {
        //            int CountSlot = 3;
        //            var Id = await new ListBuilder(_componentEventService).ListButtonSliderBuilder(giftsordered, emb, "emojigiftshop", Context, CountSlot, true);


        //            if (Id == (0, 0))
        //                return;
        //            var UserBuyed = await db.GetUser(Context.User.Id);
        //            var product = giftsordered[Id.Item2];
        //            if (UserBuyed.money < product.PriceTrade)
        //            {
        //                emb.WithDescription($"Недостаточно денег для покупки {product.Name}.\nВам нехватает: {product.PriceTrade - UserBuyed.money} coins");

        //                await Context.Channel.ModifyMessageAsync(Id.MessageId, x => { x.Embed = emb.Build(); });

        //                return;
        //            }

        //            var UserGetted = await db.GetUser(product.UserId);

        //            UserBuyed.money -= product.PriceTrade;
        //            UserGetted.money += product.PriceTrade;

        //            product.UserId = Context.User.Id;
        //            product.PriceTrade = 0;
        //            await db.SaveChangesAsync();
        //            emb.WithDescription($"Вы успешно купили {product.Name} за {product.PriceTrade}");

        //            await Context.Channel.ModifyMessageAsync(Id.MessageId, x => { x.Embed = emb.Build(); });
        //        }
        //    }
        //    else
        //    {
        //        var Emojiinfo = _db.EmojiGift.FirstOrDefault(x => x.Id == number);
        //        if (Emojiinfo is null)
        //            emb.WithDescription("Эмодзи с таким номером не найден.");
        //        else
        //        {
        //            var prefix = _db.Settings.FirstOrDefault().Prefix;
        //            if (Emojiinfo.PriceTrade == 0)
        //            {
        //                if (price == 0)
        //                    emb.WithDescription($"Для выставления эмодзи на продажу, нужно выставить его цену!\n{prefix}emojigiftshop [number] [price]");
        //                else
        //                {
        //                    Emojiinfo.PriceTrade = price;
        //                    emb.WithDescription("Вы успешно выставили эмодзи на продажу!");
        //                    await _db.SaveChangesAsync();
        //                }
        //            }
        //            else
        //            {
        //                if (price == 0)
        //                {
        //                    Emojiinfo.PriceTrade = 0;
        //                    emb.WithDescription("Вы успешно сняли эмодзи с продажу!");
        //                }
        //                else
        //                {
        //                    emb.WithDescription($"Вы успешно сменили цену эмодзи с {Emojiinfo.PriceTrade} на {price} coins");
        //                    Emojiinfo.PriceTrade = price;
        //                }
        //                await _db.SaveChangesAsync();
        //            }

        //        }
        //        await Context.Channel.SendMessageAsync("", false, emb.Build());
        //    }

        //}
    }
}