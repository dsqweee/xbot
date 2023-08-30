using Discord.Rest;
using XBOT.Services.Configuration;

namespace XBOT.Services;

public class GiveAway_Service
{
    //private readonly Db _db;

    //public GiveAway_Service(Db db)
    //{
    //    _db = db;
    //}

    public Dictionary<ulong, bool> Giveaway_List = new();


    public string GiveawayTextFormat(TimeSpan TimeToGo, string Give)
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


    public async Task GiveAwayTimer(GiveAways ThisTask, RestUserMessage message)
    {
        using var _db = new Db();
        var emb = new EmbedBuilder()
            .WithColor(BotSettings.DiscordColor)
            .WithAuthor($"🎲 **РОЗЫГРЫШ** 🎲");

        var TimeToGo = ThisTask.TimesEnd - DateTime.Now;
        string Text = GiveawayTextFormat(TimeToGo, ThisTask.Surpice);
        var Task = Giveaway_List.FirstOrDefault(x => x.Key == ThisTask.Id);

        if (!Giveaway_List.Any(x => x.Key == ThisTask.Id))
        {
            Giveaway_List.Add(ThisTask.Id, false);
        }

        while (ThisTask.TimesEnd > DateTime.Now)
        {
            TimeToGo = ThisTask.TimesEnd - DateTime.Now;

            if (TimeToGo.TotalSeconds % 5 == 0)
            {
                Text = GiveawayTextFormat(TimeToGo, ThisTask.Surpice);
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
        Giveaway_List.Remove(Task.Key);

        await message.ModifyAsync(x => x.Embed = emb.Build());
    }
}
