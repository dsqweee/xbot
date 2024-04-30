using Fergun.Interactive;
using System.Text.RegularExpressions;
using System.Timers;
using XBOT.Services.Configuration;

namespace XBOT.Services;

public class GiftQuestion_Service
{
    private readonly DiscordSocketClient _client;
    private readonly InteractiveService _interactive;
    private readonly Db _db;
    private string Question { get; set; }
    private long QuestionResult { get; set; }
    public GiftQuestion_Service(DiscordSocketClient client, Db db, InteractiveService interactive)
    {
        _client = client;
        _db = db;
        _interactive = interactive;
    }

    internal Task StartGiftQuestion()
    {
        Console.WriteLine("--- StartGiftQuestion START ---");
        TimeSpan time = new TimeSpan(0, 60, 0);
        System.Timers.Timer TaskTime = new(time);
        TaskTime.Elapsed += GiftQuestionActivate;
        TaskTime.Start();
        Console.WriteLine("--- StartGiftQuestion STOP ---");
        return Task.CompletedTask;
    }


    private async void GiftQuestionActivate(object sender, ElapsedEventArgs e)
    {
        //using var _db = new Db();

        var Timer = sender as System.Timers.Timer;
        var RandomMinutes = new Random().Next(60, 180);
        Timer.Interval = new TimeSpan(0, RandomMinutes, 0).TotalMilliseconds;

        var channel = _client.GetChannel(BotSettings.DefaultChannel) as SocketTextChannel;

        var emb = new EmbedBuilder()
            .WithColor(BotSettings.DiscordColor)
            .WithAuthor("Викторина призов!");

        CalculateMath();

        var TimeActive = TimeSpan.FromSeconds(60);
        emb.WithDescription($"Вопрос: сколько будет {Question}?")
           .WithFooter($"На ответ дается {TimeActive.TotalSeconds} секунд.");

        var message = await channel.SendMessageAsync("",false, emb.Build());

        var messageResult = await _interactive.NextMessageAsync((message) =>
        {
            string sanitizedInput = Regex.Replace(message.Content, @"\s+", "");
            if (!Regex.IsMatch(sanitizedInput, @"^-?\d+$"))
                return false;
            if (!long.TryParse(sanitizedInput, out long numberAnswer))
                return false;

            if (QuestionResult != numberAnswer)
                return false;

            return true;

        }, timeout: TimeActive);

        //var MessageResult = await _interactive.NextMessageAsync(x => long.TryParse(x.Content, out long result) == QuestionResult, timeout: TimeActive); // не активировались условия

        if (messageResult.IsTimeout)
        {
            emb.WithDescription($"Вопрос: сколько будет {Question}?\nОтвет: {QuestionResult}")
                .WithFooter("Ни кто не успел ответить правильно.");
            await message.ModifyAsync(x => x.Embed = emb.Build());
            await Task.Delay(5000);
            await message.DeleteAsync();
        }
        else if (messageResult.IsSuccess)
        {
            var userId = messageResult.Value.Author.Id;
            emb.WithAuthor("Победитель викторины!");
            var userDb = await _db.GetUser(userId);
            var randomMoney = (uint)new Random().Next(0, 500);
            userDb.money += randomMoney;
            _db.Update(userDb);
            await _db.SaveChangesAsync();
            emb.WithDescription($"Победитель: <@{userId}>\n" +
                                $"{QuestionResult} = {Question}\n" +
                                $"Приз: {randomMoney} coins")
               .WithFooter("Не пропусти следующую викторину, и забери приз!");
            QuestionResult = 0;
            await message.DeleteAsync();
            await channel.SendMessageAsync("", false, emb.Build());
        }
    }

    private void CalculateMath()
    {
        var random = new Random();

        string operations = "+-*/";

        long result = 0;
        long randomNumber1 = random.Next(-100000, 100000);
        long randomNumber2 = random.Next(-100000, 100000);

        char symbol = operations[random.Next(operations.Length)];
        switch (symbol)
        {
            case '+':
                result = randomNumber1 + randomNumber2;
                break;
            case '-':
                result = randomNumber1 - randomNumber2;
                break;
            case '*':
                result = randomNumber1 * randomNumber2;
                break;
            case '/':
                result = randomNumber1 / randomNumber2;
                break;
        }

        QuestionResult = result;
        Question = $"{randomNumber1} {symbol} {randomNumber2}";
    }
}
