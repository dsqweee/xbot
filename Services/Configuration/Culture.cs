using System.Globalization;

namespace XBOT.Services.Configuration;

public class Culture
{
    public Culture(string Culture = "ru-RU")
    {
        var CurrentCulture = new CultureInfo(Culture);
        Thread.CurrentThread.CurrentCulture = CurrentCulture;
        Thread.CurrentThread.CurrentUICulture = CurrentCulture;
        CultureInfo.DefaultThreadCurrentCulture = CurrentCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CurrentCulture;
    }
}
