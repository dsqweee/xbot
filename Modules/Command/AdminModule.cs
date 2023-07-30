using XBOT.DataBase.Models.Roles_data;
using XBOT.Services;
using XBOT.Services.Attribute;
using XBOT.Services.Configuration;

namespace XBOT.Modules.Command
{
    [Summary("Админские\nкоманды"), Name("Admin")]
    [UserPermission(UserPermission.RolePermission.Admin)]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private ComponentEventService _componentEventService;

        public AdminModule(ComponentEventService componentEventService)
        {
            _componentEventService = componentEventService;
        }
        public enum RoleTypeEnum : byte
        {
            Level,
            Reputation,
            Buy,
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
            using (db _db = new())
            {
                string AuthorText = "";
                string DescriptionText = "";
                string YourRole = "";
                string Command = "";

                switch (Type)
                {
                    case RoleTypeEnum.Level:
                        AuthorText = "уровневую";
                        DescriptionText = "уровень";
                        YourRole = "уровневые";
                        Command = "lr";
                        break;
                    case RoleTypeEnum.Reputation:
                        AuthorText = "репутационную";
                        DescriptionText = "репутации";
                        YourRole = "репутационные";
                        Command = "rr";
                        break;
                    case RoleTypeEnum.Buy:
                        AuthorText = "магазинную";
                        DescriptionText = "coins";
                        YourRole = "магазинные";
                        Command = "br";
                        break;
                }

                var emb = new EmbedBuilder()
                    .WithColor(BotSettings.DiscordColor)
                    .WithAuthor($"🔨 Добавить {AuthorText} роль");

                async void SendMessage(string description, string text = "")
                {
                    await Context.Channel.SendMessageAsync(text, false, emb.Build());
                    return;
                }

                var lvlrole = _db.Roles_Level.FirstOrDefault(x => x.RoleId == role.Id);
                if (lvlrole != null)
                {
                    SendMessage($"Роль {role.Mention} уже выдается за {lvlrole.Level} {DescriptionText}");
                }

                var rolepos = role.Guild.CurrentUser.Roles.FirstOrDefault(x => x.Position > role.Position);
                if (rolepos == null)
                {
                    SendMessage($"Позиция роли {role.Mention} находится выше, роли бота.\nПопросите икс поднять эту роль выше моей", Context.Guild.Owner.Mention);
                }

                if (role.IsManaged)
                {
                    SendMessage("Данную роль нельзя выставить.");
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
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }
        private async Task roledel(SocketRole role, RoleTypeEnum Type)
        {
            using (db _db = new())
            {

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

                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task unwarn(SocketGuildUser user)
        {
            using (var _db = new db())
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor).WithAuthor($"unwarn {user.Mention}");
                var userwarned = await _db.GetUser(user.Id);
                if(userwarned.CountWarns == 0)
                {
                    emb.WithDescription($"У пользователя {user.Mention} отстствуют нарушения.");
                    await Context.Channel.SendMessageAsync("",false, emb.Build());
                    return;
                }

                var Warns = _db.Guild_Warn.ToList();

                if(Warns.Count == userwarned.CountWarns)
                {
                    foreach (var warn in userwarned.User_Warn)
                    {
                        warn.WarnSkippedAfterUnban = true;
                    }
                    await Context.Guild.RemoveBanAsync(user);
                    emb.WithDescription($"Пользователь разбанен. И все нарушения, анулированы.");
                    await Context.Channel.SendMessageAsync("", false, emb.Build());
                }
                else
                {
                    var builder = new ComponentBuilder();

                    Dictionary<User_UnWarn.WarnStatus, string> buttonIds = new();
                    var ButtonHashSetList = new HashSet<string>();
                    User_UnWarn UnWarnInfo;
                    var ThisWarn = userwarned.User_Warn.LastOrDefault();
                    if(ThisWarn.UnWarn_Id == null || ThisWarn.UnWarn_Id == 0)
                    {
                        UnWarnInfo = new User_UnWarn { Warn_Id = ThisWarn.Id, Admin_Id = Context.User.Id, Status = User_UnWarn.WarnStatus.error,ReviewAdd = DateTime.Now, EndStatusSet = DateTime.Now };

                        buttonIds = new Dictionary<User_UnWarn.WarnStatus, string> 
                        {
                            { User_UnWarn.WarnStatus.restart, Guid.NewGuid().ToString() },
                            { User_UnWarn.WarnStatus.error, Guid.NewGuid().ToString() }
                        };

                        string ButtonRestart = buttonIds[User_UnWarn.WarnStatus.restart];
                        string ButtonError = buttonIds[User_UnWarn.WarnStatus.error];
                        builder.WithButton("Везкая причина", $"{ButtonRestart}", ButtonStyle.Secondary);
                        builder.WithButton("Ошибка", $"{ButtonError}", ButtonStyle.Secondary);
                        ButtonHashSetList = new HashSet<string> { ButtonError, ButtonRestart };
                    }
                    else
                    {
                        var AdminPermissionInfo = await _db.GetUser(Context.User.Id);
                        
                        UnWarnInfo = _db.User_UnWarn.FirstOrDefault(x => x.Id == ThisWarn.UnWarn_Id);
                        UnWarnInfo.EndStatusSet = DateTime.Now;
                        UnWarnInfo.Admin_Id = AdminPermissionInfo.User_Permission_Id;

                        buttonIds = new Dictionary<User_UnWarn.WarnStatus, string>
                        {
                            { User_UnWarn.WarnStatus.UnWarned, Guid.NewGuid().ToString() },
                            { User_UnWarn.WarnStatus.Rejected, Guid.NewGuid().ToString() }
                        };

                        string ButtonRejected = buttonIds[User_UnWarn.WarnStatus.Rejected];
                        string ButtonUnWarned = buttonIds[User_UnWarn.WarnStatus.UnWarned];

                        builder.WithButton("Варн верный", $"{ButtonRejected}", ButtonStyle.Success);
                        builder.WithButton("Варн неверный", $"{ButtonUnWarned}", ButtonStyle.Danger);
                        ButtonHashSetList = new HashSet<string> { ButtonUnWarned, ButtonRejected };
                    }
                    var mes = await Context.Channel.SendMessageAsync("", false, emb.Build(), components: builder.Build());

                    var userInteraction = new ComponentEvent<SocketMessageComponent>(ButtonHashSetList);
                    _componentEventService.AddInteraction(ButtonHashSetList, userInteraction);

                    var selectedOption = await userInteraction.WaitForInteraction();

                    if (selectedOption != null)
                    {
                        foreach (var key in buttonIds.Keys)
                        {
                            var Value = buttonIds[key];
                            if (selectedOption.Data.CustomId == Value)
                            {
                                UnWarnInfo.Status = key;
                                break;
                            }
                        }
                        if(UnWarnInfo.Status != User_UnWarn.WarnStatus.Rejected)
                        {
                            var WarnInfo = _db.Guild_Warn.FirstOrDefault(x=>x.Id == ThisWarn.Guild_Warns_Id);
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
                            _db.Add(UnWarnInfo);

                        await _db.SaveChangesAsync();
                        emb.WithDescription($"Статус нарушения {ThisWarn.Id} изменен!");
                    }

                    foreach (var item in buttonIds.Values)
                    {
                        _componentEventService.RemoveInteraction(item);
                    }

                    await mes.ModifyAsync(x => { x.Components = new ComponentBuilder().Build(); x.Embed = emb.Build(); });
                }
            }
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
