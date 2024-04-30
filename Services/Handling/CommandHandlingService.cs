using System.Reflection;
using XBOT.Services.Configuration;
using XBOT.Services.PrivateStructure;
using XBOT.Services.Attribute.ErrorList;
using static XBOT.DataBase.Guild_Logs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace XBOT.Services.Handling;

public class CommandHandlingService : IHostedService
{
    private readonly DiscordSocketClient _discord;
    private readonly TaskTimer _timer;
    private readonly GiftQuestion_Service _gift;
    private readonly Invite_Service _invite;
    private readonly Guild_Logs_Service _guildlogs;
    private readonly UserMessagesSolution _messagesolution;
    private readonly Meeting_Logs_Service _meeting;
    private readonly GiveAway_Service _giveaway;
    private readonly Refferal_Service _refferal;

    private readonly PrivateSystem _privatesystem;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;
    private readonly ILogger<CommandService> _commandslogger;
    private readonly Db _db;

    public CommandHandlingService(
        DiscordSocketClient discord,
        CommandService commands,
        IServiceProvider services,
        ILogger<CommandService> commandslogger,
        TaskTimer timer,
        GiftQuestion_Service gift,
        Invite_Service invite,
        Guild_Logs_Service guildlogs,
        Db db,
        PrivateSystem privatesystem,
        UserMessagesSolution messagesolution,
        Meeting_Logs_Service meeting,
        GiveAway_Service giveaway,
        Refferal_Service refferal)
    {
        _discord = discord;
        _commands = commands;
        _services = services;
        _commandslogger = commandslogger;

        _commands.Log += msg => LoggingService.OnLogAsync(_commandslogger, msg);
        _timer = timer;
        _gift = gift;
        _invite = invite;
        _guildlogs = guildlogs;
        _db = db;
        _privatesystem = privatesystem;
        _messagesolution = messagesolution;
        _meeting = meeting;
        _giveaway = giveaway;
        _refferal = refferal;
    }



    private async Task SelectMenuExecuted(SocketMessageComponent interaction)
    {
        string CustomId_times = interaction.Data.CustomId.Split("_")[0];

        if ((interaction.Message.CreatedAt.LocalDateTime - DateTime.Now).TotalHours > 24 && CustomId_times != "infinity")
            await interaction.Message.ModifyAsync(x => x.Components = new ComponentBuilder().Build());

        if (CustomId_times != "infinity" && CustomId_times != "timely")
            return;


        if (interaction.Data.Type == ComponentType.SelectMenu)
        {
            
            var result = ulong.TryParse(interaction.Data.Values.First(), out ulong ulongvalue);
            if (result)
            {
                string MenuType = interaction.Data.CustomId.Split("_")[1];
                var user = interaction.User as SocketGuildUser;
                if(MenuType == "addrole")
                {
                    if (user.Roles.Any(x => x.Id == ulongvalue))
                        await user.RemoveRoleAsync(ulongvalue);
                    else
                        await user.AddRoleAsync(ulongvalue);
                }
                else
                {
                    await user.RemoveRoleAsync(ulongvalue);
                }

                await interaction.DeferAsync();
            }
        }
        else if (interaction.Data.Type == ComponentType.Button)
        {
            //var userInteraction = _componentEventService.GetInteraction<SocketMessageComponent>(interaction.Data.CustomId);
            //var TotalTime = (DateTime.Now - interaction.CreatedAt.LocalDateTime);

            //if (TotalTime.TotalSeconds > 180 || userInteraction == null)
            //    await interaction.Message.ModifyAsync(x => x.Components = new ComponentBuilder().Build());
            //else
            //{
            //    userInteraction.Component = interaction;
            //    userInteraction.CompleteInteraction();
            //    await interaction.DeferAsync();
            //}
        }
    }

    //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //{

    //}

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _discord.UserJoined += UserJoined;
        _discord.UserLeft += UserLeft;

        _discord.Ready += Ready;

        _discord.SelectMenuExecuted += SelectMenuExecuted;
        _discord.ButtonExecuted += SelectMenuExecuted;
        _discord.UserVoiceStateUpdated += UserVoiceStateUpdated;

        _commands.CommandExecuted += CommandExecuted;
        _discord.MessageReceived += MessageReceivedAsync;

        //_discord.InviteDeleted += Invite_Service.InviteDelete;
        _discord.InviteCreated += _invite.InviteCreate;

        _discord.UserBanned += _guildlogs.InUserBanned;
        _discord.UserUnbanned += _guildlogs.InUserUnBanned;

