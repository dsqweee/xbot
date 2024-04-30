using Discord.Rest;
using XBOT.Services.Configuration;

namespace XBOT.Services.PrivateStructure;

public class PrivateSystem
{
    private readonly Db _db;
    private readonly DiscordSocketClient _client;

    public PrivateSystem(Db db, DiscordSocketClient client)
    {
        _db = db;
        _client = client;
    }

    public async Task PrivateChecking()
    {
        Console.WriteLine("--- PrivateChecking START ---");
        //using var _db = new Db();
        var guild = _client.Guilds.First();
        foreach (var PC in _db.PrivateChannel)
        {
            var chnl = guild.GetVoiceChannel(PC.Id);
            if (chnl != null)
                await PrivateScanUsers(chnl, PC);
            else
                _db.PrivateChannel.Remove(PC);
        }
        await _db.SaveChangesAsync();
        Console.WriteLine("--- PrivateChecking STOP ---");
    }

    public async Task PrivateScanUsers(SocketVoiceChannel VoiceChannel, PrivateChannel ThisPrivateChannel)
    {
        //using var _db = new Db();
        if (VoiceChannel.ConnectedUsers.Count == 0)
        {
            await VoiceChannel.DeleteAsync();
            _db.PrivateChannel.Remove(ThisPrivateChannel);
            await _db.SaveChangesAsync();
        }
        else if (!VoiceChannel.ConnectedUsers.Any(x => x.Id == ThisPrivateChannel.UserId))
        {
            var newusr = VoiceChannel.ConnectedUsers.FirstOrDefault();
            var newusrDb = await _db.GetUser(newusr.Id);

            var oldusr = VoiceChannel.GetUser(ThisPrivateChannel.UserId);
            ThisPrivateChannel.UserId = newusrDb.Id;
            await _db.SaveChangesAsync();
            await VoiceChannel.RemovePermissionOverwriteAsync(oldusr);
            await VoiceChannel.AddPermissionOverwriteAsync(newusr, PermissionCreatorChannel);
        }
    }

    private readonly OverwritePermissions PermissionCreatorChannel = new(connect: PermValue.Allow, muteMembers: PermValue.Allow, deafenMembers: PermValue.Allow, moveMembers: PermValue.Allow, manageChannel: PermValue.Allow, manageRoles: PermValue.Allow);

    public async Task PrivateCreate(SocketGuildUser user, SocketVoiceChannel PrivateChannel)
    {
        //using var _db = new Db();
        if (PrivateSpam.CheckSpamPrivate(user))
            return;

        PrivateSpam.AddUser(user);
        if (PrivateChannel.Category == null)
        {
            var cat = await user.Guild.CreateCategoryChannelAsync(BotSettings.PrivateCategoryName, X => { X.Position = int.MaxValue; });
            await PrivateChannel.ModifyAsync(x => x.CategoryId = cat.Id);
        }

        RestVoiceChannel voicechannel = null;
        try
        {
            var everyoneRole = new OverwritePermissions(connect: PermValue.Deny);
            var voicePermissions = new List<Overwrite> { new Overwrite(user.Id, PermissionTarget.User, PermissionCreatorChannel), new Overwrite(user.Guild.EveryoneRole.Id, PermissionTarget.Role, everyoneRole) };
            voicechannel = await user.Guild.CreateVoiceChannelAsync($"{user}` VOICE", x => { x.CategoryId = PrivateChannel.CategoryId; x.PermissionOverwrites = voicePermissions; });
            await user.ModifyAsync(x => x.Channel = voicechannel);

            var UserDb = await _db.GetUser(user.Id);
            _db.PrivateChannel.Add(new PrivateChannel() { UserId = UserDb.Id, Id = voicechannel.Id });
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await voicechannel?.DeleteAsync();
        }
    }
}
