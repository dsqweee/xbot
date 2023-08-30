using XBOT.Services;
using XBOT.Services.Attribute;
using XBOT.Services.Configuration;

namespace XBOT.Modules.Command
{
    [UserPermission(UserPermission.RolePermission.Iventer)]
    [Name("Iventer"), Summary("Команды управления ивентами")]
    public class IventerModule : ModuleBase<SocketCommandContext>
    {
        //private readonly Db _db;
        private readonly GiveAway_Service _giveaway;

        public IventerModule(/*Db db, */GiveAway_Service giveaway)
        {
            //_db = db;
            _giveaway = giveaway;
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task IventMessage(string JsonText)
        {
            var mes = JsonToEmbed.JsonCheck(JsonText);
            if (mes.Item1 == null)
            {
                mes.Item2 = null;
                mes.Item1.Color = new Color(BotSettings.DiscordColor);
                mes.Item1.WithAuthor("Ошибка!");
                mes.Item1.Description = "Неправильная конвертация в Json.\nИспользуйте [сайт](https://embed.discord-bot.net/), после скопируйте текст справа и вставьте вместо сообщения.";
            }
            await Context.Channel.SendMessageAsync($"{mes.Item2}\n<@{BotSettings.IventerLoverId}>", false, mes.Item1.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task giveawaystart(string Time, byte WinnersCount, [Remainder] string Given)
        {
            using var _db = new Db();
            bool Error = true;
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"🎲 **РОЗЫГРЫШ** 🎲");

            bool Success = TimeSpan.TryParse(Time, out TimeSpan result);
            if (!Success)
                emb.WithDescription("Время введено неверно, возможно вы ввели слишком больше число?\nФормат: 01:00:00 [ч:м:с]\nФормат 2: 07:00:00:00 [д:ч:с:м]");
            else if (result.TotalSeconds < 30 || result.TotalSeconds > 604800)
                emb.WithDescription("Время розыгрыша не может быть меньше 30 секунд, и больше 7 дней!");
            else
            {
                if (_db.GiveAways.Count() <= 5)
                {
                    int maxWinners = 25;
                    if (WinnersCount <= maxWinners)
                    {
                        var TimeToIvent = DateTime.Now.Add(result);
                        var TimeToGo = TimeToIvent - DateTime.Now;
                        var ReactionDice = new Emoji("🎟");
                        emb.WithDescription(_giveaway.GiveawayTextFormat(TimeToGo, Given));
                        var message = await Context.Channel.SendMessageAsync("", false, emb.Build());
                        await message.AddReactionAsync(ReactionDice);
                        var ThisTask = _db.Add(new GiveAways() { Id = message.Id, TextChannelId = Context.Channel.Id, TimesEnd = TimeToIvent, Surpice = Given, WinnerCount = WinnersCount }).Entity;
                        await _db.SaveChangesAsync();
                        _giveaway.Giveaway_List.Add(message.Id, false);
                        await _giveaway.GiveAwayTimer(ThisTask, message);
                        Error = false;
                    }
                    else
                        emb.WithDescription($"Кол-во победителей, не может превышать {maxWinners}!");
                }
                else
                    emb.WithDescription("Нельзя запустить больше 5 конкурсов");
                
            }

            if (Error)
                await Context.Channel.SendMessageAsync($"<@{BotSettings.IventerLoverId}>", false, emb.Build());
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task giveawaystop(ulong MessageId)
        {
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"🎲 **РОЗЫГРЫШ**  🎲");

            if (_giveaway.Giveaway_List.Any(x => x.Key == MessageId))
                emb.WithDescription("Розыгрыша с таким сообщением не существует!");
            else
            {
                _giveaway.Giveaway_List[MessageId] = true;
                emb.WithDescription("Розыгрыш принудительно завершен!");
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }
    }
}
