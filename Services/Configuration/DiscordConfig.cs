using Discord.Interactions;

namespace XBOT.Services.Configuration;

public class DiscordConfig
{
    public static DiscordSocketConfig discordSocketConfig = new DiscordSocketConfig
    {
        GatewayIntents =
                GatewayIntents.AllUnprivileged |
                GatewayIntents.MessageContent |
                GatewayIntents.Guilds |
                GatewayIntents.GuildMembers |
                GatewayIntents.GuildMessageReactions |
                GatewayIntents.GuildMessages |
                GatewayIntents.GuildVoiceStates | 
                GatewayIntents.DirectMessages,
                
        UseInteractionSnowflakeDate = false,
        MessageCacheSize = 128,
        DefaultRetryMode = RetryMode.Retry502,
        AlwaysDownloadUsers = true,
    };

    public static CommandServiceConfig configService = new CommandServiceConfig
    {
        LogLevel = LogSeverity.Verbose,
        DefaultRunMode = Discord.Commands.RunMode.Async,
    };

    public static InteractionServiceConfig configInteraction = new InteractionServiceConfig
    {
        LogLevel = LogSeverity.Verbose,
        DefaultRunMode = Discord.Interactions.RunMode.Async,
        AutoServiceScopes = true,
        EnableAutocompleteHandlers = true,
        ExitOnMissingModalField = true,
        UseCompiledLambda = true,
    };
}
