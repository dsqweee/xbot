
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XBOT.Services;
using XBOT.Services.Configuration;
using XBOT.Services.Handling;


using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddYamlFile(BotSettings.TokenPath, false);
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<Culture>(); // Поддержка русской культуры
        services.AddSingleton(new DiscordSocketClient(XBOT.Services.Configuration.DiscordConfig.discordSocketConfig));
        services.AddSingleton(new CommandService(XBOT.Services.Configuration.DiscordConfig.configService));

        services.AddSingleton<ComponentEventService>();
        services.AddSingleton<TaskTimer>();
        services.AddSingleton<GiftQuestion_Service>();
        services.AddSingleton<Guild_Logs_Service>();
        services.AddSingleton<Invite_Service>();

        //services.AddSingleton<InteractionService>();
        //services.AddHostedService<InteractionHandlingService>();    
        services.AddHostedService<CommandHandlingService>();
        services.AddHostedService<DiscordStartupService>();         
    })
    .Build();

await host.RunAsync();