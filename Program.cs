using Discord;
using Fergun.Interactive;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XBOT.Services;
using XBOT.Services.Configuration;
using XBOT.Services.Handling;
using XBOT.Services.PrivateStructure;


using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddYamlFile(BotSettings.tokenPath, false);
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<Culture>(); // Поддержка русской культуры
        services.AddSingleton(new DiscordSocketClient(XBOT.Services.Configuration.DiscordConfig.discordSocketConfig));
        services.AddSingleton(new CommandService(XBOT.Services.Configuration.DiscordConfig.configService));

        
        //services.AddDbContext<Db>(x=>
        //{
        //    x.UseSqlite(BotSettings.connectionStringDbPath);
        //    x.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddFilter((category, level) => false)));
        //});
        services.AddSingleton<TaskTimer>();
        services.AddSingleton<Refferal_Service>();
        services.AddSingleton<GiveAway_Service>();
        services.AddSingleton<UserMessagesSolution>();
        services.AddSingleton<PrivateSystem>();
        services.AddSingleton<Meeting_Logs_Service>();
        services.AddSingleton<GiftQuestion_Service>();
        services.AddSingleton<Guild_Logs_Service>();
        services.AddSingleton<Invite_Service>();
        //services.AddSingleton<ListBuilder>();
        //services.AddSingleton(new InteractiveConfig { ReturnAfterSendingPaginator = true });
        services.AddSingleton<InteractiveService>();
        //services.AddSingleton<InteractionService>();
        //services.AddHostedService<InteractionHandlingService>();    
        services.AddHostedService<CommandHandlingService>();
        services.AddHostedService<DiscordStartupService>();         
    })
    .Build();
await host.RunAsync();