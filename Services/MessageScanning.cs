using System.Text.RegularExpressions;

namespace XBOT.Services
{
    public class MessageScanning
    {
        public async void ChatSystem(SocketCommandContext Context, TextChannel Channel, string Prefix)
        {
            using (var _db = new db())
            {
                string message = Context.Message.Content;

                var Settings = _db.Settings.FirstOrDefault();
                var UserRoles = Context.Guild.CurrentUser.Roles;
                if (!UserRoles.Any(x => x.Id == Settings.AdminRoleId) &&
                    !UserRoles.Any(x => x.Id == Settings.ModeratorRoleId))
                {
                    return;
                }

                if(Channel.delUrl)
                {
                    string urlPattern = @"\b(?:https?://|www\.)\S+\b";
                    Regex regex = new Regex(urlPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    if (regex.IsMatch(message))
                    {
                        bool isImage = Context.Message.Embeds.Any(x => x.Type == EmbedType.Gifv || x.Type == EmbedType.Image);

                        if (isImage && Channel.delUrlImage || !isImage)
                        {
                            await Context.Message.DeleteAsync();
                            return;
                        }
                    }
                }

                if (Channel.inviteLink)
                {
                    string pattern = @"(?:https?://)?(?:\w+.)?discord(?:(?:app)?.com/invite|.gg)/([A-Za-z0-9-]+)";
                    Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    string content = Context.Message.Content.Replace(" ", "").ToLower();

                    if (regex.IsMatch(content))
                    {
                        var invite = await Context.Guild.GetInvitesAsync();
                        var invitex = invite.FirstOrDefault(x => content.Contains(x.Id));

                        if (invitex == null)
                        {
                            await Context.Message.DeleteAsync();
                            return;
                        }
                    }
                } // ИНВАЙТЫ


            }
        }

    }
}
