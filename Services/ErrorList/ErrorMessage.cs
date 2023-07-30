using XBOT.Services.Configuration;

namespace XBOT.Services.Attribute.ErrorList
{
    public class ErrorMessage
    {
        public static EmbedBuilder GetError(string error, string prefix, CommandInfo command = null)
        {
            using (db _db = new())
            {
                var emb = new EmbedBuilder()
                    .WithColor(BotSettings.DiscordColor)
                    .WithAuthor("Ошибка!");

                string text = string.Empty;
                var SetError = Initiliaze.Load(error);

                if (SetError?.Rus == error)
                {
                    emb.WithDescription(error);
                    return emb;
                }

                if (SetError.HelpCommand != "true")
                {
                    emb.WithDescription($"{SetError.Rus}");
                    return emb;
                }


                foreach (var Parameter in command.Parameters)
                {
                    if (Parameter.IsOptional)
                        text += $"[{Parameter}/может быть пустым]";
                    else if (Parameter.IsRemainder)
                        text += $"[{Parameter}/поддерживает предложения]";
                    else
                        text += $"[{Parameter}] ";
                }

                emb.WithDescription($"Описание ошибки: " + SetError.Rus)
                   .AddField($"Описание команды: ", $"{command.Summary ?? "отсутствует"}", true)
                   .AddField("Пример команды:", $"{prefix}{command.Name} {text}");

                return emb;
            }
        }
    }
}
