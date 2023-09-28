using static XBOT.DataBase.Guild_Logs;
using XBOT.Services.Attribute;
using XBOT.Services.Configuration;
using XBOT.Services;
using Microsoft.EntityFrameworkCore;
using static XBOT.DataBase.Guild_Warn;
using System.Diagnostics;

namespace XBOT.Modules.Command
{
    [RequireOwner]
    [Name("Settings"), Summary("Настройки бота")]
    public class SettingsModule : ModuleBase<SocketCommandContext>
    {
        //private readonly Db _db;

        //public SettingsModule(Db db)
        //{
        //    _db = db;
        //}

        [Aliases, Commands, Usage, Descriptions]
        public async Task PrivateCreate()
        {
            using var _db = new Db();
            var embed = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor("🔨 Создание канала для приваток");

            var Settings = _db.Settings.FirstOrDefault();

            var PrivateVoiceChannel = Context.Guild.GetVoiceChannel(Settings.PrivateVoiceChannelId);
            List<Overwrite> voicePermissions = new List<Overwrite> { new Overwrite(Context.Guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(speak: PermValue.Deny, deafenMembers: PermValue.Deny)) };
            if (PrivateVoiceChannel == null)
            {
                var Category = await Context.Guild.CreateCategoryChannelAsync(BotSettings.PrivateCategoryName, X => { X.Position = int.MaxValue; });
                var PrivateVoice = await Context.Guild.CreateVoiceChannelAsync(BotSettings.PrivateVoiceName, x => { x.CategoryId = Category.Id; x.PermissionOverwrites = voicePermissions; });
                Settings.PrivateVoiceChannelId = PrivateVoice.Id;
                await _db.SaveChangesAsync();
                embed.WithDescription("Приватка успешно создана!");
            }
            else
                embed.WithDescription("Приватка уже существует.")
                     .WithColor(BotSettings.DiscordColorError);


            await Context.Channel.SendMessageAsync("", false, embed.Build());

        }

