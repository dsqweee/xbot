namespace XBOT.Services
{
    public class Meeting_Logs_Service
    {
        public static async Task UserJoinAction(SocketGuildUser User)
        {
            using (var db = new db())
            {
                if (!User.IsBot)
                {
                    var Settings = db.Settings.FirstOrDefault();
                    var RoleId = Convert.ToUInt64(Settings.WelcomeRoleId);
                    if(RoleId != 0)
                        await User.AddRoleAsync(RoleId);

                    if (Settings.WelcomeDMmessage != null)
                        await MeetingSendMessage(User, User.Guild, 0, Settings.WelcomeDMmessage);

                    if(Settings.WelcomeMessage != null)
                    {
                        var WelcomeTextChannelId = Convert.ToUInt64(Settings.WelcomeTextChannelId);
                        if (WelcomeTextChannelId != 0)
                            await MeetingSendMessage(User, User.Guild, WelcomeTextChannelId, Settings.WelcomeMessage);
                    }
                }
            }
        }

        public static async Task UserLeftAction(SocketUser User, SocketGuild Guild)
        {
            using (var db = new db())
            {
                if (!User.IsBot)
                {
                    var Settings = db.Settings.FirstOrDefault();

                    if (Settings.WelcomeDMmessage != null)
                        await MeetingSendMessage(User, Guild, 0, Settings.WelcomeDMmessage);
                }
            }
        }
                                                                           // If zero, message send user
        private static async Task MeetingSendMessage(SocketUser User,SocketGuild Guild, ulong ChannelId = 0, string JsonMessage = null)
        {
            (EmbedBuilder,string) NewType = new();
            if (!string.IsNullOrWhiteSpace(JsonMessage))
            {
                NewType = JsonToEmbed.JsonCheck(JsonMessage);
                if (NewType.Item1 != null)
                {
                    var Description = NewType.Item1.Description;
                    if (Description.Length > 0 && Description.Contains("%user%"))
                        NewType.Item1.Description = Description.Replace("%user%", User.Mention);
                }
            }

            if (ChannelId != 0)
            {
                var WelcomeChannelDiscord = Guild.GetTextChannel(ChannelId);
                if (WelcomeChannelDiscord != null)
                    await WelcomeChannelDiscord.SendMessageAsync(NewType.Item2,false, NewType.Item1.Build());
            }
            else
                await User.SendMessageAsync(NewType.Item2,false, NewType.Item1.Build());
        }
    }
}
