using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RconSharp;
using System.Net;
using XBOT.Services.Configuration;

namespace XBOT.Services;

public class Minecraft_Service
{
    private readonly DiscordSocketClient _discord;
    private readonly Db _db;

    public Minecraft_Service(DiscordSocketClient discord, Db db)
    {
        _discord = discord;
        _db = db;
    }
    public async Task<string> SendRconQuery(string query)
    {

        var Settings = _db.Settings.FirstOrDefault();

        var client = RconClient.Create(Settings.minecraft_IP, Settings.minecraft_port);
        await client.ConnectAsync();

        var authenticated = await client.AuthenticateAsync(Settings.minecraft_Key);
        if (authenticated)
        {
            var status = await client.ExecuteCommandAsync(query);
            client.Disconnect();
            return status;
        }
        else
            return null;

    }

    public async Task SubscribePaymentChecker()
    {

        while (true)
        {
            using (WebClient wc = new())
            {
                try
                {
                    QiwiTransactions[] json = JsonConvert.DeserializeObject<QiwiTransactions[]>(wc.DownloadString(BotSettings.PayURL));
                    foreach (QiwiTransactions token in json)
                    {
                        var payments = _db.QiwiTransactions.FirstOrDefault(x => x.discord_id == token.discord_id && x.invoice_date_add == token.invoice_date_add && x.invoice_ammount == token.invoice_ammount);
                        if (payments == null)
                        {
                            _db.QiwiTransactions.Add(token);
                            var user = _db.User.Include(x => x.MinecraftAccount).FirstOrDefault(x => x.Id == token.discord_id);

                            if (user.MinecraftAccount.LicenceTo > DateTime.Now)
                            {
                                user.MinecraftAccount.LicenceTo = user.MinecraftAccount.LicenceTo.AddMonths(1);
                            }
                            else
                            {
                                user.MinecraftAccount.LicenceTo = DateTime.Now.AddMonths(1);
                                await SendRconQuery($"whitelist add {user.MinecraftAccount.MinecraftName}");
                                await StartMinecraftLicenceScan(user.MinecraftAccount);
                            }
                            user.MinecraftAccount.whitelistAdded = true;
                            _db.User.Update(user);
                            await _db.SaveChangesAsync();
                        }
                    }
                }
                catch { }
            }

            await Task.Delay(60000);
        }

    }

    // Licence timer user
    private Task StartMinecraftLicenceScan(User_MinecraftAccount acc)
    {
        System.Timers.Timer TaskTime = new(acc.LicenceTo - DateTime.Now);
        TaskTime.Elapsed += (s, e) => MinecraftLicenceScan(acc.MinecraftName);
        TaskTime.Start();
        return Task.CompletedTask;
    }

    private async Task UserLicence(string MinecraftName, bool Activate)
    {

        var user = _db.User_MinecraftAccount.FirstOrDefault(x => x.MinecraftName == MinecraftName);
        user.LicenceTo = DateTime.Now.AddMonths(Activate ? 1 : 0);
        user.whitelistAdded = Activate;
        await SendRconQuery($"whitelist {(Activate ? "add" : "remove")} {user.MinecraftName}");
        _db.Update(user);
        await _db.SaveChangesAsync();

        if (Activate)
            await StartMinecraftLicenceScan(user);

    }

    public async Task UserLicenceActivate(string MinecraftName)
        => await UserLicence(MinecraftName, true);

    public async Task UserLicenceDeActivate(string MinecraftName)
        => await UserLicence(MinecraftName, false);


    // Scan one user
    private async void MinecraftLicenceScan(string MinecraftName)
    {
        var user = _db.User_MinecraftAccount.FirstOrDefault(x => x.MinecraftName == MinecraftName);
        if (user.LicenceTo > DateTime.Now)
        {
            await StartMinecraftLicenceScan(user);
            return;
        }

        await SendRconQuery($"whitelist remove {user.MinecraftName}");
        user.whitelistAdded = false;
        _db.Update(user);
        await _db.SaveChangesAsync();
    }


    // Scann all users
    public async void MinecraftAllScan()
    { 
        string request = await SendRconQuery($"whitelist list");
        var userList = await minecraftWhiteListStringtoUserList(request);

        foreach (var user in _db.User_MinecraftAccount)
        {
            var thiswhitelist = userList.FirstOrDefault(user.MinecraftName);
            if (user.LicenceTo > DateTime.Now)
            {
                if (thiswhitelist == null)
                {
                    await SendRconQuery($"whitelist add {user.MinecraftName}");
                    user.whitelistAdded = true;
                }

                await StartMinecraftLicenceScan(user);
            }
            else
            {
                if (thiswhitelist != null)
                {
                    await SendRconQuery($"whitelist remove {user.MinecraftName}");
                    user.whitelistAdded = false;
                }
            }
            _db.User_MinecraftAccount.Update(user);
        }
        await _db.SaveChangesAsync();
    }


    // whitelist text to List<string>
    private async Task<List<string>> minecraftWhiteListStringtoUserList(string request)
    {
        var userList = new List<string>();
        if (request == null || request != "There are no whitelisted players" || !request.Contains("whitelisted players:"))
        {
            request = "Проблемы с Rcon\n\n";
            await _discord.GetUser(BotSettings.xId).SendMessageAsync(request);
            return userList;
        }

        userList = request?.Replace(" ", "").Split(":")[1].Split(',').ToList();
        return userList;
    }
}
