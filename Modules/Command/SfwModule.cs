using Nekos.Net.V2;
using XBOT.Services.Attribute;
using XBOT.Services.Configuration;
using Nekos.Net.V2.Endpoint;

namespace DarlingNet.Modules
{
    [Name("SfwGif")]
    [Summary("RP гифки")]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [RequireBotPermission(ChannelPermission.EmbedLinks)]
    public class RPgif : ModuleBase<SocketCommandContext>
    {
        private async Task GenerateGif(SfwEndpoint Type, SocketUser user = null, string text = "")
        {
            var embed = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"{Type} GIF");
            NekosV2Client NekoClient = new();
            var Request = await NekoClient.RequestSfwResultsAsync(Type);
            string Description = $"{Context.User.Mention} " + text;

            if (user != null && user != Context.User)
                Description += $" {user.Mention}";
            else if(user != null && user == Context.User)
                Description += $" себя";


            if (Request.Any())
            {
                embed.WithImageUrl(Request.First().Url)
                     .WithDescription(Description);
            }
            else
                embed.WithDescription("Повторите попытку.");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }


        [Aliases, Commands, Usage, Descriptions]
        public async Task cuddle(SocketUser user) => await GenerateGif(SfwEndpoint.Cuddle, user, "Прижался(ась) к");

        [Aliases, Commands, Usage, Descriptions]
        public async Task feed(SocketUser user) => await GenerateGif(SfwEndpoint.Feed, user, "кормит");

        [Aliases, Commands, Usage, Descriptions]
        public async Task hug(SocketUser user) => await GenerateGif(SfwEndpoint.Hug, user, "обнял(а)");

        [Aliases, Commands, Usage, Descriptions]
        public async Task kiss(SocketUser user) => await GenerateGif(SfwEndpoint.Kiss, user, "поцеловал(а)");

        [Aliases, Commands, Usage, Descriptions]
        public async Task pat(SocketUser user) => await GenerateGif(SfwEndpoint.Pat, user, "погладил(а)");

        [Aliases, Commands, Usage, Descriptions]
        public async Task poke(SocketUser user) => await GenerateGif(SfwEndpoint.Poke, user, "ткнул(а)");

        [Aliases, Commands, Usage, Descriptions]
        public async Task slap(SocketUser user) => await GenerateGif(SfwEndpoint.Slap, user, "дал(а) пощечину");

        [Aliases, Commands, Usage, Descriptions]
        public async Task tickle(SocketUser user) => await GenerateGif(SfwEndpoint.Tickle, user, "щекочет");

        [Aliases, Commands, Usage, Descriptions]
        public async Task baka(SocketUser user) => await GenerateGif(SfwEndpoint.Baka, user, "называет дураком");

        [Aliases, Commands, Usage, Descriptions]
        public async Task nekos() => await GenerateGif(SfwEndpoint.Neko, null, "приносит кошечку");

        [Aliases, Commands, Usage, Descriptions]
        public async Task smug() => await GenerateGif(SfwEndpoint.Smug, null, "самодовольничает");
    }
}
