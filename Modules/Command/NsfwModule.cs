//using Nekos.Net.V3;
//using Nekos.Net.V3.Endpoints;
//using XBOT.Services.Attribute;
//using XBOT.Services.Configuration;

//namespace XBOT.Modules
//{
//    [Name("NsfwGif")]
//    [Summary("18+ гифки")]
//    [RequireBotPermission(ChannelPermission.SendMessages)]
//    [RequireBotPermission(ChannelPermission.EmbedLinks)]
//    public class NSFW : ModuleBase<SocketCommandContext>
//    {
//        private readonly NekosV3Client NekoClient = new();

//        private static EmbedBuilder CheckNSFW(SocketCommandContext Context)
//        {
//            var emb = new EmbedBuilder();
//            var ThisTextChannel = Context.Message.Channel as SocketTextChannel;
//            if (!ThisTextChannel.IsNsfw)
//            {
//                SocketTextChannel NsfwChannel = null;
//                foreach (var Channel in Context.Guild.TextChannels.Where(x => x.IsNsfw))
//                {
//                    var Permission = Channel.GetPermissionOverwrite(Context.Guild.EveryoneRole);
//                    if(Permission.Value.SendMessages != PermValue.Deny && 
//                       Permission.Value.ViewChannel  != PermValue.Deny)
//                    {
//                        NsfwChannel = Channel;
//                        break;
//                    }
//                }

//                emb.WithDescription("Данный канал не является NSFW для использования этой команды.\n");
//                if (NsfwChannel == null)
//                    emb.Description += "Попросите админа создать канал с параметром NSFW.";
//                else
//                    emb.Description += $"Используйте эту команду в {NsfwChannel.Mention}";
//            }
//            return emb;
//        }

//        private async Task GenerateImage(NsfwGifEndpoint Type)
//        {
//            var emb = CheckNSFW(Context)
//                .WithColor(BotSettings.DiscordColor)
//                .WithAuthor($"{Type} GIF 18+");

//            if (emb.Description == null)
//            {
//                int CountGenerate = 0;
//                while (true)
//                {
//                    if (CountGenerate == 10)
//                        break;

//                    var Request = await NekoClient.WithNsfwGifEndpoint(Type).GetAsync();
//                    if (Request.Status.IsSuccess)
//                    {
//                        emb.WithImageUrl(Request.Data.Response.Url);
//                        break;
//                    }
//                    else
//                        emb.WithDescription("Команда временно не работает!");

//                    CountGenerate++;
//                }
//            }
//            await Context.Channel.SendMessageAsync("", false, emb.Build());
//        }

//        [Aliases, Commands, Usage, Descriptions]
//        public async Task Yuri() => await GenerateImage(NsfwGifEndpoint.Yuri); 

//        [Aliases, Commands, Usage, Descriptions]
//        public async Task Anal() => await GenerateImage(NsfwGifEndpoint.Anal);

//        [Aliases, Commands, Usage, Descriptions]
//        public async Task Blowjob() => await GenerateImage(NsfwGifEndpoint.Blow_Job);

//        [Aliases, Commands, Usage, Descriptions]
//        public async Task Classic() => await GenerateImage(NsfwGifEndpoint.Classic);

//        [Aliases, Commands, Usage, Descriptions]
//        public async Task Cum() => await GenerateImage(NsfwGifEndpoint.Cum);

//        [Aliases, Commands, Usage, Descriptions]
//        public async Task Feet() => await GenerateImage(NsfwGifEndpoint.Feet);

//        [Aliases, Commands, Usage, Descriptions]
//        public async Task Kuni() => await GenerateImage(NsfwGifEndpoint.Kuni);

//        [Aliases, Commands, Usage, Descriptions]
//        public async Task Neko() => await GenerateImage(NsfwGifEndpoint.Neko);

//        [Aliases, Commands, Usage, Descriptions]
//        public async Task Pussy() => await GenerateImage(NsfwGifEndpoint.Pussy);

//        [Aliases, Commands, Usage, Descriptions]
//        public async Task Pwank() => await GenerateImage(NsfwGifEndpoint.Pussy_Wank);

//        [Aliases, Commands, Usage, Descriptions]
//        public async Task Solo() => await GenerateImage(NsfwGifEndpoint.Girls_Solo);

//        [Aliases, Commands, Usage, Descriptions]
//        public async Task Spank() => await GenerateImage(NsfwGifEndpoint.Spank);

//        [Aliases, Commands, Usage, Descriptions]
//        public async Task Tits() => await GenerateImage(NsfwGifEndpoint.Tits);

//        [Aliases, Commands, Usage, Descriptions]
//        public async Task Yiff() => await GenerateImage(NsfwGifEndpoint.Yiff);
//    }
//}