        _discord.MessageUpdated += _guildlogs.EditedMessage;
        _discord.MessagesBulkDeleted += BulkDeleteMessages;
        _discord.MessageDeleted += _guildlogs.DeleteMessage;


        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    private async Task BulkDeleteMessages(IReadOnlyCollection<Cacheable<IMessage, ulong>> messages, Cacheable<IMessageChannel, ulong> channel)
    {
        //using var _db = new Db();

        if (await channel.GetOrDownloadAsync() is not SocketTextChannel textChannel)
            return;

        var guildLog = _db.Guild_Logs.FirstOrDefault(x=>x.Type == ChannelsTypeEnum.MessageDelete);
        if (guildLog is null)
            return;

        var messageChannel = textChannel.Guild.GetTextChannel(guildLog.TextChannelId);
        if (messageChannel is null)
            return;

        var emb = new EmbedBuilder()
            .WithColor(BotSettings.DiscordColor)
            .WithAuthor("Массовое удаление сообщений")
            .WithTimestamp(DateTime.UtcNow);

        IUser? currentUser = null;

        var reversedMessages = messages.Reverse().ToList();

        foreach (var message in reversedMessages)
        {
            var getMessage = await message.GetOrDownloadAsync();
            if (getMessage is null || getMessage.Author.IsBot || getMessage.Content.Length > 1023)
                continue;

            if (currentUser != getMessage.Author || currentUser is null)
            {
                currentUser = getMessage.Author;
                emb.Description += $"Отправитель [{getMessage.Author.Mention}]\n";
            }

            string text = "⠀⠀Сообщение [`{0}`:{1}]";

            var messageTime = getMessage.Timestamp.ToString("HH:mm dd.MM.yy");
            emb.Description += string.IsNullOrWhiteSpace(getMessage.Content)
                ? string.Format(text, getMessage.Attachments.FirstOrDefault()?.Url, messageTime)
                : string.Format(text, getMessage.Content, messageTime);

            emb.Description += "\n";

            if (emb.Description.Length > 800)
            {
                currentUser = null;
                await messageChannel.SendMessageAsync("", embed: emb.Build());
                emb.WithDescription("");
            }
        }

        await messageChannel.SendMessageAsync("", embed: emb.Build());
    }

    private async Task CommandExecuted(Optional<CommandInfo> CommandInfo, ICommandContext Context, Discord.Commands.IResult Result)
    {
        //using var _db = new Db();
        if (!string.IsNullOrWhiteSpace(Result?.ErrorReason))
        {
            string Prefix = _db.Settings.FirstOrDefault().Prefix;
            var emb = ErrorMessage.GetError(Result.ErrorReason, Prefix, CommandInfo.IsSpecified ? CommandInfo.Value : null);
            emb.WithFooter($"Build: {BotSettings.GetBuild()}");
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }
    }

    private async Task ReactAdd(Cacheable<IUserMessage, ulong> mess, Cacheable<IMessageChannel, ulong> chnl, SocketReaction emj)
        => await GetOrRemoveRole(mess, emj, false);
    private async Task ReactRem(Cacheable<IUserMessage, ulong> mess, Cacheable<IMessageChannel, ulong> chnl, SocketReaction emj)
        => await GetOrRemoveRole(mess, emj, true);
    private static async Task GetOrRemoveRole(Cacheable<IUserMessage, ulong> mess, SocketReaction emj, bool getOrRemove)
    {
        if (emj.User.Value.IsBot)
            return;

        //var message = await mess.GetOrDownloadAsync();
        //if (message != null)
        //{
        //    string EmoteName = emj.Emote.ToString();
        //    var mes = db.EmoteClick.FirstOrDefault(x => x.MessageId == message.Id && EmoteName == x.Emote);
        //    if (mes != null)
        //    {
        //        var usr = emj.User.Value as SocketGuildUser;
        //        var role = usr.Guild.GetRole(mes.RoleId);
        //        if (role != null)
        //        {
        //            var rolepos = usr.Guild.CurrentUser.Roles.FirstOrDefault(x => x.Position > role.Position);
        //            if (rolepos != null)
        //            {
        //                if (getOrRemove && !mes.Get || !getOrRemove && mes.Get)
        //                {
        //                    if (usr.Roles.FirstOrDefault(x => x.Id == role.Id) != null)
        //                        await usr.RemoveRoleAsync(role.Id);
        //                }
        //                else if (!getOrRemove && !mes.Get)
        //                {
        //                    if (usr.Roles.FirstOrDefault(x => x.Id == role.Id) == null)
        //                        await usr.RemoveRoleAsync(role.Id);
        //                }
        //            }
        //        }
        //    }
        //}
    } // Проверка эмодзи в событии ReactRem и ReactAdd



