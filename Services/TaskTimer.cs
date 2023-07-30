using Microsoft.EntityFrameworkCore;
using XBOT.DataBase;

namespace XBOT.Services
{
    public class TaskTimer
    {
        public static DiscordSocketClient _client;

        public TaskTimer(DiscordSocketClient client)
        {
            _client = client;
        }


        internal static Task StartEveryHoursScan()
        {
            TimeSpan time = new TimeSpan(0, 60, 0);
            System.Timers.Timer TaskTime = new(time);
            TaskTime.AutoReset = false;
            TaskTime.Elapsed += (s, e) => EveryHoursScan();
            TaskTime.Start();
            return Task.CompletedTask;
        }
        private static async void EveryHoursScan()
        {
            using (db db = new())
            {
                var Users = _client.Guilds.First().Users;
                await Refferal_Service.ReferalRoleScaningUser(Users);
            }
        }



        private static readonly TimeSpan TimeAddExp = new TimeSpan(0, 0, 10);

        internal static async Task StartVoiceActivity(SocketGuildUser User)
        {
            TimeSpan time = TimeAddExp;
            Timer TaskTime = new Timer(VoiceActivity, User, time, time);
            await Task.CompletedTask;
        }
        private static async void VoiceActivity(object obj)
        {
            using (db db = new())
            {
                SocketGuildUser User = obj as SocketGuildUser;
                if (User.VoiceChannel != null && User.VoiceChannel.Id != User.Guild.AFKChannel?.Id)
                {
                    if (User.VoiceChannel.Users.Count > 1)
                    {
                        uint CountSpeak = 0;
                        bool ThisUserActive = false;
                        foreach (var UserChannel in User.VoiceChannel.Users)
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
                            var isPrivateChannel = await db.PrivateChannel.AnyAsync(x => x.Id == User.VoiceChannel.Id);
                            var user = await db.GetUser(User.Id);
                            if (isPrivateChannel)
                                user.voiceActive_private += TimeAddExp;
                            else
                                user.voiceActive_public += TimeAddExp;

                            user.XP += 10;
                            db.User.Update(user);
                            await db.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    Timer Timer = obj as Timer;
                    Timer?.Dispose();
                }
            }
        }


        internal static Task StartWarnTimer(User_Warn Warn)
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
        private static async void BanTimer(User_Warn Warn)
        {
            var Guild = _client.Guilds.FirstOrDefault();
            var ThisBan = await Guild.GetBanAsync(Warn.UserId);
            if (ThisBan != null)
                await Guild.RemoveBanAsync(ThisBan.User);
        }

        private static async void MuteTimer(User_Warn Warn) // Обновление бесконечного мута
        {
            using (var Db = new db())
            {
                Warn = Db.User_Warn.FirstOrDefault(x => x.Id == Warn.Id);
                if (Warn.UnWarn_Id == null)
                {
                    var Guild = _client.Guilds.FirstOrDefault();
                    var User = Guild.GetUser(Warn.UserId);
                    if (User != null)
                    {
                        var TimeSpan = new TimeSpan(28, 0, 0, 0);
                        await User.SetTimeOutAsync(TimeSpan);
                        Warn.ToTimeWarn.Add(TimeSpan);
                        Db.User_Warn.Update(Warn);
                        await Db.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
