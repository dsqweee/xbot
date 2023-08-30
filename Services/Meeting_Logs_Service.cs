namespace XBOT.Services;

public class Meeting_Logs_Service
{
    //private readonly Db _db;

    //public Meeting_Logs_Service(Db db)
    //{
    //    _db = db;
    //}

    public async Task UserJoinAction(SocketGuildUser User)
    {
        if (User.IsBot)
            return;
        using var _db = new Db();
        var Settings = _db.Settings.FirstOrDefault();
        var RoleId = Convert.ToUInt64(Settings.WelcomeRoleId);

        if (RoleId != 0)
            await User.AddRoleAsync(RoleId);

        await MeetingSendMessage(User, User.Guild, Settings.WelcomeDMmessage, 0);

        var WelcomeTextChannelId = Convert.ToUInt64(Settings.WelcomeTextChannelId);
        await MeetingSendMessage(User, User.Guild, Settings.WelcomeMessage, WelcomeTextChannelId);
    }

    public async Task UserLeftAction(SocketUser User, SocketGuild Guild)
    {
        if (User.IsBot)
            return;
        using var _db = new Db();
        var Settings = _db.Settings.FirstOrDefault();
        await MeetingSendMessage(User, Guild, Settings.WelcomeDMmessage, 0);
    }

    // If zero, message send user
    private async Task MeetingSendMessage(SocketUser User, SocketGuild Guild, string JsonMessage, ulong ChannelId = 0)
    {
        (EmbedBuilder emb, string message) NewType = new();

        if (string.IsNullOrWhiteSpace(JsonMessage))
            return;

        NewType = JsonToEmbed.JsonCheck(JsonMessage);
        if (NewType.emb is not null)
        {
            var Description = NewType.emb.Description;
            if (!string.IsNullOrWhiteSpace(Description))
                NewType.emb.Description = Description.Replace("%user%", User.Mention);
        }

        if (ChannelId != 0)
        {
            var WelcomeChannelDiscord = Guild.GetTextChannel(ChannelId);
            if (WelcomeChannelDiscord != null)
                await WelcomeChannelDiscord.SendMessageAsync(NewType.message, false, NewType.emb.Build());
        }
        else
        {
            await User.SendMessageAsync(NewType.message, false, NewType.emb.Build());
        }
    }
}
