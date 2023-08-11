using Newtonsoft.Json;
using System.Collections.Concurrent;
using XBOT.Services.Configuration;

namespace XBOT.Services.Attribute.ErrorList;

public class Initiliaze
{
    private static readonly Lazy<ConcurrentDictionary<string, Errors>> _commandData = new Lazy<ConcurrentDictionary<string, Errors>>(() =>
    {
        var jsonData = File.ReadAllText(BotSettings.ErrorConfig);
        return JsonConvert.DeserializeObject<ConcurrentDictionary<string, Errors>>(jsonData);
    });

    public static Errors Load(string key)
    {
        if (_commandData.Value.TryGetValue(key, out var toReturn))
        {
            return toReturn;
        }

        return new Errors { Rus = key, HelpCommand = key };
    }

    public class Errors
    {
        public string Rus { get; set; }
        public string HelpCommand { get; set; }
    }
}