        private static string SelectChannelType(string[] channelTypes, uint num, bool change)
        {
            Enum.TryParse(channelTypes[num], out ChannelsTypeEnum myStatus);
            switch (myStatus)
            {
                case ChannelsTypeEnum.Ban:
                    return $"Бан{(change ? "е" : "")} пользователя";
                case ChannelsTypeEnum.UnBan:
                    return $"Разбан{(change ? "е" : "")} пользователя";
                case ChannelsTypeEnum.Kick:
                    return $"Кик{(change ? "е" : "")} пользователя";
                case ChannelsTypeEnum.Left:
                    return $"Выход{(change ? "е" : "")} пользователя";
                case ChannelsTypeEnum.Join:
                    return $"Вход{(change ? "е" : "")} пользователя";
                case ChannelsTypeEnum.MessageEdit:
                    return string.Format("Измененны{0} сообщения{1}", change ? "х" : "е", change ? "х" : "");
                case ChannelsTypeEnum.MessageDelete:
                    return string.Format("Удаленны{0} сообщения{1}", change ? "х" : "е", change ? "х" : "");
                case ChannelsTypeEnum.VoiceAction:
                    return $"Активност{(change ? "и" : "ь")} в голосовых чатах";
                case ChannelsTypeEnum.BirthDay:
                    return $"Поздравлени{(change ? "и" : "е")} пользователей с их днем рождения";
                default:
                    return string.Empty;
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task logsettings(uint selection = 0, SocketTextChannel channel = null)
        {
            using var _db = new Db();
            var Prefix = _db.Settings.FirstOrDefault().Prefix;
            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor).WithAuthor(" - Логирование на сервере", Context.Guild.IconUrl);
            var ChannelTypes = Enum.GetNames(typeof(ChannelsTypeEnum));
            if (selection == 0 && channel == null)
            {
                var ChannelLogs = _db.Guild_Logs.ToList();
                for (uint i = 1; i < ChannelTypes.Length; i++)
                {
                    SocketTextChannel Channel = null;
                    string text = SelectChannelType(ChannelTypes, i, false);
                    var This = ChannelLogs.FirstOrDefault(x => x.Type.ToString() == ChannelTypes[i]);
                    if (This != null)
                    {
                        Channel = Context.Guild.GetTextChannel(This.TextChannelId);
                        if (Channel == null)
                        {
                            _db.Guild_Logs.Remove(This);
                            await _db.SaveChangesAsync();
                        }
                    }
                    emb.AddField($"{i}.{text}", $"{Channel?.Mention ?? "Канал не указан"}", true);
                }
                emb.WithFooter($"Включить - {Prefix}LogSettings [цифра] [канал]\nОтключить - {Prefix}LogSettings [цифра]");
            }
            else
            {
                if (!(selection >= 1 && selection <= ChannelTypes.Length))
                    emb.WithDescription($"Выбор может быть только от 1 до {ChannelTypes.Length}").WithFooter($"Подробнее - {Prefix}LogSettings");
                else
                {
                    string text = SelectChannelType(ChannelTypes, selection, true);
                    bool success = false;
                    _ = Enum.TryParse(ChannelTypes[selection], out ChannelsTypeEnum myStatus);
                    var ChannelLogs = _db.Guild_Logs.FirstOrDefault(x => x.Type == myStatus);

                    if (channel == null)
                    {
                        if (ChannelLogs == null)
                            emb.WithDescription($"Отправка сообщений о {text} и так отключена");
                        else
                        {
                            success = true;
                            _db.Guild_Logs.Remove(ChannelLogs);
                            emb.WithDescription($"Отправка сообщений о {text} была отключена");
                        }
                    }
                    else
                    {
                        if (ChannelLogs == null || ChannelLogs.Id != channel.Id)
                        {
                            if (ChannelLogs != null)
                            {
                                ChannelLogs.TextChannelId = channel.Id;
                                _db.Guild_Logs.Update(ChannelLogs);
                            }
                            else
                            {
                                await _db.GetTextChannel(channel.Id);
                                _db.Guild_Logs.Add(new Guild_Logs() { TextChannelId = channel.Id, Type = myStatus });
                            }

                            success = true;
                            emb.WithDescription($"Отправка сообщений о {text} включена в канал {channel.Mention}");
                        }
                        else
                            emb.WithDescription($"Отправка сообщений о {text} уже включена в канал {channel.Mention}");
                    }
                    if (success)
                        await _db.SaveChangesAsync();

                }
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task messagesettings(byte selection = 0, [Remainder] string text = null)
        {
            using var _db = new Db();
            var ThisGuild = _db.Settings
                .Include(x => x.WelcomeTextChannel)
                .Include(x => x.LeaveTextChannel)
                .FirstOrDefault();


            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor("🛎 Настройка оповещения");

            byte point = 1;
            if (selection == 0 && string.IsNullOrWhiteSpace(text))
            {
                var WelcomeChannel = Context.Guild.GetTextChannel(Convert.ToUInt64(ThisGuild.WelcomeTextChannelId));
                emb.AddField($"{point}.Канал для Сообщений при входе [channel]", WelcomeChannel?.Mention ?? "Отсутствует", true); // 1 
                point++;
                if (WelcomeChannel != null)
                {
                    emb.AddField($"{point}.Сообщение при входе [json]", ThisGuild.WelcomeMessage != null ? $"Установлено" : "Отсутствует", true); // 2
                    point++;
                    emb.AddField($"{point}.Личное сообщение при входе [json]", ThisGuild.WelcomeDMmessage != null ? $"Установлено" : "Отсутствует", true); // 3
                    point++;

                    var WelcomeRole = Context.Guild.GetRole(Convert.ToUInt64(ThisGuild.WelcomeRoleId));
                    emb.AddField($"{point}.Роль при входе [role]", WelcomeRole?.Mention ?? "Отсутствует", true); // 4
                    point++;
                }

                var LeaveChannel = Context.Guild.GetTextChannel(Convert.ToUInt64(ThisGuild.LeaveTextChannelId));
                emb.AddField($"{point}.Канал для Сообщений при выходе [channel]", LeaveChannel?.Mention ?? "Отсутствует", true); // 5 | 2 
                point++;

                if (LeaveChannel != null)
                {
                    emb.AddField($"{point}.Сообщение при выходе [json]", ThisGuild.LeaveMessage != null ? $"Установлено" : "Отсутствует", true); //6 | 3
                    point++;
                }

                emb.WithFooter($"Вкл - {ThisGuild.Prefix}ms [цифра] [channel/json(embed.discord-bot.net)/role]\nВыкл - {ThisGuild.Prefix}ms [цифра]\n%user% - чтобы упомянуть пользователя");
            }
            else
            {
                point = 2;
                if (ThisGuild.WelcomeTextChannel != null)
                    point += 3;

                if (ThisGuild.LeaveTextChannel != null)
                    point++;

                if (!(selection >= 1 && selection <= point))
                    emb.WithDescription($"Выбор может быть только от 1 до {point}").WithFooter($"Подробнее - {ThisGuild.Prefix}ms");
                else
                {
                    if (selection == 1 || selection == 5 && ThisGuild.WelcomeTextChannel != null || selection == 2 && ThisGuild.WelcomeTextChannel == null)
                    {
                        var Channel = Context.Message.MentionedChannels.FirstOrDefault();
                        if (Channel != null)
                        {
                            emb.WithDescription($"В канал <#{Channel.Id}> буду приходить сообщения о {(selection == 5 || selection == 2 ? "выходе" : "входе")} пользователей.")
                               .WithFooter("Напишите команду еще раз, чтобы работать с открывшимися инструментами.");

                            var CreatedChannel = await _db.GetTextChannel(Channel.Id);

                            if (selection == 1)
                                ThisGuild.WelcomeTextChannelId = CreatedChannel.Id;
                            else
                                ThisGuild.LeaveTextChannelId = CreatedChannel.Id;
                        }
                        else
                        {
                            if (ThisGuild.WelcomeTextChannel == null)
                                emb.WithDescription($"Введенный канал не найден.");
                            else
                            {
                                emb.WithDescription($"В канал <#{ThisGuild.WelcomeTextChannelId}> не будут приходить сообщения о {(selection == 5 || selection == 2 ? "выходе" : "входе")} пользователей.");

                                if (selection == 1 || selection == 5)
                                    ThisGuild.WelcomeTextChannelId = null;
                                else
                                    ThisGuild.LeaveTextChannelId = null;
                            }
                        }

                    }
                    else if ((selection == 2 || selection == 3) && ThisGuild.WelcomeTextChannel != null ||
                        selection == 3 && ThisGuild.LeaveTextChannel == null || selection == 6 && ThisGuild.LeaveTextChannel != null)
                    {
                        if (text != null)
                        {
                            var embed = JsonToEmbed.JsonCheck(text);
                            if (embed.Item1 == null)
                                emb.WithDescription($"Сообщение составлено не верно.").WithFooter("Создать сообщение - embed.discord-bot.net");
                            else
                            {
                                emb.WithDescription($"EmbedVisualizer cообщение будет отправляться ");
                                if (selection == 3)
                                {
                                    emb.Description += "пользователю при входе на сервер.";
                                    emb.WithFooter("Если он не отключил возможность отправлять ему сообщения");
                                    ThisGuild.WelcomeDMmessage = text;
                                }
                                else if (selection == 2)
                                {
                                    emb.Description += $"в канал <#{ThisGuild.WelcomeTextChannelId}> при входе на сервер.";
                                    ThisGuild.WelcomeMessage = text;
                                }
                                else
                                {
                                    emb.Description += $"в канал <#{ThisGuild.LeaveTextChannelId}> при выходе пользователя с сервера.";
                                    ThisGuild.LeaveMessage = text;
                                }
                            }
                        }
                        else
                        {
                            if (selection == 3)
                            {
                                if (ThisGuild.WelcomeDMmessage == null)
                                {
                                    emb.WithDescription($"Введите текст для включения Личного сообщения при входе").WithFooter($"Подробнее - {ThisGuild.Prefix}ms");
                                }
                                else
                                {
                                    emb.WithDescription($"Личное сообщение при входе выключено");
                                    ThisGuild.WelcomeDMmessage = null;
                                }
                            }
                            else if (selection == 2)
                            {
                                if (ThisGuild.WelcomeMessage == null)
                                    emb.WithDescription($"Введите текст для включения Сообщения при входе").WithFooter($"Подробнее - {ThisGuild.Prefix}ms");
                                else
                                {
                                    emb.WithDescription($"Сообщение при входе выключено");
                                    ThisGuild.WelcomeMessage = null;
                                }
                            }
                            else
                            {
                                if (ThisGuild.LeaveMessage == null)
                                    emb.WithDescription($"Введите текст для включения Сообщения при выходе").WithFooter($"Подробнее - {ThisGuild.Prefix}ms");
                                else
                                {
                                    emb.WithDescription($"Сообщение при выходе выключено");
                                    ThisGuild.LeaveMessage = null;
                                }
                            }

                        }
                    }
                    else if (selection == 4)
                    {
                        if (text != null)
                        {
                            var Role = Context.Message.MentionedRoles.FirstOrDefault();
                            if (Role != null)
                            {
                                emb.WithDescription($"Новым пользователям будет выдаваться роль {Role.Mention}.");
                                await _db.GetRole(Role.Id);
                                ThisGuild.WelcomeRoleId = Role.Id;
                            }
                            else
                                emb.WithDescription($"Введенная роль не найдена. Укажите роль в таком формате: {Context.Guild.EveryoneRole.Mention}").WithFooter("Не бойтесь, ни один ваш любимчик не был потревожен.");
                        }
                        else
                        {
                            if (ThisGuild.WelcomeRoleId == 0)
                                emb.WithDescription($"Введите роль для выдачи ее пользователям").WithFooter($"Подробнее - {ThisGuild.Prefix}ms");
                            else
                            {
                                emb.WithDescription($"Роль <@{ThisGuild.WelcomeRoleId}> не будет выдаваться пользователям");
                                ThisGuild.WelcomeRoleId = 0;
                            }
                        }
                    }


                    _db.Settings.Update(ThisGuild);
                    await _db.SaveChangesAsync();
                }
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task addwarn(byte CountWarn, ReportTypeEnum report, string Time = null)
        {
            using var _db = new Db();
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor("⚜️ WarnSystem - Добавить варн");

            const string error = "Варн должен содержать время, возможно вы ввели большое число?\nПример: 01:00:00 [ч:м:с]\nПример 2: 07:00:00:00 [д:ч:м:с]";
            bool Success = TimeSpan.TryParse(Time, out TimeSpan result);
            if ((report == ReportTypeEnum.TimeBan || report == ReportTypeEnum.TimeOut) && !Success)
                emb.WithDescription(error);
            else if (result.TotalDays > 7)
                emb.WithDescription("Время нарушения не может превышать 7 дней!");
            else
            {
                if (CountWarn >= 1 && CountWarn <= 15)
                {
                    if (_db.Guild_Warn.Any(x => x.ReportTypes == ReportTypeEnum.Ban && x.CountWarn < CountWarn))
                        emb.WithDescription("Вы не можете добавлять другие нарушения выше нарушения с баном.");
                    else
                    {
                        var ThisWarn = _db.Guild_Warn.FirstOrDefault(x => x.CountWarn == CountWarn);
                        if (ThisWarn != null)
                        {
                            emb.WithDescription($"Варн {CountWarn} был перезаписан с `{ThisWarn.ReportTypes}` на `{report}`.");
                            ThisWarn.ReportTypes = report;
                            ThisWarn.Time = result;
                            _db.Guild_Warn.Update(ThisWarn);
                        }
                        else
                        {
                            emb.WithDescription($"Варн {CountWarn} был успешно добавлен.");
                            var newwarn = new Guild_Warn() { CountWarn = CountWarn, ReportTypes = report, Time = result };
                            _db.Guild_Warn.Add(newwarn);
                        }
                        var Prefix = _db.Settings.FirstOrDefault().Prefix;
                        emb.WithFooter($"Посмотреть все варны {Prefix}ws");
                        await _db.SaveChangesAsync();
                    }
                }
                else emb.WithDescription($"Количество варнов может быть больше 1 и меньше 15");
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task delwarn(byte CountWarn)
        {
            using var _db = new Db();
            var Prefix = _db.Settings.FirstOrDefault().Prefix;
            var warn = _db.Guild_Warn.FirstOrDefault(x => x.CountWarn == CountWarn);
            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor).WithAuthor("⚜️ WarnSystem - Удалить варн");
            if (warn != null)
            {
                _db.Guild_Warn.Remove(warn);
                await _db.SaveChangesAsync();
                emb.WithDescription($"Варн с номером {CountWarn} успешно удален.");
            }
            else emb.WithDescription($"Варн с номером {CountWarn} отсутствует.");

            emb.WithFooter($"Посмотреть все варны {Prefix}ws");
            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task AdminRoleSet(SocketRole role) => await PrivilegeRoleSet(UserPermission.RolePermission.Admin, role);

        [Aliases, Commands, Usage, Descriptions]
        public async Task ModeratorRoleSet(SocketRole role) => await PrivilegeRoleSet(UserPermission.RolePermission.Moder, role);

        [Aliases, Commands, Usage, Descriptions]
        public async Task IventerRoleSet(SocketRole role) => await PrivilegeRoleSet(UserPermission.RolePermission.Iventer, role);

        private async Task PrivilegeRoleSet(UserPermission.RolePermission permission, SocketRole role)
        {
            using var _db = new Db();
            string text = "";
            var newrole = await _db.GetRole(role.Id);
            ulong oldroleId = 0;
            if (permission == UserPermission.RolePermission.Admin)
            {
                text = "админа";
                var Settings = _db.Settings.Include(x => x.AdminRole).FirstOrDefault();
                oldroleId = Convert.ToUInt64(Settings.AdminRoleId);
                Settings.AdminRoleId = newrole.Id;
            }
            else if (permission == UserPermission.RolePermission.Moder)
            {
                var Settings = _db.Settings.Include(x => x.ModeratorRole).FirstOrDefault();
                oldroleId = Convert.ToUInt64(Settings.ModeratorRoleId);
                Settings.ModeratorRoleId = newrole.Id;
                text = "модератора";
            }
            else
            {
                var Settings = _db.Settings.Include(x => x.IventerRole).FirstOrDefault();
                oldroleId = Convert.ToUInt64(Settings.IventerRoleId);
                Settings.IventerRoleId = newrole.Id;
                text = "ивентера";
            }

            await _db.SaveChangesAsync();

            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor).WithAuthor($"Роль {text}");

            if (oldroleId == 0)
                emb.WithDescription($"Вы успешно выставили роль {text} {role.Mention}");
            else
            {
                emb.WithDescription($"Вы успешно заменили роль {text} с <@&{oldroleId}> на {role.Mention}");
                foreach (var user in Context.Guild.Users.Where(x => x.Roles.Any(x => x.Id == oldroleId)))
                {
                    await user.RemoveRoleAsync(oldroleId);
                    await user.AddRoleAsync(role.Id);
                }
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task botblock(ulong userId, string reason)
        {
            using var _db = new Db();
            var user = await _db.GetUser(userId);
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor);

            if (!string.IsNullOrWhiteSpace(user.BlockReason))
            {
                emb.WithAuthor("Снятие блокировки")
                   .WithDescription($"Вы успешно сняли блокировку бота для <@{userId}>");
                user.BlockReason = "";
            }
            else
            {
                emb.WithAuthor("Выдача блокировки")
                   .WithDescription($"Вы успешно заблокировали бота для <@{userId}>");
                user.BlockReason = reason;
            }
            _db.User.Update(user);
            await _db.SaveChangesAsync();
            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }

        //[Aliases, Commands, Usage, Descriptions]
        //public async Task test()
        //{
        //    var user = await _db.GetUser(Context.User.Id);
        //    user.money += 1;
        //    _db.User.Update(user);
        //    await _db.SaveChangesAsync();
        //}

        //[Aliases, Commands, Usage, Descriptions]
        //public async Task test2()
        //{
        //    var user = await _db.GetUser(Context.User.Id);
        //    user.money += 1;
        //    await _db.SaveChangesAsync();
        //}

        [Aliases, Commands, Usage, Descriptions]
        public async Task prefix(string prefix = null)
        {
            using var _db = new Db();
            var Settings = _db.Settings.FirstOrDefault();
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor("Ваш префикс")
                .WithDescription($"Префикс бота: {Settings.Prefix}");

            if (prefix != null)
            {
                Settings.Prefix = prefix;
                _db.Settings.Update(Settings);
                await _db.SaveChangesAsync();
                emb.WithDescription($"Префикс бота изменен с {Settings.Prefix} на {prefix}");
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task RoleToMessage(ulong messageId, SocketTextChannel channel, SocketRole role, bool add)
        {
            using var _db = new Db();
            var Settings = _db.Settings.FirstOrDefault();
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor);

            var message = await channel.GetMessageAsync(messageId);
            if (message != null)
            {
                SelectMenuBuilder menu = null;
                foreach (var component in message.Components)
                {
                    var thiscomp = component as ActionRowComponent;
                    foreach (var item in thiscomp.Components)
                    {
                        menu = (item as SelectMenuComponent).ToBuilder();
                    }
                }

                //var Menu = (message.Components.FirstOrDefault(x=>x.Type == ComponentType.ActionRow) as ActionRowComponent).Components.FirstOrDefault() as SelectMenuBuilder;
                if (menu == null)
                {
                    menu = new SelectMenuBuilder()
                    .WithPlaceholder("Выберите роль")
                    .WithCustomId($"infinity_{(add ? "add" : "rem")}role_{Guid.NewGuid()}")
                    .WithMinValues(1)
                    .WithMaxValues(1);

                    menu.AddOption($"{role.Name}", $"{role.Id}");
                    emb.WithDescription($"Вы успешно выставили роль {role.Mention} в список!");
                }
                else
                {
                    var Option = menu.Options.FirstOrDefault(x => x.Value == $"{role.Id}");
                    if (Option != null)
                    {
                        menu.Options.Remove(Option);
                        emb.WithDescription($"Вы успешно удалили роль {role.Mention} из списока!");
                    }
                    else
                    {
                        emb.WithDescription($"Вы успешно выставили роль {role.Mention} в список!");
                        menu.AddOption($"{role.Name}", $"{role.Id}");
                    }

                }
                var builder = new ComponentBuilder()
                    .WithSelectMenu(menu);


                await channel.ModifyMessageAsync(messageId, x => x.Components = builder.Build());
            }
            else
                emb.WithDescription("Сообщение с таким Id не найдено!");

            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task channelsettings(SocketTextChannel channel = null, float number = 0)
        {
            using var _db = new Db();
            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor);
            var Prefix = _db.Settings.FirstOrDefault().Prefix;
            var chnl = new TextChannel();
            emb.WithAuthor($"🔨 Настройка каналов {(channel == null ? " " : $"- {channel.Name}")}");
            if (channel != null)
                chnl = await _db.GetTextChannel(channel.Id);

            if (channel != null && number == 0)
            {
                if (chnl != null)
                {
                    emb.AddField("1 Получение опыта", chnl.giveXp ? "Вкл" : "Выкл", true);
                    emb.AddField("2 Удалять ссылки", chnl.delUrl ? "Вкл" : "Выкл", true);
                    if (chnl.delUrl) emb.AddField("2.1 Удалять ссылки-изображения?", chnl.delUrlImage ? "Вкл" : "Выкл", true);
                    emb.AddField("3 Использование команд", chnl.useCommand ? "Вкл" : "Выкл", true);
                    if (!chnl.useCommand) emb.AddField("3.1 Использование RP команд?", chnl.useRPcommand ? "Вкл" : "Выкл", true);
                    if (!chnl.useCommand) emb.AddField("3.2 Использование Admin команд?", chnl.useAdminCommand ? "Вкл" : "Выкл", true);
                    emb.AddField("4 Удалять приглашения(кроме тех что сюда)", chnl.inviteLink ? "Вкл" : "Выкл", true);
                    emb.AddField("Номер Чата", chnl.Id, true);
                    emb.WithFooter($"Вкл/Выкл опции канала - {Prefix}cs [channel] [number]");
                }
                else
                    emb.WithDescription("Данный канал не найден!");
            }
            else if (channel != null && number != 0)
            {
                if (number >= 1 && number <= 4)
                {
                    switch (number)
                    {
                        case 1:
                            chnl.giveXp = !chnl.giveXp;
                            emb.WithDescription($"Получение уровней в {channel.Mention} {(chnl.giveXp ? "включено" : "выключено")}");
                            break;
                        case 2:
                            chnl.delUrl = !chnl.delUrl;
                            emb.WithDescription($"Ссылки в {channel.Mention} {(chnl.delUrl ? "удаляются" : "не удаляются")}");
                            if (chnl.delUrl)
                            {
                                emb.WithFooter("Вы можете настроить удаление ссылок-картинок! Откройте команду еще раз!");
                                chnl.delUrlImage = true;
                            }
                            else
                                chnl.delUrlImage = false;
                            break;
                        case 2.1f:
                            chnl.delUrlImage = !chnl.delUrlImage;
                            emb.WithDescription($"Ссылки-картинки в {channel.Mention} {(chnl.delUrlImage ? "удаляются" : "не удаляются")}");
                            break;
                        case 3:
                            chnl.useCommand = !chnl.useCommand;
                            emb.WithDescription($"Команды в {channel.Mention} теперь {(chnl.useCommand ? "включены" : "выключены")}");
                            if (!chnl.useCommand)
                            {
                                emb.WithFooter("Вы можете настроить RP и Admin команды! Откройте команду еще раз!");
                                chnl.useRPcommand = false;
                                chnl.useAdminCommand = false;
                            }
                            else
                            {
                                chnl.useAdminCommand = true;
                                chnl.useRPcommand = true;
                            }
                            break;
                        case 3.1f:
                            chnl.useRPcommand = !chnl.useRPcommand;
                            emb.WithDescription($"RP-команды в {channel.Mention} теперь {(chnl.useRPcommand ? "включены" : "выключены")}");
                            break;
                        case 3.2f:
                            chnl.useAdminCommand = !chnl.useAdminCommand;
                            emb.WithDescription($"Admin-команды в {channel.Mention} теперь {(chnl.useAdminCommand ? "включены" : "выключены")}");

                            break;
                        case 4:
                            chnl.inviteLink = !chnl.inviteLink;
                            emb.WithDescription($"Приглашения на другие сервера в {channel.Mention} теперь {(chnl.inviteLink == true ? "удаляются" : "не удаляются")}");
                            break;
                        default:
                            emb.WithDescription($"Команда с таким номером не найдена!");
                            break;
                    }
                    _db.TextChannel.Update(chnl);
                    await _db.SaveChangesAsync();
                }
                else emb.WithDescription("Номер может быть от 1 до 4.").WithFooter($"Подробнее - {Prefix}cs [channel]");
            }
            else emb.WithDescription($"Введите нужный вам канал, пример - {Prefix}cs [channel]");

            await Context.Channel.SendMessageAsync("", false, emb.Build());

        }
    }
}
