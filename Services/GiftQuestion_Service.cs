using System.Timers;
using XBOT.Services.Configuration;

namespace XBOT.Services
{
    public class GiftQuestion_Service
    {
        private readonly DiscordSocketClient _client;

        private string question { get; set; }
        public long QuestionResult { get; set; }
        public GiftQuestion_Service(DiscordSocketClient client)
        {
            _client = client;
        }

        internal Task StartGiftQuestion()
        {
            TimeSpan time = new TimeSpan(0, 60, 0);
            System.Timers.Timer TaskTime = new(time);
            TaskTime.Elapsed += GiftQuestionActivate;
            TaskTime.Start();
            return Task.CompletedTask;
        }


        public async void GiftQuestionActivate(object sender, ElapsedEventArgs e)
        {
            var channel = _client.GetChannel(BotSettings.DefaultChannel) as SocketTextChannel;

            var emb = new EmbedBuilder()
                .WithColor(BotSettings.DiscordColor)
                .WithAuthor("Викторина призов!");

            var Random = new Random();

            string operations = "+-*/";

            long result = 0;
            long RandomNumber1 = Random.Next(-100000, 100000);
            long RandomNumber2 = Random.Next(-100000, 100000);

            char symbol = operations[Random.Next(operations.Length)];
            switch (symbol)
            {
                case '+':
                    result = RandomNumber1 + RandomNumber2;
                    break;
                case '-':
                    result = RandomNumber1 - RandomNumber2;
                    break;
                case '*':
                    result = RandomNumber1 * RandomNumber2;
                    break;
                case '/':
                    result = RandomNumber1 / RandomNumber2;
                    break;
            }
            QuestionResult = result;
            var Timer = sender as System.Timers.Timer;
            var RandomMinutes = new Random().Next(60, 180);
            Timer.Interval = new TimeSpan(0, RandomMinutes, 0).TotalMilliseconds;

            question = $"{RandomNumber1} {symbol} {RandomNumber2}";
            emb.WithDescription($"Вопрос: сколько будет {question}?")
               .WithFooter("Для ответа просто напишите значение!");
            await channel.SendMessageAsync("", false, emb.Build());
        }


        public async Task QuestionAnswerScan(SocketGuildUser user, long Answer)
        {
            using (var _db = new db())
            {
                if (QuestionResult != Answer)
                    return;

                var emb = new EmbedBuilder()
                    .WithColor(BotSettings.DiscordColor)
                    .WithAuthor("Победитель викторины!");
                var userDb = await _db.GetUser(user.Id);
                var randomMoney = (uint)new Random().Next(0, 500);
                userDb.money += randomMoney;
                await _db.SaveChangesAsync();
                emb.WithDescription($"Победитель: {user.Mention}\n" +
                                    $"{QuestionResult} = {question}\n" +
                                    $"Приз: {randomMoney} coins")
                   .WithFooter("Не пропусти следующую викторину, и забери приз!");
                QuestionResult = 0;
                var Channel = _client.GetChannel(BotSettings.DefaultChannel) as SocketTextChannel;
                await Channel.SendMessageAsync("", false, emb.Build());
            }
        }
    }
}
