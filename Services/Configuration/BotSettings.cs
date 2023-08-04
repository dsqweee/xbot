namespace XBOT.Services.Configuration
{
    public class BotSettings
    {
        public static string connectionStringDb = "Data Source=XBOT.db";
        public static string TokenPath = "_config.yml";
        public static string CommandConfig = "CommandsList.json";

        public static string PrivateCategoryName = "━━ • ПРИВАТКИ • ━━";
        public static string PrivateTextName = "━━ • НАСТРОЙКИ • ━━";
        public static string PrivateVoiceName = "➕Создать канал";

        public static Color DiscordColor = new Color(232, 189, 94);
        public static Color DiscordColorError = new Color(255, 76, 86);

        public static uint CoinsMaxUser = 100000;

        public static ulong xId = 1018972629534773310;
        public static ulong DefaultChannel = 1128457837346041858;
    }
}
