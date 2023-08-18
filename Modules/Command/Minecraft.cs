using Fergun.Interactive;
using Microsoft.EntityFrameworkCore;
using XBOT.Services;
using XBOT.Services.Attribute;
using XBOT.Services.Configuration;

namespace XBOT.Modules.Command
{
    [RequireContext(ContextType.DM)]
    [RequireOwner]
    public class Minecraft : ModuleBase<SocketCommandContext>
    {
        private readonly InteractiveService _interactive;
        private readonly Minecraft_Service _minecraft;
        private readonly Db _db;


        public Minecraft(InteractiveService interactive, Minecraft_Service minecraft, Db db)
        {
            _interactive = interactive;
            _minecraft = minecraft;
            _db = db;
        }
        
        
        [Aliases, Commands, Usage, Descriptions]
        [RequireOwner]
        public async Task SetMinecraftData(string key,ushort port,string ip)
        {
            var settings = _db.Settings.FirstOrDefault();
            settings.minecraft_IP = ip;
            settings.minecraft_port = port;
            settings.minecraft_Key = key;
            _db.Settings.Update(settings);
            await _db.SaveChangesAsync();
            await Context.Channel.SendMessageAsync("Данные успешно внесены!");
        }

        [Aliases, Commands, Usage, Descriptions]
        [RequireOwner]
        public async Task Setlicence(string minecraftName)
        {
            await _minecraft.UserLicenceActivate(minecraftName);
            await Context.Channel.SendMessageAsync("Лицензия активирована");
        }

        [Aliases, Commands, Usage, Descriptions]
        [RequireOwner]
        public async Task Dellicence(string minecraftName)
        {
            await _minecraft.UserLicenceDeActivate(minecraftName);
            await Context.Channel.SendMessageAsync("Лицензия деактивирована");
        }

        [Command("test"), RequireOwner]
        public async Task get()
        {
            var user = _db.User.FirstOrDefault(x => x.Id == Context.User.Id);
            var moneybefore = user.money;
            user.money++;
            await _db.SaveChangesAsync();
            await Context.Channel.SendMessageAsync($"{moneybefore} -> {user.money}");
        }


        [Command("GetMinecraft")]
        [MinecraftPermission]
        public async Task GetMinecraft()
        {
            var emb = new EmbedBuilder().WithColor(BotSettings.DiscordColor);

            var UserDb = _db.User.Include(x => x.MinecraftAccount).FirstOrDefault(x => x.Id == Context.User.Id);
            byte DefaultPrice = 100;
            string buynow = $"[пополнить баланс]({String.Format(BotSettings.PayUserURL, Context.User.Id, DefaultPrice)})";

            if (UserDb.MinecraftAccountId == 0)
            {
                emb.WithAuthor($"Minecraft проходка отсутствует");
                emb.WithDescription("У вас еще нету проходки на наш сервер?\n-Я в шоке...\n\n" +
                                    "Для начала нужен твой ник Minecraft, чтобы выдать ему доступ\n\n" +
                                    "Напиши имя аккаунта без ошибок, в будущем его будет нельзя поменять:")
                   .WithFooter("Если вводишь имя, а ничего не происходит, напиши команду еще раз!");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
                var result = await _interactive.NextMessageAsync(x => x.Channel.Id == Context.Channel.Id, timeout: TimeSpan.FromDays(31));

                if (result.Status == InteractiveStatus.Success)
                {
                    var mineacc = new User_MinecraftAccount { MinecraftName = result.Value.Content, UserId = UserDb.Id };
                    _db.User_MinecraftAccount.Add(mineacc);
                    await _db.SaveChangesAsync();
                    var Settings = _db.Settings.FirstOrDefault();
                    emb.WithDescription($"Привет: {mineacc.MinecraftName}\n\n" +
                                        "Вы можете купить проходку прямо сейчас, и начать играть вместе с нашими игроками.\n" +
                                        "Покупая проходку за 75 рублей, вы получаете доступ к серверу на 1 месяц, который можете продлить в любой момент, произведя оплату еще раз.\n" +
                                        "Покупая проходку, вы подтверждаете что ознакомились с правилами в канале <#1138035687849988156>\n\n" +
                                        $"Ну что, готов к приключениям? Кликай, и покупай проходку -> **{buynow}**")
                       .WithFooter($"После оплаты, статус проходки появится в {Settings.Prefix}userinfo, или введя еще раз эту команду.");
                    await Context.Channel.SendMessageAsync("", false, emb.Build());
                }
            }
            else
            {
                if (UserDb.MinecraftAccount.LicenceTo > DateTime.Now)
                {
                    emb.WithDescription($"Привет: {UserDb.MinecraftAccount.MinecraftName}\n" +
                                        $"Лицензия до: {UserDb.MinecraftAccount.LicenceTo.ToString("dd.MM.yy HH:mm")}\n\n" +
                                        $"Хочешь продлить? Кликай -> **{buynow}**");
                }
                else
                {
                    emb.WithDescription($"Привет: {UserDb.MinecraftAccount.MinecraftName}\n" +
                                        $"Твоя лицензия закончилась! Хочешь продлить? Кликай -> **{buynow}**");
                }
                emb.WithFooter("Лицензия активируется в течении 1 минуты!");
                await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }
    }
}
