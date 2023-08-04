using Discord.Interactions;
using Discord.Rest;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using XBOT.DataBase.Models;
using XBOT.DataBase;
using XBOT.Services.Configuration;
using XBOT.Services.PrivateStructure;
using XBOT.Modules.Command;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace XBOT.Services.Handling
{
    public class CommandHandlingService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly TaskTimer _timer;
        private readonly GiftQuestion_Service _gift;
        private readonly Invite_Service _invite;
        private readonly Guild_Logs_Service _guildlogs;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly ILogger<CommandService> _commandslogger;
        private ComponentEventService _componentEventService;

        public CommandHandlingService(
            DiscordSocketClient discord,
            CommandService commands,
            IServiceProvider services,
            ILogger<CommandService> commandslogger,
            ComponentEventService componentEventService,
            TaskTimer timer,
            GiftQuestion_Service gift,
            Invite_Service invite,
            Guild_Logs_Service guildlogs)
        {
            _discord = discord;
            _commands = commands;
            _services = services;
            _commandslogger = commandslogger;

            _commands.Log += msg => LoggingService.OnLogAsync(_commandslogger, msg);
            _componentEventService = componentEventService;
            _timer = timer;
            _gift = gift;
            _invite = invite;
            _guildlogs = guildlogs;
        }

        private async Task SelectMenuExecuted(SocketMessageComponent interaction)
        {
            if(interaction.Data.Type == ComponentType.SelectMenu)
            {
                var result = ulong.TryParse(interaction.Data.Value, out ulong ulongvalue);
                if(result)
                {
                    await (interaction.User as SocketGuildUser).AddRoleAsync(ulongvalue);
                    await interaction.DeferAsync();
                }
            }
            else if(interaction.Data.Type == ComponentType.Button)
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
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _discord.UserJoined += UserJoined;
            _discord.UserLeft += UserLeft;

            _discord.Ready += Ready;

            _discord.SelectMenuExecuted += SelectMenuExecuted;
            _discord.ButtonExecuted += SelectMenuExecuted;
            _discord.MessageReceived += MessageReceivedAsync;
            _discord.UserVoiceStateUpdated += UserVoiceStateUpdated;

            //_discord.InviteDeleted += Invite_Service.InviteDelete;
            _discord.InviteCreated += _invite.InviteCreate;

            _discord.UserBanned += _guildlogs.InUserBanned;
            _discord.UserUnbanned += _guildlogs.InUserUnBanned;

            _discord.MessageUpdated += _guildlogs.EditedMessage;
            _discord.MessageDeleted += _guildlogs.DeleteMessage;


            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }


        private async Task ReactAdd(Cacheable<IUserMessage, ulong> mess, Cacheable<IMessageChannel, ulong> chnl, SocketReaction emj)
    => await GetOrRemoveRole(mess, emj, false);
        private async Task ReactRem(Cacheable<IUserMessage, ulong> mess, Cacheable<IMessageChannel, ulong> chnl, SocketReaction emj)
            => await GetOrRemoveRole(mess, emj, true);
        private static async Task GetOrRemoveRole(Cacheable<IUserMessage, ulong> mess, SocketReaction emj, bool getOrRemove)
        {
            using (db _db = new())
            {
                if (emj.User.Value.IsBot)
                    return;

                //var message = await mess.GetOrDownloadAsync();
                //if (message != null)
                //{
                //    string EmoteName = emj.Emote.ToString();
                //    var mes = _db.EmoteClick.FirstOrDefault(x => x.MessageId == message.Id && EmoteName == x.Emote);
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
            }
        } // Проверка эмодзи в событии ReactRem и ReactAdd



        private async Task Ready()
        {
            using (var _db = new db())
            {
                Console.WriteLine($"Connected and Start Scanning!");

                var Guild = _discord.Guilds.FirstOrDefault();
                var Invites = await Guild.GetInvitesAsync();

                await _invite.InviteScanning(); // Проверка инвайтов

                await _timer.StartEveryHoursScan();

                await _gift.StartGiftQuestion();

                await PrivateSystem.PrivateChecking(Guild); // Проверка приваток

                await _timer.StartBirthdates();

                foreach (var Give in _db.GiveAways)
                {
                    var TextChannel = Guild.GetTextChannel(Give.TextChannelId);
                    if(TextChannel != null)
                    {
                        var Message = await TextChannel.GetMessageAsync(Give.Id);
                        if (Message != null)
                            await IventerModule.GiveAwayTimer(Give, Message as RestUserMessage);
                        else
                            _db.GiveAways.Remove(Give);
                    }
                    else
                        _db.GiveAways.Remove(Give);
                } // проверка розыгрышей
                await _db.SaveChangesAsync();

                Console.WriteLine("Ready to work!");
            }
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task UserLeft(SocketGuild Guild, SocketUser User)
        {
            await _guildlogs.InLeftedAndKickedUser(Guild, User);
            await Meeting_Logs_Service.UserLeftAction(User, Guild);
        }

        private async Task UserVoiceStateUpdated(SocketUser User, SocketVoiceState Before, SocketVoiceState After)
        {
            using (var db = new db())
            {
                var Settings = db.Settings.FirstOrDefault();
                var userDiscord = User as SocketGuildUser;
                var userDb = await db.GetUser(userDiscord.Id);
                await _guildlogs.InVoicelogs(User, Before, After);


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

        private async Task UserJoined(SocketGuildUser userDiscord)
        {
            using (var _db = new db())
            {
                await _db.GetUser(userDiscord.Id);

                await _guildlogs.InJoinedUser(userDiscord);
                await Meeting_Logs_Service.UserJoinAction(userDiscord);
            }
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            using (var _db = new db())
            {
                if (message is not SocketUserMessage userMessage || message.Source != MessageSource.User)
                    return;


                var TextChannel = await _db.GetTextChannel(userMessage.Channel.Id);
                var user = await _db.GetUser(userMessage.Author.Id);
                var Settings = _db.Settings.FirstOrDefault();
                var context = new SocketCommandContext(_discord, userMessage);

                if (await MessageScanning.ChatSystem(context, TextChannel, Settings.Prefix))
                {
                    return;
                }

                if (TextChannel.giveXp)
                    await Leveling.SetPointAsync(userMessage);


                if (string.IsNullOrWhiteSpace(userMessage.Content) || !TextChannel.useCommand)
                    return;

                string sanitizedInput = Regex.Replace(userMessage.Content, @"\s+", "");
                if (Regex.IsMatch(sanitizedInput, @"^-?\d+$"))
                {
                    if (long.TryParse(sanitizedInput, out long numberAnswer))
                    {
                        await _gift.QuestionAnswerScan(userMessage.Author as SocketGuildUser, numberAnswer);
                    }
                }
                    
                    

                if (!TextChannel.useRPcommand && CheckCommands("SfwGif")) // Проверка на RP команды
                    return;
                else if (!TextChannel.useAdminCommand && CheckCommands("Admin")) // Проверка на Админ команды
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
                    if (string.IsNullOrWhiteSpace(user.BlockReason))
                    {
                        await _commands.ExecuteAsync(context, argPos, _services);
                    }
                    else
                    {
                        var emb = new EmbedBuilder()
                            .WithColor(BotSettings.DiscordColor)
                            .WithAuthor("Вы заблокированы!")
                            .WithDescription($"Вы были заблокированы за препядствие работы бота.\nПричина:{user.BlockReason}\n\nСнять блокировку: пишите администратору");
                        await userMessage.Channel.SendMessageAsync("", false, emb.Build());
                    }
                }
            }
        }
    }
}
