namespace XBOT.Services.Configuration;

public class BotSettings
{
    public static string connectionStringDb = "Data Source=XBOT.db";
    public static string TokenPath = "_config.yml";
    public static string CommandConfig = "CommandsList.json";
    public static string ErrorConfig = "ErrorsList.json";

    public static string PrivateCategoryName = "━━ • ПРИВАТКИ • ━━";
    public static string PrivateTextName = "━━ • НАСТРОЙКИ • ━━";
    public static string PrivateVoiceName = "➕Создать канал";

    public static string PayURL = "https://bill.discord-bot.net/botpay/get?token=5e43cb4e72a78";
    public static string PayUserURL = "https://bill.discord-bot.net/botpay/user?owner_id=551373471536513024&discord_id={0}&payment_method=qiwi&amount={1}";

    public static Color DiscordColor = new Color(232, 189, 94);
    public static Color DiscordColorError = new Color(255, 76, 86);

    public static uint CoinsMaxUser = 100000;

    public static ulong xId = 1018972629534773310;
    public static ulong DefaultChannel = 1128457837346041858;

    public static ulong IventerLoverId = 1128877663638003733;
}
