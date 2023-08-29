using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace XBOT.Services;

public class DiscordStartupService : IHostedService
{
    private readonly DiscordSocketClient _discord;
    private readonly IConfiguration _config;
    private readonly ILogger<DiscordSocketClient> _discordlogger;
    private readonly Db _db;

    public DiscordStartupService(DiscordSocketClient discord, IConfiguration config, ILogger<DiscordSocketClient> discordlogger, Db db)
    {
        _discord = discord;
        _config = config;
        _discordlogger = discordlogger;

        _discord.Log += msg => LoggingService.OnLogAsync(_discordlogger, msg);
        _db = db;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _db.Database.Migrate();
        await _db.SaveChangesAsync();

        var prefix = _db.Settings.FirstOrDefault().Prefix;
        string token = "";

        token = _config["token-test"];
        //token = _config["token"];
        await _discord.LoginAsync(TokenType.Bot, token);

        await _discord.StartAsync();
        await _discord.SetStatusAsync(UserStatus.DoNotDisturb);
        await _discord.SetGameAsync($"Префикс - [{prefix}]", null, ActivityType.Playing);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _discord.LogoutAsync();
        await _discord.StopAsync();
    }
}
