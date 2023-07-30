using Discord;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Threading.Channels;
using XBOT.DataBase;
using XBOT.DataBase.Models;
using XBOT.DataBase.Models.Invites;
using XBOT.Services.Configuration;
using XBOT.Services.PrivateStructure;

namespace XBOT.Services.Handling
{
    public class CommandHandlingService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly ILogger<CommandService> _commandslogger;
        private ComponentEventService _componentEventService;

        public CommandHandlingService(
            DiscordSocketClient discord,
            CommandService commands,
            IServiceProvider services,
            ILogger<CommandService> commandslogger,
            ComponentEventService componentEventService)
        {
            _discord = discord;
            _commands = commands;
            _services = services;
            _commandslogger = commandslogger;


            _commands.Log += msg => LoggingService.OnLogAsync(_commandslogger, msg);
            _componentEventService = componentEventService;
        }

        private async Task SelectMenuExecuted(SocketMessageComponent interaction)
        {
            if ((DateTime.Now - interaction.CreatedAt.DateTime).TotalSeconds > 180)
                await interaction.Message.ModifyAsync(x => x.Components = new ComponentBuilder().Build());

            var userInteraction = _componentEventService.GetInteraction<SocketMessageComponent>(interaction.Data.CustomId);

            if (userInteraction != null)
            {
                userInteraction.Component = interaction;
                userInteraction.CompleteInteraction();
                await interaction.DeferAsync();
            }
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _discord.InviteCreated += _InviteCreated;

            _discord.UserJoined += _UserJoined;
            _discord.UserLeft += _UserLeft;

            _discord.SelectMenuExecuted += SelectMenuExecuted;
            _discord.ButtonExecuted += SelectMenuExecuted;
            _discord.MessageReceived += MessageReceivedAsync;
            _discord.UserVoiceStateUpdated += UserVoiceStateUpdated;

            //_discord.InviteDeleted += Invite_Service.InviteDelete;
            _discord.InviteCreated += Invite_Service.InviteCreate;

            _discord.UserBanned += Guild_Logs_Service.InUserBanned;
            _discord.UserUnbanned += Guild_Logs_Service.InUserUnBanned;

            _discord.MessageUpdated += Guild_Logs_Service.EditedMessage;
            _discord.MessageDeleted += Guild_Logs_Service.DeleteMessage;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task _UserLeft(SocketGuild Guild, SocketUser User)
        {
            await Guild_Logs_Service.InLeftedAndKickedUser(Guild, User);
            await Meeting_Logs_Service.UserLeftAction(User, Guild);
        }

        private async Task UserVoiceStateUpdated(SocketUser User, SocketVoiceState Before, SocketVoiceState After)
        {
            using (var db = new db())
            {
                var Settings = db.Settings.FirstOrDefault();
                var userDiscord = User as SocketGuildUser;
                var userDb = await db.GetUser(userDiscord.Id);
                await Guild_Logs_Service.InVoicelogs(User, Before, After);


                if (Before.VoiceChannel != null)
                {
                    var PrivateVoiceDiscord = userDiscord.Guild.GetVoiceChannel(Settings.PrivateVoiceChannelId);
                    if (PrivateVoiceDiscord != null)
                        await PrivateSystem.PrivateChecking(userDiscord.Guild);
                }

                if (After.VoiceChannel != null)
                {
                    if (Settings.PrivateVoiceChannelId == After.VoiceChannel.Id && !User.IsBot)
                    {
                        await PrivateSystem.PrivateCreate(userDiscord, After.VoiceChannel);
                    }
                }
            }
        }

        private async Task _UserJoined(SocketGuildUser userDiscord)
        {
            using (var _db = new db())
            {
                await _db.GetUser(userDiscord.Id);

                await Guild_Logs_Service.InJoinedUser(userDiscord);
                await Meeting_Logs_Service.UserJoinAction(userDiscord);
            }
        }

        private async Task _InviteCreated(SocketInvite Invite)
        {
            await Invite_Service.InviteCreate(Invite);
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            using (var _db = new db())
            {
                if (message is not SocketUserMessage userMessage || message.Source != MessageSource.User)
                    return;

                var TextChannel = await _db.GetTextChannel(userMessage.Channel.Id);
                var user = await _db.GetUser(userMessage.Author.Id);
                if (string.IsNullOrWhiteSpace(user.BlockReason))
                {
                    if (!TextChannel.useCommand)
                        return;

                    if (string.IsNullOrWhiteSpace(userMessage.Content))
                        return;

                    var Settings = _db.Settings.FirstOrDefault();

                    if (TextChannel.useRPcommand && !CheckCommands("RolePlay")) // Проверка на RP команды
                        return;
                    else if (TextChannel.useAdminCommand && !CheckCommands("Admin")) // Проверка на Админ команды
                        return;

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



                    var argPos = 0;
                    if (userMessage.HasStringPrefix(Settings.Prefix, ref argPos) && message.MentionedUsers.Count(x => x.IsBot) == 0)
                    {
                        var context = new SocketCommandContext(_discord, userMessage);
                        await _commands.ExecuteAsync(context, argPos, _services);
                    }
                }
                else
                {
                    var emb = new EmbedBuilder()
                        .WithColor(BotSettings.DiscordColor)
                        .WithAuthor("Вы заблокированы!")
                        .WithDescription($"Вы были заблокированы за препядствие работы бота.\nПричина:{user.BlockReason}\n\nСнять блокировку: пишите администратору");
                    await userMessage.Channel.SendMessageAsync("", false, emb.Build());
                }

                if (TextChannel.giveXp)
                    await Leveling.SetPointAsync(userMessage);
            }
        }
    }
}
