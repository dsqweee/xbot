using Discord;
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
        //using var _db = new Db();
        var guild = _client.Guilds.First();
        foreach (var PC in _db.PrivateChannel)
        {
            var chnl = guild.GetVoiceChannel(PC.Id);

            if(chnl == null || chnl?.ConnectedUsers.Count == 0) // Если нет канала, и если есть, но 0 человек
            {
                if(chnl != null)
                    await chnl.DeleteAsync();
                _db.PrivateChannel.Remove(PC);
            }
        }
        await _db.SaveChangesAsync();
    }


    private readonly OverwritePermissions PermissionCreatorChannel = new(connect: PermValue.Allow, muteMembers: PermValue.Allow, deafenMembers: PermValue.Allow, moveMembers: PermValue.Allow, manageChannel: PermValue.Allow, manageRoles: PermValue.Allow, manageWebhooks: PermValue.Allow);

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

            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor);
            emb.WithAuthor("Настрока приватных каналов");
            emb.WithDescription("Для настройки приватного канала, используй шестеренку, справа сверху голосового канала (как показано на фото).\nТак ты сможешь выдать доступ своим друзьям!\n(p.s. твой канал закрыт для всех по дефолту!)");
            emb.WithImageUrl("https://media.discordapp.net/attachments/1128513695270580236/1242120561128116224/f1d2db3eb37c47b4.PNG?ex=664cae6b&is=664b5ceb&hm=01172afab9fd063e0e6ee333eabdbe7d652677c1b9b3de44af3141a3bf716e7a&=&format=webp&quality=lossless");
            await voicechannel.SendMessageAsync($"<@{UserDb.Id}>", false, emb.Build());

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await voicechannel?.DeleteAsync();
        }
    }
}
