using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace XBOT.Services
{
    public class DiscordStartupService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IConfiguration _config;
        private readonly ILogger<DiscordSocketClient> _discordlogger;

        public DiscordStartupService(DiscordSocketClient discord, IConfiguration config, ILogger<DiscordSocketClient> discordlogger)
        {
            _discord = discord;
            _config = config;
            _discordlogger = discordlogger;

            _discord.Log += msg => LoggingService.OnLogAsync(_discordlogger, msg);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var mContext = new db())
            {
                mContext.Database.Migrate();
                await mContext.SaveChangesAsync();
                mContext.Dispose();

                await _discord.LoginAsync(TokenType.Bot, _config["token"]);

                await _discord.StartAsync();
                await _discord.SetStatusAsync(UserStatus.DoNotDisturb);
                await _discord.SetGameAsync("Загрузка бота!", null, ActivityType.Playing);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discord.LogoutAsync();
            await _discord.StopAsync();
        }
    }
}
