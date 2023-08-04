using XBOT.Services.Configuration;
using XBOT.Services;
using XBOT.Services.Attribute;
using XBOT.DataBase.Models.Roles_data;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Discord;

namespace XBOT.Modules.Command
{
    [Summary("Пользовательские\nкоманды"), Name("User")]
    public class UserModule : ModuleBase<SocketCommandContext>
    {
        private ComponentEventService _componentEventService;

        public UserModule(ComponentEventService componentEventService)
        {
            _componentEventService = componentEventService;
        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task usertop()
        {
            using (db db = new())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor)
                                            .WithAuthor($"Топ пользователей", Context.User.GetAvatarUrl());

                var users = db.User;

                var userLevel = users.AsEnumerable().OrderBy(x => x.XP).Take(5);
                var userRep = users.AsEnumerable().OrderBy(x => x.reputation).Take(5);
                var userMoney = users.AsEnumerable().OrderBy(x => x.money).Take(5);


                string GetTopUsersText(IEnumerable<ulong> Ids, IEnumerable<ulong> Value, string ValueName)
                {
                    var stringBuilder = new StringBuilder();
                    if (Ids.Count() > 5)
                    {
                        for (int i = 0; i < Ids.Count(); i++)
                        {
                            stringBuilder.AppendLine($"{i}.<@{Ids.ElementAt(i)}> - {Value.ElementAt(i)} {ValueName}");
                        }
                    }
                    else
                        stringBuilder.AppendLine("Недостаточно данных");
                    
                    return stringBuilder.ToString();
                }

                string Text = GetTopUsersText(userLevel.Select(x=>x.Id), userLevel.Select(x => (ulong)x.Level), "уровень");
                emb.AddField("ТОП УРОВЕНЬ 📈", Text, true);

                Text = GetTopUsersText(userRep.Select(x => x.Id), userRep.Select(x => x.reputation), "репутации");
                emb.AddField("ТОП РЕПУТАЦИИ ⚜️", Text, true);

                Text = GetTopUsersText(userMoney.Select(x => x.Id), userMoney.Select(x => x.money), "coins");
                emb.AddField("ТОП COINS 🏧", Text, true);


                await Context.Channel.SendMessageAsync($"", embed: emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task sex(SocketGuildUser user)
        {
            using (db db = new())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor)
                                            .WithAuthor($"Чпокан чпокан чпокан", Context.User.GetAvatarUrl());

                var thisuserDb = await db.GetUser(Context.User.Id);
                var mentionuserDb = await db.GetUser(user.Id);
                if(thisuserDb.BirthDate.Year != 1 && thisuserDb.BirthDate.Year <= 18 ||
                   mentionuserDb.BirthDate.Year != 1 && mentionuserDb.BirthDate.Year <= 18)
                {
                    emb.WithDescription("Какой секс, в школу пора...").WithFooter("Паспорт покажи... (укажите день рождения)");
                    await Context.Channel.SendMessageAsync($"", embed: emb.Build());
                    return;
                }

                var AcceptButton = Guid.NewGuid().ToString();
                var DeclineButton = Guid.NewGuid().ToString();
                var builder = new ComponentBuilder()
                                .WithButton("Принять", AcceptButton, ButtonStyle.Success)
                                .WithButton("Отклонить", DeclineButton, ButtonStyle.Danger);
                var mes = await Context.Channel.SendMessageAsync("", false, emb.Build(), components: builder.Build());

                var userInteraction = new ComponentEvent<SocketMessageComponent>(new HashSet<string> { AcceptButton, DeclineButton });
                _componentEventService.AddInteraction(new HashSet<string> { AcceptButton, DeclineButton }, userInteraction);

                var selectedOption = await userInteraction.WaitForInteraction();
                if (selectedOption == null)
                {
                    emb.WithDescription($"{user.Mention} не захотел(а) чпокаца!");
                }
                else if (selectedOption.Data.CustomId == AcceptButton)
                {
                    thisuserDb.CountSex += 1;
                    mentionuserDb.CountSex += 1;
                    await db.SaveChangesAsync();
                    emb.WithDescription($"{user.Mention} 💕 {Context.User.Mention} захубабубились!");
                }
                else
                {
                    emb.WithDescription($"{user.Mention} отказался(лась) от чпоканья!");
                }
                _componentEventService.RemoveInteraction(AcceptButton);
                _componentEventService.RemoveInteraction(DeclineButton);

                emb.Footer.Text = null;
                await mes.ModifyAsync(x => { x.Components = new ComponentBuilder().Build(); x.Embed = emb.Build(); });



            }
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task BirthDateSet(string date)
        {
            using (db db = new())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor)
                                            .WithAuthor($"День рождения", Context.User.GetAvatarUrl());
                DateOnly dateConvert;

                if (!DateOnly.TryParseExact(date, "dd.MM.yyyy", out dateConvert))
                {
                    emb.WithDescription($"Введит дату в формате: {DateOnly.FromDateTime(DateTime.Now)}");
                    await Context.Channel.SendMessageAsync("",false, emb.Build());
                    return;
                }

                emb.WithDescription($"Для выставления даты [{date}], вам необходимо подтвердить ее, так как в будущем сменить ее будет невозможно.")
                   .WithFooter("Подтверждение доступно в течении 60 секунд");


                var AcceptButton = Guid.NewGuid().ToString();
                var DeclineButton = Guid.NewGuid().ToString();
                var builder = new ComponentBuilder()
                                .WithButton("Принять", AcceptButton, ButtonStyle.Success)
                                .WithButton("Отклонить", DeclineButton, ButtonStyle.Danger);
                var mes = await Context.Channel.SendMessageAsync("", false, emb.Build(), components: builder.Build());

                var userInteraction = new ComponentEvent<SocketMessageComponent>(new HashSet<string> { AcceptButton, DeclineButton });
                _componentEventService.AddInteraction(new HashSet<string> { AcceptButton, DeclineButton }, userInteraction);

                var selectedOption = await userInteraction.WaitForInteraction();
                if (selectedOption == null)
                {
                    await mes.DeleteAsync();
                }
                else if (selectedOption.Data.CustomId == AcceptButton)
                {
                    emb.WithDescription($"Вы успешно выставили дату рождения.\nДата: {date}");
                    var user = await db.GetUser(Context.User.Id);
                    user.BirthDate = dateConvert;
                    await db.SaveChangesAsync();
                    await mes.ModifyAsync(x => { x.Components = new ComponentBuilder().Build(); x.Embed = emb.Build(); });
                }
                else
                {
                    await mes.DeleteAsync();
                }

                _componentEventService.RemoveInteraction(AcceptButton);
                _componentEventService.RemoveInteraction(DeclineButton);
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task userinfo(SocketGuildUser user = null)
        {
            using (db _db = new())
            {
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

                string DailyRep = null;
                TimeToDaily = UserDataBase.daily_Time - DateTime.Now;
                if (TimeToDaily.TotalSeconds < -1)
                    DailyRep = "Репутация - доступна сейчас!";
                else
                    DailyRep = $"До репутации - {TimeToDaily.Hours}:{TimeToDaily.Minutes}:{TimeToDaily.Seconds}";

                string Marryed = "Не состоит";
                if (Convert.ToUInt64(UserDataBase.MarriageId) != 0)
                {
                    string Timemarryed = string.Empty;
                    var Time = DateTime.Now - UserDataBase.MarriageTime;
                    if (Time.TotalSeconds < 60)
                        Timemarryed = "Меньше минуты";
                    else if (Time.TotalMinutes <= 60)
                        Timemarryed = $"{Math.Round(Time.TotalMinutes)} минут";
                    else if (Time.TotalHours <= 24)
                        Timemarryed = $"{Math.Round(Time.TotalHours)} часов";
                    else
                        Timemarryed = $"{Math.Round(Time.TotalDays)} дней";

                    Marryed = $"Половинка: <@{UserDataBase.MarriageId}>\nВ браке: {Timemarryed}";
                }
                if(UserDataBase.BirthDate.Year != 1 && UserDataBase.BirthDate.Year >= 18)
                    Marryed += $"\nКол-во половых партнеров: {UserDataBase.CountSex}";

                emb.AddField("Отношения", Marryed, true);

                emb.AddField("Репутация", $"Количество: {UserDataBase.reputation}\n{DailyRep}", true);

                emb.AddField("Coins", $"Количество: {UserDataBase.money}\nКомбо: {UserDataBase.streak}\n{DailyCoin}", true);

                var Settings = _db.Settings.FirstOrDefault();
                emb.AddField("Реферальная система", $"{Settings.Prefix}refferal", true);

                var TimePublic = ConvertTime(UserDataBase.voiceActive_public);
                var TimePrivate = ConvertTime(UserDataBase.voiceActive_private);

                string ConvertTime(TimeSpan Time)
                    => $"{(int)Time.TotalHours:00}:{Time.Minutes:00}:{Time.Seconds:00}";


                uint count = Convert.ToUInt32(UserDataBase.Level * User.PointFactor * UserDataBase.Level);
                uint countNext = Convert.ToUInt32((UserDataBase.Level + 1) * User.PointFactor * (UserDataBase.Level + 1));
                emb.AddField("Опыт", $"Уровень: {UserDataBase.Level}\nОпыт: {UserDataBase.XP - count}/{countNext - count}\nАктивность в голосовых чатах: {TimePublic}\nАктивность в приватных чатах: {TimePrivate}", false);


                if (UserDataBase.BirthDate.Year != 1)
                    emb.AddField("День рождения", UserDataBase.BirthDate, true);

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task refferal()
        {
            using (db _db = new())
            {
                var emb = new EmbedBuilder()
                    .WithColor(BotSettings.DiscordColor)
                    .WithAuthor($"Реферальная система");
                var userDs = Context.User as SocketGuildUser;
                var userDb = _db.User.Include(x => x.MyInvites).ThenInclude(x => x.ReferralLinks).ThenInclude(x => x.User).FirstOrDefault(x => x.Id == userDs.Id);
                var UserValue = Refferal_Service.GetRefferalValue(userDb);

                var RefferalRoles = _db.ReferralRole.OrderBy(x=>x.UserJoinedValue).ToList();

                var ThisRole = RefferalRoles.FirstOrDefault(x => UserValue.CountRef >= x.UserJoinedValue && UserValue.WriteInWeek >= x.UserWriteInWeekValue && UserValue.Level5up >= x.UserUp5LevelValue);
                var indexThisRole = RefferalRoles.IndexOf(ThisRole);
                var NextRole = RefferalRoles[indexThisRole + 1];

                emb.WithDescription("Это реферальная система друзей. Приглашая на сервер друга, вы получаете за него ачько и дополнительные плюшки.\n" +
                                    "Чем больше вы пригласите активных друзей, тем лучше. Мы надеемся что вы не будете тревожить незнакомых людей, ведь это не круто!\n\n" +
                                    $"Ваша текущая роль: <@&{ThisRole.Id}> -> <@&{NextRole.Id}>\n" +
                                    $"Приведенных клиентов: {UserValue.CountRef}/{ThisRole.UserJoinedValue}\n" +
                                    $"Писали в течении недели: {UserValue.WriteInWeek}/{ThisRole.UserWriteInWeekValue}\n" +
                                    $"Достигли 5 уровня: {UserValue.Level5up}/{ThisRole.UserUp5LevelValue}")
                    .WithFooter("Роль может выдаваться в течении часа.");

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        //[Aliases, Commands, Usage, Descriptions]
        //[ActivityPermission]
        //public async Task warnappilation(ulong warnid = 0)
        //{
        //    using (db _db = new())
        //    {
        //        var emb = new EmbedBuilder()
        //            .WithColor(BotSettings.DiscordColor)
        //            .WithAuthor($"Аппеляция на нарушение");
                    

        //        if(warnid == 0)
        //        {
        //            emb.WithDescription("");
        //            var user = _db.User.Include(x => x.User_Warn).ThenInclude(x => x.UnWarn).FirstOrDefault(x => x.Id == Context.User.Id);
        //            var OldDate = DateTime.Now.AddDays(-3);
        //            var ActiveWarns = user.User_Warn.Where(x => x.TimeSetWarn >= OldDate);
        //            if (ActiveWarns.Any())
        //            {
        //                foreach (var warn in ActiveWarns)
        //                {
        //                    emb.Description += $"{warn.Id}.<@{warn.Admin_Id}> [{warn.TimeSetWarn:dd.MM HH:mm}] Причина: {warn.Reason}\n";
        //                }
        //                var Settings = _db.Settings.FirstOrDefault();
        //                emb.WithFooter($"Чтоб подать аппеляция - {Settings.Prefix}warnappilation [number]");
        //            }
        //            else
        //                emb.WithDescription("Нарушений за 3 дня не найдено!")
        //                   .WithColor(BotSettings.DiscordColorError);
                    
        //        }
        //        else
        //        {
        //            var warninfo = _db.User_Warn.Include(x => x.User).FirstOrDefault(x=>x.Id == warnid);
        //            if (warninfo is null)
        //                emb.WithDescription($"Нарушение под номером [{warnid}] не найдено.").WithColor(BotSettings.DiscordColorError);
        //            else
        //            {
        //                var Settings = _db.Settings.Include(x=>x.AdminRole).FirstOrDefault();
        //                List<SocketGuildUser> users = new();
        //                foreach (var user_perm in _db.User_Permission)
        //                {
        //                    var User = Context.Guild.GetUser(user_perm.User_Id);
        //                    if(User != null && User.Roles.Any(x=>x.Id == Settings.AdminRole.Id))
        //                    {
        //                        users.Add(User);
        //                    }
        //                }

        //                var PinAdmin = users.ElementAt(new Random().Next(0, users.Count));

        //                var Appilation = new User_UnWarn {Admin_Id = PinAdmin.Id, Warn_Id = warnid,ReviewAdd = DateTime.Now,Status = User_UnWarn.WarnStatus.review};
        //                _db.User_UnWarn.Add(Appilation);
        //                await _db.SaveChangesAsync();
                        
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
            using (db _db = new())
            {
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
                currentUser.money -= coin;
                transfUser.money += coin;

                _db.User.UpdateRange(new User[] { currentUser, transfUser });
                await _db.SaveChangesAsync();
                SendMessage($"Перевод в размере {coin} StarCoin успешно прошел.");

                async void SendMessage(string description)
                {
                    emb.WithDescription(description);
                    await ReplyAsync("", embed: emb.Build());
                }
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [ActivityPermission]
        public async Task reputation(SocketGuildUser RepUser)
        {
            using (db db = new())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor)
                                            .WithAuthor($" - Репутация 🏧", Context.User.GetAvatarUrl());
                var userDB = await db.GetUser(Context.User.Id);
                var DateNow = DateTime.Now;


                DateTime Daily = userDB.reputation_Time;

                if (Daily.Year == 1)
                    Daily = DateNow;

                if (DateNow >= Daily)
                {
                    if (RepUser.Id == Context.User.Id)
                        emb.WithDescription("Повысить репутацию самому себе нельзя.");
                    else
                    {
                        if (userDB.lastReputationUserId == 0 || RepUser.Id != userDB.lastReputationUserId)
                        {
                            var UserDb = await db.GetUser(RepUser.Id);
                            await RepRole(RepUser, UserDb.reputation);
                            UserDb.reputation += 1;
                            var TransferLog = new TransactionUsers_Logs { SenderId = userDB.Id, RecipientId = UserDb.Id, Amount = 1, TimeTransaction = DateTime.Now, Type = TransactionUsers_Logs.TypeTransation.Reputation };

                            userDB.lastReputationUserId = UserDb.Id;
                            userDB.reputation_Time = DateNow.AddHours(User.PeriodHours);

                            emb.WithDescription($"{Context.User.Mention} повысил репутацию {RepUser.Mention}\nРепутация: +{UserDb.reputation}\nСледующая репутация через {User.PeriodHours} часов"); // {Math.Round((UserThis.DailyRep - DateTime.Now).TotalHours)}
                            db.TransactionUsers_Logs.Add(TransferLog);
                            db.User.UpdateRange(new[] { UserDb, userDB });
                            await db.SaveChangesAsync();
                        }
                        else
                            emb.WithDescription("Вы не можете выдать репутацию одному и тому же пользователю 2 раза подряд.");
                    }
                }
                else
                {
                    var TimeToDaily = Daily - DateNow;
                    if (TimeToDaily.TotalSeconds >= 3600)
                        emb.WithDescription($"Дождитесь {TimeToDaily.Hours} часов и {TimeToDaily.Minutes} минут чтобы выдать репутацию!");
                    else
                        emb.WithDescription($"Дождитесь {(TimeToDaily.TotalSeconds > 60 ? $"{TimeToDaily.Minutes} минут и " : "")} {TimeToDaily.Seconds} секунд чтобы выдать репутацию!");
                }

                await ReplyAsync("", embed: emb.Build());
            }
        }

        public static async Task RepRole(SocketGuildUser UserDiscord, ulong Reputation)
        {
            using (db _db = new())
            {
                var Roles = _db.Roles_Reputation.OrderBy(x => x.Reputation);
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
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task daily()
        {
            using (db db = new())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor)
                                            .WithAuthor($"Ежедневное пособие Coins 🏧", Context.User.GetAvatarUrl());

                var userDB = await db.GetUser(Context.User.Id);
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
                        db.TransactionUsers_Logs.Add(TransferLog);
                        emb.WithDescription($"Монет получено: {result}");
                    }
                    emb.Description += $"\nКомбо: {userDB.streak}\nСледующее получение coins через {User.PeriodHours} часов";

                    db.User.Update(userDB);
                    await db.SaveChangesAsync();
                }
                else
                {
                    var TimeToDaily = Daily - DateNow;
                    if (TimeToDaily.TotalSeconds >= 3600)
                        emb.WithDescription($"Дождитесь {TimeToDaily.Hours} часов и {TimeToDaily.Minutes} минут чтобы получить coins!");
                    else
                        emb.WithDescription($"Дождитесь {(TimeToDaily.TotalSeconds > 60 ? $"{TimeToDaily.Minutes} минут и " : "")} {TimeToDaily.Seconds} секунд чтобы получить coins!");
                }

                await Context.Channel.SendMessageAsync("", embed: emb.Build());
            }
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
            using (db _db = new())
            {
                var emb = new EmbedBuilder()
                            .WithColor(BotSettings.DiscordColor)
                            .WithAuthor($"💞 Женидьба - Ошибка");

                async void SendMessage()
                {
                    await Context.Channel.SendMessageAsync("", false, emb.Build());
                    return;
                }

                if (Context.User.Id == user.Id)
                {
                    emb.WithDescription("Я знаю что ты любишь себя, но к сожалению на себе жениться нельзя!");
                    SendMessage();
                }

                var marryuser = await _db.GetUser(user.Id);

                if (marryuser.MarriageId != null)
                {
                    var Prefix = _db.Settings.FirstOrDefault();
                    emb.WithDescription($"{user} женат(а), этому пользователю сначала нужно развестись!")
                       .WithFooter($"Развестить - {Prefix.Prefix}divorce");
                    SendMessage();
                }

                var ContextUser = await _db.GetUser(Context.User.Id);
                emb.WithAuthor($"{Context.User} 💞 {user}")
                   .WithDescription($"{Context.User.Mention} отправил заявку на помолвку {user.Mention}.\n\n{user.Username}, хочешь пожениться?")
                   .WithFooter("Заявка активна 60 секунд.")
                   .WithThumbnailUrl(user.GetAvatarUrl());

                var AcceptButton = Guid.NewGuid().ToString();
                var DeclineButton = Guid.NewGuid().ToString();
                var builder = new ComponentBuilder()
                                .WithButton("Принять", AcceptButton, ButtonStyle.Success)
                                .WithButton("Отклонить", DeclineButton, ButtonStyle.Danger);
                var mes = await Context.Channel.SendMessageAsync("", false, emb.Build(), components: builder.Build());

                var userInteraction = new ComponentEvent<SocketMessageComponent>(new HashSet<string> { AcceptButton, DeclineButton });
                _componentEventService.AddInteraction(new HashSet<string> { AcceptButton, DeclineButton }, userInteraction);

                var selectedOption = await userInteraction.WaitForInteraction();
                if (selectedOption == null)
                {
                    emb.WithDescription($"{user.Mention} не успел(а) принять заявку!");
                }
                else if (selectedOption.Data.CustomId == AcceptButton)
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
                _componentEventService.RemoveInteraction(AcceptButton);
                _componentEventService.RemoveInteraction(DeclineButton);

                emb.Footer.Text = null;
                await mes.ModifyAsync(x => { x.Components = new ComponentBuilder().Build(); x.Embed = emb.Build(); });
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        [MarryPermission]
        public async Task divorce()
        {
            using (db _db = new())
            {
                var ContextUser = await _db.GetUser(Context.User.Id);
                var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor).WithAuthor($" - Развод <@{ContextUser.MarriageId}>", Context.User.GetAvatarUrl());
                emb.WithDescription($"Вы успешно развелись с <@{ContextUser.MarriageId}>!");
                var MarryedUser = await _db.GetUser(Convert.ToUInt64(ContextUser.MarriageId));
                ContextUser.MarriageId = null;
                MarryedUser.MarriageId = null;
                _db.User.UpdateRange(new[] { ContextUser, MarryedUser });
                await _db.SaveChangesAsync();

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }



        [Aliases, Commands, Usage, Descriptions]
        public async Task reprole()
        {
            using (db _db = new())
            {
                var RepRoles = _db.Roles_Reputation.OrderBy(u => u.Reputation);

                var embed = new EmbedBuilder()
                    .WithAuthor($"🔨 Репутационные роли {(RepRoles.Any() ? "" : "отсутствуют ⚠️")}")
                    .WithColor(BotSettings.DiscordColor);

                foreach (var Role in RepRoles)
                    embed.Description += $"{Role.Reputation} репутации - <@&{Role.RoleId}>\n";

                var prefix = _db.Settings.FirstOrDefault().Prefix;
                RoleForOwnerMessage(RoleType.Reputation, prefix, ref embed);

                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task levelrole()
        {
            using (db _db = new())
            {
                var LvlRoles = _db.Roles_Level.OrderBy(u => u.Level);

                var embed = new EmbedBuilder()
                    .WithAuthor($"🔨 Уровневые роли {(LvlRoles.Any() ? "" : "отсутствуют ⚠️")}")
                    .WithColor(BotSettings.DiscordColor);

                foreach (var Role in LvlRoles)
                    embed.Description += $"{Role.Level} уровень - <@&{Role.RoleId}>\n";

                var prefix = _db.Settings.FirstOrDefault().Prefix;
                RoleForOwnerMessage(RoleType.Level, prefix, ref embed);

                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task refferalrole()
        {
            using (db _db = new())
            {
                var RefRoles = _db.ReferralRole.OrderBy(u => u.UserJoinedValue);

                var embed = new EmbedBuilder()
                    .WithAuthor($"🔨 Реферальные роли {(RefRoles.Any() ? "" : "отсутствуют ⚠️")}")
                    .WithColor(BotSettings.DiscordColor);

                foreach (var Role in RefRoles)
                    embed.Description += $"<@&{Role.RoleId}> - [{Role.UserJoinedValue} приглашенных] [{Role.UserWriteInWeekValue} писали в течении недели] [{Role.UserUp5LevelValue} получили 5lvl]\n";

                var prefix = _db.Settings.FirstOrDefault().Prefix;
                RoleForOwnerMessage(RoleType.Refferal, prefix, ref embed);

                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task buyrole()
        {
            using (db _db = new())
            {
                var User = Context.User as SocketGuildUser;
                var embed = new EmbedBuilder().WithColor(BotSettings.DiscordColor);
                var userDb = await _db.GetUser(User.Id);
                var DBroles = _db.Roles_Buy.AsEnumerable();

                if (!DBroles.Any())
                {
                    var prefix = _db.Settings.FirstOrDefault().Prefix;
                    RoleForOwnerMessage(RoleType.Level, prefix, ref embed);
                    embed.WithAuthor($"🔨BuyRole - Роли не выставлены на продажу ⚠️");
                }
                else
                {
                    embed.WithAuthor($"🔨 Покупка ролей");
                    DBroles = DBroles.OrderBy(u => u.Price);
                    int CountSlot = 3;
                    var Id = await new ListBuilder(_componentEventService).ListButtonSliderBuilder(DBroles, embed, "buyrole", Context, CountSlot, true);


                    if (Id == (0, 0))
                        return;

                    var DBrole = DBroles.ToList()[Id.Item2];
                    var Role = Context.Guild.GetRole(DBrole.RoleId);

                    if (userDb.money >= DBrole.Price)
                    {
                        if (!_db.Roles_User.Any(x => x.RoleId == Role.Id && x.UserId == Context.User.Id))
                        {
                            _db.Roles_User.Add(new Roles_User { RoleId = Role.Id, UserId = Context.User.Id });
                            userDb.money -= DBrole.Price;
                            _db.User.Update(userDb);
                            await _db.SaveChangesAsync();
                            embed.WithDescription($"Вы успешно купили {Role.Mention} за {DBrole.Price} coins");
                        }
                        else
                            embed.WithDescription($"Вы уже купили роль {Role.Mention}");

                        if (!User.Roles.Contains(Role))
                            await User.AddRoleAsync(Role.Id);
                    }
                    else
                        embed.WithDescription($"У вас недостаточно средств на счете!\nВаш баланс: {userDb.money} coins");
                }

                await Context.Channel.SendMessageAsync("", false, embed.Build());
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




        //[Aliases, Commands, Usage, Descriptions]
        //public async Task emojigiftshop(ulong number = 0, ulong price = 0)
        //{
        //    using (var _db = new db())
        //    {
        //        var emb = new EmbedBuilder()
        //            .WithColor(BotSettings.DiscordColor)
        //            .WithAuthor("Магазин эмодзи");

        //        if(number == 0 && price == 0)
        //        {
        //            var gifts = _db.EmojiGift.Include(x => x.User).Include(x => x.Emoji).Where(x => x.PriceTrade != 0).ToList();
        //            var giftsordered = gifts.OrderBy(x => x.Emoji.Factor).ThenBy(x => x.PriceTrade).ToList();
        //            if (giftsordered.Count == 0)
        //            {
        //                var prefix = _db.Settings.FirstOrDefault().Prefix;
        //                emb.WithDescription("Эмодзи на продажу еще не выставлены!").WithFooter($"Выставить - {prefix}emojigiftshop [number] [price]");
        //                await Context.Channel.SendMessageAsync("", false, emb.Build());
        //            }
        //            else
        //            {
        //                int CountSlot = 3;
        //                var Id = await new ListBuilder(_componentEventService).ListButtonSliderBuilder(giftsordered, emb, "emojigiftshop", Context, CountSlot, true);


        //                if (Id == (0, 0))
        //                    return;
        //                var UserBuyed = await _db.GetUser(Context.User.Id);
        //                var product = giftsordered[Id.Item2];
        //                if (UserBuyed.money < product.PriceTrade)
        //                {
        //                    emb.WithDescription($"Недостаточно денег для покупки {product.Name}.\nВам нехватает: {product.PriceTrade - UserBuyed.money} coins");

        //                    await Context.Channel.ModifyMessageAsync(Id.MessageId, x => { x.Embed = emb.Build(); });

        //                    return;
        //                }

        //                var UserGetted = await _db.GetUser(product.UserId);

        //                UserBuyed.money -= product.PriceTrade;
        //                UserGetted.money += product.PriceTrade;

        //                product.UserId = Context.User.Id;
        //                product.PriceTrade = 0;
        //                await _db.SaveChangesAsync();
        //                emb.WithDescription($"Вы успешно купили {product.Name} за {product.PriceTrade}");

        //                await Context.Channel.ModifyMessageAsync(Id.MessageId, x => { x.Embed = emb.Build(); });
        //            }
        //        }
        //        else
        //        {
        //            var Emojiinfo = _db.EmojiGift.Include(x=>x.Emoji).Include(x=>x.User).FirstOrDefault(x=>x.Id == number);
        //            if(Emojiinfo is null)
        //            {
        //                emb.WithDescription("Эмодзи с таким номером не найден.");
        //            }
        //            else
        //            {
        //                var prefix = _db.Settings.FirstOrDefault().Prefix;
        //                if (Emojiinfo.PriceTrade == 0)
        //                {
        //                    if(price == 0)
        //                        emb.WithDescription($"Для выставления эмодзи на продажу, нужно выставить его цену!\n{prefix}emojigiftshop [number] [price]");
        //                    else
        //                    {
        //                        Emojiinfo.PriceTrade = price;
        //                        emb.WithDescription("Вы успешно выставили эмодзи на продажу!");
        //                        await _db.SaveChangesAsync();
        //                    }
        //                }
        //                else
        //                {
        //                    if (price == 0)
        //                    {
        //                        Emojiinfo.PriceTrade = 0;
        //                        emb.WithDescription("Вы успешно сняли эмодзи с продажу!");
        //                    }
        //                    else
        //                    {
        //                        emb.WithDescription($"Вы успешно сменили цену эмодзи с {Emojiinfo.PriceTrade} на {price} coins");
        //                        Emojiinfo.PriceTrade = price;
        //                    }
        //                    await _db.SaveChangesAsync();
        //                }
                        
        //            }
        //            await Context.Channel.SendMessageAsync("",false, emb.Build());
        //        }
        //    }
        //}
    }
}