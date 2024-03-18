using Discord.Rest;
using Microsoft.EntityFrameworkCore;
using XBOT.Services.Configuration;

namespace XBOT.Services;

public class GiveAway_Service
{
    private readonly Db _db;
    private readonly DiscordSocketClient _client;
    private readonly EmbedBuilder embed = new EmbedBuilder().WithColor(BotSettings.DiscordColor).WithAuthor($"🎲 **РОЗЫГРЫШ** 🎲");

    public GiveAway_Service(Db db, DiscordSocketClient client)
    {
        _db = db;
        _client = client;
    }


    public string GiveawayTextFormat(TimeSpan TimeToGo, string Give)
    {
        var text = $"Розыгрыш ***{Give} ***\nНажмите на эмодзи 🎟 чтобы учавствовать!";
        if (TimeToGo.TotalSeconds >= 86400)
            text += $"\nОсталось: {TimeToGo.Days} дней и {TimeToGo.Hours} часов";
        else if (TimeToGo.TotalSeconds > 3600)
            text += $"\nОсталось: {TimeToGo.Hours} часов и {TimeToGo.Minutes} минут";
        else if (TimeToGo.TotalSeconds == 3600)
            text += $"\nОсталось: {TimeToGo.Hours} часов";
        else if (TimeToGo.TotalSeconds > 60)
            text += $"\nОсталось: {TimeToGo.Minutes} минут и {TimeToGo.Seconds} секунд";
        else if (TimeToGo.TotalSeconds == 60)
            text += $"\nОсталось: {TimeToGo.Minutes} минут";
        else
            text += $"\nОсталось: {TimeToGo.Seconds} секунд";
        return text;
    }

    internal Task StartGiveAwayTimer(GiveAways ThisTask, RestUserMessage message)
    {
        var time = new TimeSpan(0, 0, 5);
        System.Timers.Timer TaskTime = new(time);
        TaskTime.Elapsed += (s, e) => GiveAwayTimer(ThisTask,message, s);
        TaskTime.Start();
        return Task.CompletedTask;
    }

    private async void GiveAwayTimer(GiveAways ThisTask, RestUserMessage message, object timer)
    {
        //using var _db = new Db();
        bool timeOut = false;

        var Time = ThisTask.TimesEnd - DateTime.Now;
        if (Time.TotalSeconds <= 0)
            timeOut = true;

        if (timeOut || ThisTask.IsCanceled)
        {
            (timer as System.Timers.Timer).Dispose();
            await GiveAwayResult(ThisTask, message);
        }
        else
        {
            var emb = embed;

            var TimeToGo = ThisTask.TimesEnd - DateTime.Now;
            string Text = GiveawayTextFormat(TimeToGo, ThisTask.Surpice);

            ThisTask = await _db.GiveAways.FirstOrDefaultAsync(x => x.Id == ThisTask.Id);
            Text = GiveawayTextFormat(TimeToGo, ThisTask.Surpice);
            emb.WithDescription(Text);
            await message.ModifyAsync(x => x.Embed = emb.Build());
        }
    }

    private async Task GiveAwayResult(GiveAways ThisTask, RestUserMessage message)
    {
        var emb = embed;
        string Winner = string.Empty;
        if (ThisTask.IsCanceled)
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

        await message.ModifyAsync(x => x.Embed = emb.Build());
    }

    public async Task GiveAwayScan()
    {
        var Guild = _client.Guilds.First();
        Console.WriteLine(Guild.Id + " - guild");
        foreach (var Give in _db.GiveAways)
        {
            var textChannel = Guild.GetTextChannel(Give.TextChannelId);
            if (textChannel != null)
            {
                var message = await textChannel.GetMessageAsync(Give.Id) as RestUserMessage;
                if (message != null)
                {
                    await StartGiveAwayTimer(Give, message);
                }
                else
                    _db.GiveAways.Remove(Give);
            }
            else
                _db.GiveAways.Remove(Give);
        }
        Console.WriteLine(_db.GiveAways.Count());
        await _db.SaveChangesAsync();
    }
}