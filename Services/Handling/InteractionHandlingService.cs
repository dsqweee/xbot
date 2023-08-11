using Discord.Interactions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using XBOT.Services.Configuration;

namespace XBOT.Services.Handling;

public class InteractionHandlingService : IHostedService
{
    private readonly DiscordSocketClient _discord;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    ILogger<InteractionService> _interactionlogger;


    public InteractionHandlingService(
        DiscordSocketClient discord,
        InteractionService interactions,
        IServiceProvider services,
        ILogger<InteractionService> interactionlogger)
    {
        _discord = discord;
        _interactions = interactions;
        _services = services;
        _interactionlogger = interactionlogger;

        //_discord.SelectMenuExecuted += SelectMenuExecuted;
        //_discord.ButtonExecuted += SelectMenuExecuted;
        _interactions.Log += msg => LoggingService.OnLogAsync(_interactionlogger, msg);
    }

    //private async Task SelectMenuExecuted(SocketMessageComponent interaction)
    //{
    //    if ((DateTime.Now - interaction.CreatedAt.DateTime).TotalSeconds > 180)
    //        await interaction.Message.ModifyAsync(x => x.Components = new ComponentBuilder().Build());

    //    var userInteraction = _componentEventService.GetInteraction<SocketMessageComponent>(interaction.Data.CustomId);

    //    if (userInteraction != null)
    //    {
    //        userInteraction.Component = interaction;
    //        userInteraction.CompleteInteraction();
    //        await interaction.DeferAsync();
    //    }
    //}

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _discord.Ready += () => _interactions.RegisterCommandsGloballyAsync(true);
        _discord.InteractionCreated += OnInteractionAsync;

        await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _interactions.Dispose();
        return Task.CompletedTask;
    }

    private async Task OnInteractionAsync(SocketInteraction interaction)
    {
        try
        {
            if (interaction.User.IsBot)
                return;

            // Проверка есть ли в упоминании бот
            var options = ((IApplicationCommandInteraction)interaction).Data.Options;
            var MentionBot = false;
            foreach (var Properties in options)
            {
                if (Properties.Type == ApplicationCommandOptionType.User)
                {
                    var User = Properties.Value as IUser;
                    if (User.IsBot)
                        MentionBot = true;
                }
            }
            if (MentionBot)
            {
                var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColorError)
                                            .WithAuthor("Ошибка команды")
                                            .WithDescription("Использовать команду на бота нельзя!");
                await interaction.RespondAsync("", embed: emb.Build());
                return;
            }



            var context = new SocketInteractionContext(_discord, interaction);
            var result = await _interactions.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ToString());

        }
        catch
        {
            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(msg => msg.Result.DeleteAsync());
            }
        }
    }
}
