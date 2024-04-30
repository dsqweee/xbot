using System.Text.RegularExpressions;
using XBOT.Services.Configuration;

namespace XBOT.Services;

public class UserMessagesSolution
{
    private readonly Db _db;

    public UserMessagesSolution(Db db)
    {
        _db = db;
    }

    public async Task SetPointAsync(SocketMessage Message)
    {
        //using var _db = new Db();
        var userDiscord = Message.Author as SocketGuildUser;
        var userDb = await _db.GetUser(userDiscord.Id);

        userDb.messageCounterForDaily += 1;
        userDb.LastMessageTime = DateTime.Now;

        var Roles = _db.Roles_Level.OrderBy(x => x.Level).ToList();
        var thisRole = Roles.LastOrDefault(x => x.Level <= userDb.Level);

        if(thisRole != null)
        {
            foreach (var role in Roles)
            {
                if (userDiscord.Roles.Any(x => x.Id == role.RoleId) && role.RoleId != thisRole.RoleId)
                    await userDiscord.RemoveRoleAsync(role.RoleId);
            }
        }
        

        var nextLevel = (ulong)Math.Sqrt((userDb.XP + User.ExpPlus) / User.PointFactor);
        if (nextLevel > userDb.Level)
        {
            var nextRole = Roles.FirstOrDefault(x => x.Level == nextLevel);
            if (nextRole != null)
            {
                if (thisRole != null)
                    await userDiscord.RemoveRoleAsync(thisRole.RoleId);

                await userDiscord.AddRoleAsync(nextRole.RoleId);
            }


            uint result = (uint)(User.DefaultMoney + ((User.DefaultMoney * 0.055) * (userDb.Level + 1)));
            userDb.money += result;


            long count = (userDb.Level + 1) * (uint)User.PointFactor * (userDb.Level + 1);
            long countNext = (userDb.Level + 2) * (uint)User.PointFactor * (userDb.Level + 2);

            long MessageFnextlevel = (countNext - count) / User.ExpPlus;

            var Fields = new EmbedBuilder().WithAuthor($"{userDiscord.Username} поднял уровень!", userDiscord.GetAvatarUrl())
                                           .WithColor(BotSettings.DiscordColor)
                                           .AddField("Уровень:", $"{userDb.Level + 1}", true)
                                           .AddField("Опыт:", $"{userDb.XP + User.ExpPlus}", true)
                                           .AddField("Coins:", $"+{result}", true)
                                           .WithFooter($"Спасибо за активность в чае <3");
            await Message.Channel.SendMessageAsync("", false, Fields.Build());
        }
        else if (thisRole != null)
        {
            if(userDiscord.Guild.GetRole(thisRole.RoleId) != null)
                await userDiscord.AddRoleAsync(thisRole.RoleId);
        }
            

        userDb.XP += User.ExpPlus;
        await _db.SaveChangesAsync();
    }


    public async Task<bool> ChatRulesAutoModeration(SocketCommandContext Context, TextChannel Channel, string Prefix)
    {
        //using var _db = new Db();
        string message = Context.Message.Content;
        var Settings = _db.Settings.FirstOrDefault();
        var UserRoles = Context.Guild.Roles;

        var isStuff = UserRoles.Any(x => x.Id == Settings.AdminRoleId || x.Id == Settings.ModeratorRoleId);
        if (isStuff)
        {
            return false; // Для модерации проверка ниже не производится
        }

        bool isDelete = false;

        if (Channel.delUrl)
        {
            isDelete = MessageDetectUrl(Context, Channel, message);
        }

        if (Channel.inviteLink)
        {
            isDelete = await MessageDetectInvite(Context);
        } 

        if(isDelete)
        {
            await Context.Message.DeleteAsync();
            return true;
        }

        return false;
    }


    private static bool MessageDetectUrl(SocketCommandContext Context, TextChannel Channel, string message)
    {
        string urlPattern = @"\b(?:https?://|www\.)\S+\b";
        Regex regex = new Regex(urlPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        if (!regex.IsMatch(message))
            return false;

        bool isImage = Context.Message.Embeds.Any(x => x.Type == EmbedType.Gifv || x.Type == EmbedType.Image);

        if (!isImage)
        {
            string contentPattern = @"(?:([^:/?#]+):)?(?://([^/?#]*))?([^?#]*\.(?:jpg|gif|png|mp4|webm|webp|jpeg|bmp))(?:\?([^#]*))?(?:#(.*))?";
            regex = new Regex(contentPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (regex.IsMatch(message))
                isImage = true;
        }

        if (isImage && Channel.delUrlImage || !isImage)
        {
            return true; // Происходит в случае если Это изображение и включено удаление ссылок или в случае если это не изображение
        }

        return false;
    }

    private static async Task<bool> MessageDetectInvite(SocketCommandContext Context)
    {
        string pattern = @"(?:https?://)?(?:\w+.)?discord(?:(?:app)?.com/invite|.gg)/([A-Za-z0-9-]+)";
        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        string content = Context.Message.Content.Replace(" ", "");

        if (!regex.IsMatch(content))
            return false;

        var guildInvites = await Context.Guild.GetInvitesAsync();
        var inviteToThisGuild = guildInvites.Any(x => content.Contains(x.Id));

        if (!inviteToThisGuild)
            return true; // Проверка является ли приглашение на этот сервер

        return false;
    }
}
