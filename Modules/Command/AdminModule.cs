using Fergun.Interactive;
using XBOT.DataBase.Models.Roles_data;
using XBOT.Services;
using XBOT.Services.Attribute;
using XBOT.Services.Configuration;
using static System.Net.Mime.MediaTypeNames;

namespace XBOT.Modules.Command
{
    [Summary("Админские\nкоманды"), Name("Admin")]
    [UserPermission(UserPermission.RolePermission.Admin)]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private readonly InteractiveService _interactive;
        private readonly Db _db;
        public AdminModule(InteractiveService interactive, Db db)
        {
            _interactive = interactive;
            _db = db;
        }

        public enum RoleTypeEnum : byte
        {
            Level,
            Reputation,
            Buy,
            Refferal
        }



        [Aliases, Commands, Usage, Descriptions]
        public async Task moderatoradd(SocketGuildUser user)
            => await PermissionAdd(UserPermission.RolePermission.Moder, user, false);

        [Aliases, Commands, Usage, Descriptions]
        public async Task moderatordel(SocketGuildUser user)
            => await PermissionDel(UserPermission.RolePermission.Moder, user);

        [Aliases, Commands, Usage, Descriptions]
        public async Task iventeradd(SocketGuildUser user)
            => await PermissionAdd(UserPermission.RolePermission.Iventer, user, false);

        [Aliases, Commands, Usage, Descriptions]
        public async Task iventerdel(SocketGuildUser user)
            => await PermissionDel(UserPermission.RolePermission.Iventer, user);

        [Aliases, Commands, Usage, Descriptions]
        [RequireOwner]
        public async Task adminadd(SocketGuildUser user)
            => await PermissionAdd(UserPermission.RolePermission.Admin, user, false);

        [Aliases, Commands, Usage, Descriptions]
        [RequireOwner]
        public async Task admindel(SocketGuildUser user)
            => await PermissionDel(UserPermission.RolePermission.Admin, user);

        public async Task PermissionAdd(UserPermission.RolePermission permission, SocketGuildUser user, bool unlimited)
        {
            //using var _db = new Db();
            string role = "";
            ulong RoleId = 0;
            var Settings = _db.Settings.FirstOrDefault();
            if (permission == UserPermission.RolePermission.Admin)
            {

                RoleId = Convert.ToUInt64(Settings.AdminRoleId);
                role = "админа";

            }
            else if (permission == UserPermission.RolePermission.Iventer)
            {
                RoleId = Convert.ToUInt64(Settings.IventerRoleId);
                role = "ивентера";
            }
            else
            {
                RoleId = Convert.ToUInt64(Settings.ModeratorRoleId);
                role = "модератора";
            }

            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"🔨 Добавить {role}");


            if (RoleId != 0)
            {
                var permissionDb = _db.User_Permission.FirstOrDefault(x => x.User_Id == user.Id);
                if (permissionDb == null)
                {
                    var userDb = await _db.GetUser(user.Id);

                    if (permission != UserPermission.RolePermission.Iventer)
                    {
                        var userPermission = new User_Permission { User_Id = userDb.Id, Unlimited = unlimited, Active = true };
                        _db.User_Permission.Add(userPermission);
                        await _db.SaveChangesAsync();
                    }
                    emb.WithDescription($"Вы успешно выдали {user.Mention} права {role}");
                }
                else
                {
                    emb.WithDescription($"Пользователь {user.Mention} уже обладает правами {role}");
                }

                await user.AddRoleAsync(RoleId);
            }
            else
                emb.WithDescription($"Роль {role} не назначена в системе!").WithColor(BotSettings.DiscordColorError);

