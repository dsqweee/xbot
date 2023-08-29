namespace XBOT.Services.Configuration;

public class BotSettings
{
    public const string connectionStringDbPath = "Data Source=XBOT.db";
    public const string tokenPath = "_config.yml";
    public const string commandListPath = "CommandsList.json";
    public const string errorListPath = "ErrorsList.json";

    public const string PrivateCategoryName = "━━ • ПРИВАТКИ • ━━";
    public const string PrivateTextName = "━━ • НАСТРОЙКИ • ━━";
    public const string PrivateVoiceName = "➕Создать канал";

    public const string PayURL = "https://bill.discord-bot.net/botpay/get?token=5e43cb4e72a78";
    public const string PayUserURL = "https://bill.discord-bot.net/botpay/user?owner_id=551373471536513024&discord_id={0}&payment_method=qiwi&amount={1}";

    public static Color DiscordColor = new Color(232, 189, 94);
    public static Color DiscordColorError = new Color(255, 76, 86);

    public const uint CoinsMaxUser = 100000;

    public static string GetBuild()
    {
        DateTime creation = File.GetLastWriteTime("XBOT.dll");
        return creation.ToString("dd.MM.yy HH:mm");
    }

    public const ulong xId = 1018972629534773310;
    public const ulong DefaultChannel = 1128457837346041858; // 1146121657853956168

    public const ulong IventerLoverId = 1128877663638003733;
}
