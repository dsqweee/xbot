using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using XBOT.DataBase.Models;
using XBOT.Services.Configuration;

namespace XBOT.Services;

public class TaskTimer
{
    private readonly DiscordSocketClient _client;
    private readonly Guild_Logs_Service _guildlogs;
    private readonly Db _db;
    public TaskTimer(DiscordSocketClient client, Guild_Logs_Service guildlogs, Db db)
    {
        _client = client;
        //_guildlogs = guildlogs;
        _db = db;

    }

    private readonly TimeSpan TimeAddExp = new TimeSpan(0, 0, 30);

    private readonly List<ulong> userInVoiceActive = new();

    internal Task StartVoiceActivity(SocketGuildUser User) /*СДелать проверку если перезапустится, чтобы включался механизм*/
    {
        if (!userInVoiceActive.Any(x => x == User.Id))
        {
            userInVoiceActive.Add(User.Id);

            System.Timers.Timer TaskTime = new(TimeAddExp);
            TaskTime.Elapsed += (s, e) => VoiceActivity(TaskTime, User);
            TaskTime.Start();
        }

        return Task.CompletedTask;
    }

    internal async Task StartVoiceAllActivity() /*СДелать проверку если перезапустится, чтобы включался механизм*/
    {
        var Guild = _client.Guilds.First();
        foreach (var VoiceChannel in Guild.VoiceChannels)
        {
            foreach (var User in VoiceChannel.ConnectedUsers)
            {
                await StartVoiceActivity(User);
            }
        }
    }

    private async void VoiceActivity(System.Timers.Timer TaskTime, SocketGuildUser User)
    {
        //using var _db = new Db();
        //using (var db = new Db(new DbContextOptionsBuilder<Db>().UseSqlite(BotSettings.connectionStringDbPath).Options))

        if (User.VoiceChannel != null && User.VoiceChannel.Id != User.Guild.AFKChannel?.Id)
        {
            if (User.VoiceChannel.ConnectedUsers.Count > 1)
            {
                uint CountSpeak = 0;
                bool ThisUserActive = false;
                foreach (var UserChannel in User.VoiceChannel.ConnectedUsers)
                {
                    var UserStatus = UserChannel.VoiceState.Value;

                    if (!UserStatus.IsMuted && !UserStatus.IsDeafened &&
                        !UserStatus.IsSelfMuted && !UserStatus.IsSelfDeafened &&
                        !UserChannel.IsBot)
                    {
                        CountSpeak++;

                        if (UserChannel.Id == User.Id)
                            ThisUserActive = true;
                    }
                }
                if (ThisUserActive && CountSpeak > 1)
                {
                    var isPrivateChannel = _db.PrivateChannel.FirstOrDefault(x => x.Id == User.VoiceChannel.Id);
                    var user = await _db.GetUser(User.Id);
                    if (isPrivateChannel is not null)
                        user.voiceActive_private += TimeAddExp;
                    else
                        user.voiceActive_public += TimeAddExp;
                    //Console.WriteLine(User.Mention + " - " + $" 30 сек 10 опыта {DateTime.Now}");
                    //user.XP += 5;
                    await _db.SaveChangesAsync();
                }
            }
        }
        else
        {
            userInVoiceActive.Remove(User.Id);
            TaskTime?.Dispose();
        }
    }


    internal Task StartWarnTimer(User_Warn Warn)
    {
        System.Timers.Timer TaskTime = new((Warn.ToTimeWarn - DateTime.Now).TotalMilliseconds);
        TaskTime.AutoReset = false;
        if (Warn.Guild_Warns.ReportTypes == Guild_Warn.ReportTypeEnum.TimeBan)
            TaskTime.Elapsed += (s, e) => BanTimer(Warn);
        else
            TaskTime.Elapsed += (s, e) => MuteTimer(Warn);
        TaskTime.Start();
        return Task.CompletedTask;
    }
    private async void BanTimer(User_Warn Warn)
    {
        var Guild = _client.Guilds.FirstOrDefault();
        var ThisBan = await Guild.GetBanAsync(Warn.UserId);
        if (ThisBan != null)
            await Guild.RemoveBanAsync(ThisBan.User);
    }

    private async void MuteTimer(User_Warn Warn) // Обновление бесконечного мута
    {
        //using var _db = new Db();
        Warn = _db.User_Warn.Include(x => x.UnWarn).FirstOrDefault(x => x.Id == Warn.Id);
        if (Warn.UnWarnId == null ||
           (Warn.UnWarn != null && Warn.UnWarn.Status != User_UnWarn.WarnStatus.review && Warn.UnWarn.Status != User_UnWarn.WarnStatus.Rejected))
        {
            var Guild = _client.Guilds.FirstOrDefault();
            var User = Guild.GetUser(Warn.UserId);
            if (User != null)
            {
                var TimeSpan = new TimeSpan(28, 0, 0, 0);
                await User.SetTimeOutAsync(TimeSpan);
                Warn.ToTimeWarn.Add(TimeSpan);
                _db.User_Warn.Update(Warn);
                await _db.SaveChangesAsync();
            }
        }

    }




    internal Task StartBirthdates()
    {
        //using var _db = new Db();
        var TimeNow = DateTime.Now;

        var UsersBirthdayInThisYear = _db.User.Where(x => x.BirthDateComplete == TimeNow.Year);
        foreach (var User in UsersBirthdayInThisYear)
        {
            DateTime UserDateTime = User.BirthDate.ToDateTime(new TimeOnly(10, 0, 0));
            var Time = UserDateTime.AddYears(TimeNow.Year - User.BirthDate.Year) - TimeNow;

            System.Timers.Timer TaskTime = new(Time.TotalHours <= 168 ? 1000 : Time.TotalMilliseconds);
            TaskTime.AutoReset = false;
            TaskTime.Elapsed += (s, e) => Birthdates(User);
            TaskTime.Start();

        }
        return Task.CompletedTask;

    }

    private async void Birthdates(User User)
    {
        //using var _db = new Db();
        User = _db.User.FirstOrDefault(x => x.Id == User.Id);
        var UserDiscord = _client.GetUser(User.Id) as SocketGuildUser;
        if (UserDiscord == null)
            return;

        var emb = new EmbedBuilder()
            .WithColor(BotSettings.DiscordColor)
            .WithAuthor("С днем рождения солнышко🎉");

        DateTime TimeNow = DateTime.Now;

        User.BirthDateComplete = TimeNow.Year + 1;
        _db.User.Update(User);
        await _db.SaveChangesAsync();

        await _guildlogs.Birthday(UserDiscord);


        if (TimeNow.Day <= User.BirthDate.Day)
            emb.WithDescription("Солнышко, я слышал у тебя сегодня день рождения?\n\nЯ робот, и не очень хорошо умею говорить умные слова,\nно спасибо что ты есть, не опускай руки, я тебя люблю <3");
        else
            emb.WithDescription("Солнышко, я слышал у тебя был день рождения?\nС днем рождения солнце!!!\n\nПрости не смог поздравить тебя сразу, надеюсь за меня это сделали твои друзья :)");
        try
        {
            await UserDiscord.SendMessageAsync("", false, emb.Build());
        }
        catch { }

    }
}