    private async Task Ready()
    {
        Console.WriteLine($"Connected and Start Scanning!");

        await _giveaway.GiveAwayScan();              // Запуск розыгрышей
        await _invite.InviteScanning();              // Проверка инвайтов
        await _refferal.StartRefferalScan();         // Запуск сканирования рефералок
        await _gift.StartGiftQuestion();             // Запуск Мат. задач
        await _privatesystem.PrivateChecking();      // Проверка приваток
        await _timer.StartBirthdates();              // Запуск дней рождений
        await _timer.StartVoiceAllActivity();        // Запуск активности в голосовых


        Console.WriteLine("Ready to work!");
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task UserLeft(SocketGuild Guild, SocketUser User)
    {
        await _guildlogs.InLeftedAndKickedUser(Guild, User);
        await _meeting.UserLeftAction(User, Guild);
    }

    private async Task UserVoiceStateUpdated(SocketUser User, SocketVoiceState Before, SocketVoiceState After)
    {
        //using var _db = new Db();
        var Settings = _db.Settings.FirstOrDefault();
        var userDiscord = User as SocketGuildUser;
        await _db.GetUser(userDiscord.Id);
        await _guildlogs.InVoicelogs(User, Before, After);


        if (Before.VoiceChannel != null)
        {
            var PrivateVoiceDiscord = userDiscord.Guild.GetVoiceChannel(Settings.PrivateVoiceChannelId);
            if (PrivateVoiceDiscord != null)
                await _privatesystem.PrivateChecking();
        }
            

        if (After.VoiceChannel != null)
        {
            if(Before.VoiceChannel == null)
                await _timer.StartVoiceActivity(userDiscord);

            if (Settings.PrivateVoiceChannelId == After.VoiceChannel.Id && !User.IsBot)
            {
                await _privatesystem.PrivateCreate(userDiscord, After.VoiceChannel);
            }
        }
    }

    private async Task UserJoined(SocketGuildUser userDiscord)
    {
        //using var _db = new Db();
        await _db.GetUser(userDiscord.Id);

        await _guildlogs.InJoinedUser(userDiscord);
        await _meeting.UserJoinAction(userDiscord);
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        //using var _db = new Db();
        if (message is not SocketUserMessage userMessage || message.Source != MessageSource.User)
            return;

        var context = new SocketCommandContext(_discord, userMessage);

        var TextChannel = await _db.GetTextChannel(userMessage.Channel.Id);
        var user = await _db.GetUser(userMessage.Author.Id);
        var Settings = _db.Settings.FirstOrDefault();

        bool deleteMessage = await _messagesolution.ChatRulesAutoModeration(context, TextChannel, Settings.Prefix);
        if (deleteMessage)
            return;

        if (TextChannel.giveXp)
            await _messagesolution.SetPointAsync(userMessage);



        var argPos = 0;
        if (!userMessage.HasStringPrefix(Settings.Prefix, ref argPos))
            return;

        if (!string.IsNullOrWhiteSpace(user.BlockReason))
        {
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor("Вы заблокированы!")
                .WithDescription($"Вы были заблокированы.\nПричина:{user.BlockReason}\n\nСнять блокировку: пишите администратору");
            await userMessage.Author?.SendMessageAsync("", false, emb.Build());
            return;
        }

        if (message.MentionedUsers.Any(x => x.IsBot))
            return;

        if (!TextChannel.useCommand)
        {

            bool CheckCommands(string CommandName)
            {
                var commands = _commands.Modules.FirstOrDefault(x => x.Name == CommandName)?.Commands;
                foreach (var command in commands)
                {
                    foreach (var Alias in command.Aliases)
                    {
                        if (userMessage.Content.StartsWith($"{Settings.Prefix}{Alias}"))
                            return true;
                    }
                }
                return false;
            }


            bool SuccessCommand = false;
            if (TextChannel.useRPcommand && CheckCommands("SfwGif")) // Проверка на RP команды
                SuccessCommand = true;
            else if (TextChannel.useAdminCommand && (CheckCommands("Admin") || CheckCommands("Moderator"))) // Проверка на Админ команды
                SuccessCommand = true;

            if (!SuccessCommand)
                return;
        }

        await _commands.ExecuteAsync(context, argPos, _services);
        return;
    }
}