            await ReplyAsync(embed: emb.Build());
        }

        public async Task PermissionDel(UserPermission.RolePermission permission, SocketGuildUser user)
        {
            //using var _db = new Db();
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"🔨 удалить привилигированного пользователя");
            var UserDb = await _db.GetUser(user.Id);
            ulong roleId = 0;
            string roleName = "";
            var Settings = _db.Settings.FirstOrDefault();
            var userPermission = _db.User_Permission.FirstOrDefault(x => x.User_Id == user.Id);
            if (permission == UserPermission.RolePermission.Admin)
            {
                roleName = "администратора";
                roleId = Convert.ToUInt64(Settings.AdminRoleId);
            }
            else if (permission == UserPermission.RolePermission.Moder)
            {
                roleName = "модератора";
                roleId = Convert.ToUInt64(Settings.ModeratorRoleId);
            }
            else
            {
                roleName = "ивентера";
                roleId = Convert.ToUInt64(Settings.IventerRoleId);
            }


            if (userPermission != null || permission == UserPermission.RolePermission.Iventer)
            {
                if (userPermission != null)
                {
                    userPermission.Active = false;
                    _db.Update(userPermission);
                    await _db.SaveChangesAsync();
                }
                emb.WithDescription($"Вы успешно удалили {roleName} {user.Mention}");
            }
            else
                emb.WithDescription($"Права {roleName} не найдены!");

            await user.RemoveRoleAsync(roleId);

            await ReplyAsync(embed: emb.Build());
        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task refferalroleadd(SocketRole role, uint Invite, uint WriteInWeek, uint Get5Level)
        {
            //using var _db = new Db();
            bool returnMessage = false;
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"🔨 Добавить реферальную роль");

            if (role.IsManaged)
                emb.WithDescription("Данную роль нельзя выставить.");

            var refrole = _db.ReferralRole.FirstOrDefault(x => x.RoleId == role.Id);
            if (refrole != null)
                emb.WithDescription($"Роль {role.Mention} уже выдается");

            var rolepos = role.Guild.CurrentUser.Roles.FirstOrDefault(x => x.Position > role.Position);
            if (rolepos == null)
                emb.WithDescription($"Позиция роли {role.Mention} находится выше, роли бота.\nПопросите икс поднять эту роль выше моей");

            if (returnMessage)
            {
                await ReplyAsync(embed: emb.Build());
                return;
            }

            var Settings = _db.Settings.FirstOrDefault();
            emb.WithDescription($"Роль {role.Mention} выставлена за:\n・{Invite} приглашений\n・{WriteInWeek} активных пользователей за неделю\n・{Get5Level} пользователей получивших 5 уровень")
               .WithFooter($"Посмотреть ваши рефферальные роли {Settings.Prefix}rf");

            if (!_db.Roles.Any(x => x.Id == role.Id))
                _db.Roles.Add(new Roles { Id = role.Id });

            _db.ReferralRole.Add(new DataBase.Models.Invites.DiscordInvite_ReferralRole() { RoleId = role.Id, UserJoinedValue = Invite, UserWriteInWeekValue = WriteInWeek, UserUp5LevelValue = Get5Level });

            await _db.SaveChangesAsync();
            await ReplyAsync(embed: emb.Build());

        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task refferalroledel(SocketRole role)
        {
            //using var _db = new Db();
            var refrole = _db.ReferralRole.FirstOrDefault(x => x.RoleId == role.Id);

            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"🔨 Удалить рефферальную роль");

            emb.WithDescription($"Рефферальная роль {role.Mention} ");
            if (refrole != null)
            {
                emb.Description += "удалена.";
                _db.Remove(refrole);
                await _db.SaveChangesAsync();
            }
            else
                emb.Description += $"не является рефферальной.";

            await ReplyAsync(embed: emb.Build());
        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task levelroleadd(SocketRole role, uint level) => await roleadd(role, level, RoleTypeEnum.Level);

        [Aliases, Commands, Usage, Descriptions]
        public async Task levelroledel(SocketRole role) => await roledel(role, RoleTypeEnum.Level);

        [Aliases, Commands, Usage, Descriptions]
        public async Task reproleadd(SocketRole role, uint level) => await roleadd(role, level, RoleTypeEnum.Reputation);

        [Aliases, Commands, Usage, Descriptions]
        public async Task reproledel(SocketRole role) => await roledel(role, RoleTypeEnum.Reputation);

        [Aliases, Commands, Usage, Descriptions]
        public async Task buyroleadd(SocketRole role, uint price) => await roleadd(role, price, RoleTypeEnum.Buy);

        [Aliases, Commands, Usage, Descriptions]
        public async Task buyroledel(SocketRole role) => await roledel(role, RoleTypeEnum.Buy);

        private async Task roleadd(SocketRole role, uint value, RoleTypeEnum Type)
        {
            //using var _db = new Db();
            string AuthorText = "";
            string DescriptionText = "";
            string YourRole = "";
            string Command = "";

            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"🔨 Добавить {AuthorText} роль");

            if (role.IsManaged)
            {
                SendMessage("Данную роль нельзя выставить.");
                return;
            }

            switch (Type)
            {
                case RoleTypeEnum.Level:
                    AuthorText = "уровневую";
                    DescriptionText = "уровень";
                    YourRole = "уровневые";
                    Command = "lr";

                    var lvlrole = _db.Roles_Level.FirstOrDefault(x => x.RoleId == role.Id);
                    if (lvlrole != null)
                    {
                        SendMessage($"Роль {role.Mention} уже выдается за {lvlrole.Level} {DescriptionText}");
                    }
                    break;
                case RoleTypeEnum.Reputation:
                    AuthorText = "репутационную";
                    DescriptionText = "репутации";
                    YourRole = "репутационные";
                    Command = "rr";

                    var reprole = _db.Roles_Reputation.FirstOrDefault(x => x.RoleId == role.Id);
                    if (reprole != null)
                    {
                        SendMessage($"Роль {role.Mention} уже выдается за {reprole.Reputation} {DescriptionText}");
                        return;
                    }
                    break;
                case RoleTypeEnum.Buy:
                    AuthorText = "магазинную";
                    DescriptionText = "coins";
                    YourRole = "магазинные";
                    Command = "br";

                    var buyrole = _db.Roles_Buy.FirstOrDefault(x => x.RoleId == role.Id);
                    if (buyrole != null)
                    {
                        SendMessage($"Роль {role.Mention} уже выдается за {buyrole.Price} {DescriptionText}");
                        return;
                    }
                    break;
            }


            async void SendMessage(string description, string text = "")
            {
                emb.WithDescription(description);
                await ReplyAsync(message: text, embed: emb.Build());
            }

            var rolepos = role.Guild.CurrentUser.Roles.FirstOrDefault(x => x.Position > role.Position);
            if (rolepos == null)
            {
                SendMessage($"Позиция роли {role.Mention} находится выше, роли бота.\nПопросите икс поднять эту роль выше моей", Context.Guild.Owner.Mention);
                return;
            }

            var Settings = _db.Settings.FirstOrDefault();
            emb.WithDescription($"Роль {role.Mention} выставлена за {value} {DescriptionText}")
               .WithFooter($"Посмотреть ваши {YourRole} роли {Settings.Prefix}{Command}");

            if (!_db.Roles.Any(x => x.Id == role.Id))
                _db.Roles.Add(new Roles { Id = role.Id });

            switch (Type)
            {
                case RoleTypeEnum.Level:
                    _db.Roles_Level.Add(new Roles_Level() { RoleId = role.Id, Level = value });
                    break;
                case RoleTypeEnum.Reputation:
                    _db.Roles_Reputation.Add(new Roles_Reputation() { RoleId = role.Id, Reputation = value });
                    break;
                case RoleTypeEnum.Buy:
                    _db.Roles_Buy.Add(new Roles_Buy() { RoleId = role.Id, Price = value });
                    break;
            }

            await _db.SaveChangesAsync();
            await ReplyAsync(embed: emb.Build());
        }
        private async Task roledel(SocketRole role, RoleTypeEnum Type)
        {
            //using var _db = new Db();
            string AuthorText = "";
            string DescriptionText = "";
            string DescriptionText2 = "";
            object roles = null;

            switch (Type)
            {
                case RoleTypeEnum.Level:
                    AuthorText = "уровневую";
                    DescriptionText = "Уровневая";
                    DescriptionText2 = "уровневой";
                    roles = _db.Roles_Level.FirstOrDefault(x => x.RoleId == role.Id);
                    break;
                case RoleTypeEnum.Reputation:
                    AuthorText = "репутационную";
                    DescriptionText = "Репутационная";
                    DescriptionText2 = "репутационной";
                    roles = _db.Roles_Reputation.FirstOrDefault(x => x.RoleId == role.Id);
                    break;
                case RoleTypeEnum.Buy:
                    AuthorText = "магазинную";
                    DescriptionText = "Магазинная";
                    DescriptionText2 = "магазинной";
                    roles = _db.Roles_Buy.FirstOrDefault(x => x.RoleId == role.Id);
                    break;
            }


            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"🔨 Удалить {AuthorText} роль");

            emb.WithDescription($"{DescriptionText} роль {role.Mention} ");
            if (roles != null)
            {
                emb.Description += "удалена.";
                _db.Remove(roles);
                await _db.SaveChangesAsync();
            }
            else
                emb.Description += $"не является {DescriptionText2}.";

            await ReplyAsync(embed: emb.Build());
        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task unwarn(SocketGuildUser user)
        {
            //using var _db = new Db();
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"unwarn {user}");

            var userwarned = await _db.GetUser(user.Id);
            if (userwarned.CountWarns == 0)
            {
                emb.WithDescription($"У пользователя {user.Mention} отстствуют нарушения.");
                await ReplyAsync(embed: emb.Build());
                return;
            }

            var Warns = _db.Guild_Warn.ToList();

            if (Warns.Count == userwarned.CountWarns)
            {
                foreach (var warn in userwarned.User_Warn)
                {
                    warn.WarnSkippedAfterUnban = true;
                }
                _db.Update(userwarned);
                await _db.SaveChangesAsync();
                await Context.Guild.RemoveBanAsync(user);
                emb.WithDescription($"Пользователь разбанен. И все нарушения, анулированы.");
                await ReplyAsync(embed: emb.Build());
            }
            else
            {
                var ThisWarn = userwarned.User_Warn.LastOrDefault();
                User_UnWarn UnWarnInfo;
                var TimeoutMessage = new TimeSpan(0, 1, 0);
                var options = new List<ButtonOption<string>>();

                var AdminPermissionInfo = await _db.GetUser(Context.User.Id);
                if (ThisWarn.UnWarnId == null || ThisWarn.UnWarnId == 0)
                {
                    UnWarnInfo = new User_UnWarn 
                    { 
                        Warn_Id = ThisWarn.Id, 
                        AdminId = AdminPermissionInfo.User_Permission_Id, 
                        Status = User_UnWarn.WarnStatus.error, 
                        ReviewAdd = DateTime.Now, 
                        EndStatusSet = DateTime.Now 
                    };

                    options.Add(new("Везкая причина", ButtonStyle.Success));
                    options.Add(new("Ошибка выдачи", ButtonStyle.Danger));
                }
                else
                {
                    UnWarnInfo = _db.User_UnWarn.FirstOrDefault(x => x.Id == ThisWarn.UnWarnId);
                    UnWarnInfo.EndStatusSet = DateTime.Now;
                    UnWarnInfo.AdminId = AdminPermissionInfo.User_Permission_Id;

                    options.Add(new("Варн верный", ButtonStyle.Success));
                    options.Add(new("Варн неверный", ButtonStyle.Danger));
                }


                var pageBuilder = new PageBuilder()
                           .WithAuthor($"Нарушение {user}")
                           .WithDescription($"Нарушение выдал: <@{ThisWarn.Admin.User_Id}>\nПричина: {ThisWarn.Reason}\nВыдано: {ThisWarn.TimeSetWarn}")
                           .WithFooter($"Заявка активна {TimeoutMessage.TotalMinutes} минуту.")
                           .WithThumbnailUrl(user.GetAvatarUrl());

                var buttonSelection = new ButtonSelectionBuilder<string>()
                    .WithActionOnSuccess(ActionOnStop.DeleteInput)
                    .WithActionOnTimeout(ActionOnStop.DeleteInput)
                    .WithOptions(options)
                    .WithStringConverter(x => x.Option)
                    .WithSelectionPage(pageBuilder)
                    .AddUser(Context.User)
                    .Build();

                var result = await _interactive.SendSelectionAsync(buttonSelection, Context.Channel, TimeoutMessage);


                if (!result.IsTimeout)
                {
                    switch (result.Value.Option)
                    {
                        case "Везкая причина":
                            UnWarnInfo.Status = User_UnWarn.WarnStatus.restart;
                            break;
                        case "Ошибка выдачи":
                            UnWarnInfo.Status = User_UnWarn.WarnStatus.error;
                            break;
                        case "Варн верный":
                            UnWarnInfo.Status = User_UnWarn.WarnStatus.Rejected;
                            break;
                        case "Варн неверный":
                            UnWarnInfo.Status = User_UnWarn.WarnStatus.UnWarned;
                            break;
                    }

                    if (UnWarnInfo.Status != User_UnWarn.WarnStatus.Rejected)
                    {
                        var WarnInfo = _db.Guild_Warn.FirstOrDefault(x => x.Id == ThisWarn.Guild_WarnsId);
                        switch (WarnInfo.ReportTypes)
                        {
                            case Guild_Warn.ReportTypeEnum.TimeBan:
                                await Context.Guild.RemoveBanAsync(user);
                                break;
                            case Guild_Warn.ReportTypeEnum.Mute:
                            case Guild_Warn.ReportTypeEnum.TimeOut:
                                await user.RemoveTimeOutAsync();
                                break;
                        }
                        UnWarnInfo.EndStatusSet = DateTime.Now;
                    }

                    if (UnWarnInfo.Id == 0)
                        _db.User_UnWarn.Add(UnWarnInfo);
                    else
                        _db.User_UnWarn.Update(UnWarnInfo);

                    await _db.SaveChangesAsync();
                    emb.WithDescription($"Статус нарушения {ThisWarn.Id} изменен!");
                    await result.Message.ModifyAsync(x => x.Embed = emb.Build());
                    return;
                }

                await result.Message.DeleteAsync();
            }
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
            await ReplyAsync(embed: emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task unmute(SocketGuildUser user)
        {
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"unMute {user}");

            if(user.TimedOutUntil == null)
            {
                emb.WithDescription("У пользователя нету мута!");
            }
            else
            {
                emb.WithDescription("Вы успешно сняли мут с пользователя.");
                await user.RemoveTimeOutAsync();
            }

            await ReplyAsync(embed: emb.Build());
        }


        [Aliases, Commands, Usage, Descriptions]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        public async Task embedsay(SocketTextChannel TextChannel, [Remainder] string JsonText)
        {
            var mes = JsonToEmbed.JsonCheck(JsonText);
            if (mes.Item1 == null)
            {
                mes.Item2 = null;
                mes.Item1.Color = new Color(BotSettings.DiscordColor);
                mes.Item1.WithAuthor("Ошибка!");
                mes.Item1.Description = "Неправильная конвертация в Json.\nПрочтите инструкцию! - [Инструкция](https://docs.darlingbot.ru/commands/komandy-adminov/embedsay)";
            }
            if (string.IsNullOrWhiteSpace(mes.Item2))
                mes.Item2 = "";
            await TextChannel.SendMessageAsync(mes.Item2, false, mes.Item1.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        public async Task say(SocketTextChannel TextChannel, [Remainder] string Text)
        {
            await TextChannel.SendMessageAsync(Text);
        }
    }
}
