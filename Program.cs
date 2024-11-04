using Fergun.Interactive;
using XBOT.Services;
using XBOT.Services.Configuration;
using XBOT.Services.Handling;
using XBOT.Services.PrivateStructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


//var builder = Host.CreateApplicationBuilder(args);

//builder.Services.AddDiscordHost((config, _) =>
//{
//    config.SocketConfig = XBOT.Services.Configuration.DiscordConfig.discordSocketConfig;

//    config.Token = builder.Configuration["Token-Test"]!;
//});

//builder.Services.AddCommandService((config, _) =>
//{
//    config = XBOT.Services.Configuration.DiscordConfig.configService;
//});

////builder.Services.AddInteractionService((config, _) =>
////{
////    config = XBOT.Services.Configuration.DiscordConfig.configInteraction;
////});

////services.AddSingleton<InteractionService>();
//builder.Services.AddSingleton<Culture>(); // Поддержка русской культуры

//builder.Services.AddDbContext<Db>(x =>
//{
//    x.UseSqlite(BotSettings.connectionStringDbPath);
//    x.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddFilter((category, level) => false)));
//});
////builder.Services.AddSingleton<TaskTimer>();
////builder.Services.AddSingleton<Refferal_Service>();
////builder.Services.AddSingleton<GiveAway_Service>();
////builder.Services.AddSingleton<UserMessagesSolution>();
////builder.Services.AddSingleton<PrivateSystem>();
////builder.Services.AddSingleton<Meeting_Logs_Service>();
////builder.Services.AddSingleton<GiftQuestion_Service>();
////builder.Services.AddSingleton<Guild_Logs_Service>();
////builder.Services.AddSingleton<Invite_Service>();
////builder.Services.AddSingleton<InteractiveService>();

////builder.Services.AddHostedService<DiscordStartupService>();

//builder.Services.AddHostedService<CommandHandlingService>();

////services.AddHostedService<InteractionHandlingService>();

//var host = builder.Build();
//await host.RunAsync();


using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddYamlFile(BotSettings.tokenPath, false);
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<Culture>(); // Поддержка русской культуры
        var client = new DiscordSocketClient(XBOT.Services.Configuration.DiscordConfig.discordSocketConfig);
        services.AddSingleton(client);
        services.AddSingleton(new CommandService(XBOT.Services.Configuration.DiscordConfig.configService));

        //services.AddSingleton<InteractionService>();
        //services.AddHostedService<InteractionHandlingService>();

        services.AddDbContext<Db>(x =>
        {
            x.UseSqlite(BotSettings.connectionStringDbPath);
            x.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddFilter((category, level) => false)));
            //x.UseLazyLoadingProxies(); // Загрузка Includes
        }, ServiceLifetime.Scoped); // 

        
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

        services.AddHostedService<CommandHandlingService>();
        services.AddHostedService<DiscordStartupService>();
        
    })
    .Build();


await host.RunAsync();
