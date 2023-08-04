using Discord.Rest;
using XBOT.Services.Attribute;
using XBOT.Services.Configuration;

namespace XBOT.Modules.Command
{
    [UserPermission(UserPermission.RolePermission.Iventer)]
    [Name("Iventer"), Summary("Команды управления ивентами")]
    public class IventerModule : ModuleBase<SocketCommandContext>
    {

        static readonly Dictionary<ulong, bool> EndList = new();


        static string TextFormat(TimeSpan TimeToGo, string Give)
        {
            var text = $"Розыгрыш ***{Give} ***\nНажмите на эмодзи 🎟 чтобы учавствовать!";
            if (TimeToGo.TotalSeconds > 86400)
                text += $"\nОсталось: {TimeToGo.Days} дней и {TimeToGo.Hours} часов";
            else if (TimeToGo.TotalSeconds > 3600)
                text += $"\nОсталось: {TimeToGo.Hours} часов и {TimeToGo.Minutes} минут";
            if (TimeToGo.TotalSeconds > 60)
                text += $"\nОсталось: {TimeToGo.Minutes} минут и {TimeToGo.Seconds} секунд";
            else
                text += $"\nОсталось: {TimeToGo.Seconds} секунд";
            return text;
        }


        public static async Task GiveAwayTimer(GiveAways ThisTask, RestUserMessage message)
        {
            using (db _db = new())
            {
                var emb = new EmbedBuilder()
                    .WithColor(BotSettings.DiscordColor)
                    .WithAuthor($"🎲 **РОЗЫГРЫШ** 🎲");

                var TimeToGo = ThisTask.TimesEnd - DateTime.Now;
                string Text = TextFormat(TimeToGo, ThisTask.Surpice);
                var Task = EndList.FirstOrDefault(x => x.Key == ThisTask.Id);

                if (!EndList.Any(x => x.Key == ThisTask.Id))
                {
                    EndList.Add(ThisTask.Id, false);
                }

                while (ThisTask.TimesEnd > DateTime.Now)
                {
                    TimeToGo = ThisTask.TimesEnd - DateTime.Now;

                    if (TimeToGo.TotalSeconds % 5 == 0)
                    {
                        Text = TextFormat(TimeToGo, ThisTask.Surpice);
                        emb.WithDescription(Text);
                        await message.ModifyAsync(x => x.Embed = emb.Build());
                    }
                    if (Task.Value)
                        break;
                }

                string Winner = string.Empty;
                if (Task.Value)
                    emb.WithDescription("Розыгрыш завершен администрацией!");
                else
                {
                    var users = await message.GetReactionUsersAsync(new Emoji("🎟"), int.MaxValue).FlattenAsync();
                    var Allusers = users.Where(x => !x.IsBot).ToList();

                    if (Allusers.Any())
                    {
                        if (ThisTask.WinnerCount > 1)
                        {
                            List<IUser> WIN = new();
                            for (int i = 0; i < ThisTask.WinnerCount; i++)
                            {
                                var User = Allusers.ElementAt(new Random().Next(Allusers.Count + 1));
                                WIN.Add(User);
                                Allusers.Remove(User);
                                Winner += $"<@{User.Id}>,";
                            }
                            emb.WithDescription($"***Поздравляю***! Победители: \n{Winner} \nВыигрыш(и): {ThisTask.Surpice}!");
                        }
                        else
                        {
                            IUser WIN = Allusers.ElementAt(new Random().Next(Allusers.Count + 1));
                            Winner = WIN.Mention;
                            emb.WithDescription($"***Поздравляю***! {WIN.Mention} выиграл {ThisTask.Surpice}!");
                        }

                        await message.AddReactionAsync(new Emoji("🏆"));
                    }
                    else
                        emb.WithDescription("Недостаточно участников для розыгрыша!");
                }
                _db.GiveAways.Remove(ThisTask);
                await _db.SaveChangesAsync();
                EndList.Remove(Task.Key);

                await message.ModifyAsync(x => x.Embed = emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task giveawaystart(string Time, byte WinnersCount, [Remainder] string Given)
        {
            using (db _db = new())
            {
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
                    int maxWinners = 25;
                    if (WinnersCount <= maxWinners)
                    {
                        var TimeToIvent = DateTime.Now.Add(result);
                        var TimeToGo = TimeToIvent - DateTime.Now;
                        var ReactionDice = new Emoji("🎟");
                        emb.WithDescription(TextFormat(TimeToGo, Given));
                        var message = await Context.Channel.SendMessageAsync("", false, emb.Build());
                        await message.AddReactionAsync(ReactionDice);
                        var ThisTask = _db.Add(new GiveAways() { Id = message.Id, TextChannelId = Context.Channel.Id, TimesEnd = TimeToIvent, Surpice = Given, WinnerCount = WinnersCount }).Entity;
                        await _db.SaveChangesAsync();
                        EndList.Add(message.Id, false);
                        await GiveAwayTimer(ThisTask, message);
                        Error = false;
                    }
                    else
                        emb.WithDescription($"Кол-во победителей, не может превышать {maxWinners}!");
                }

                if (Error)
                    await Context.Channel.SendMessageAsync("", false, emb.Build());
            }
        }

        [Aliases, Commands, Usage, Descriptions]
        public async Task giveawaystop(ulong MessageId)
        {
            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor($"🎲 **РОЗЫГРЫШ**  🎲");

            if (EndList.Any(x => x.Key == MessageId))
                emb.WithDescription("Розыгрыша с таким сообщением не существует!");
            else
            {
                EndList[MessageId] = true;
                emb.WithDescription("Розыгрыш принудительно завершен!");
            }
            await Context.Channel.SendMessageAsync("", false, emb.Build());
        }
    }
}
