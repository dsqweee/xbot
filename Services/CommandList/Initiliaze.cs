using Newtonsoft.Json;
using XBOT.Services.Configuration;

namespace XBOT.Services.Attribute.CommandList;

public class Initiliaze
{
    private static readonly Dictionary<string, Commands> _commandData = JsonConvert.DeserializeObject<Dictionary<string, Commands>>(File.ReadAllText(BotSettings.commandListPath));
    public static Commands Load(string key)
    {
        if (!_commandData.TryGetValue(key, out var toReturn))
        {
            toReturn = new Commands
            {
                Category = "",
                MinDesc = key,
                Desc = key,
                Usage = new[] { key, key }
            };
        }

        else if (string.IsNullOrWhiteSpace(toReturn.MinDesc))
            toReturn.MinDesc = toReturn.Desc;

        if (string.IsNullOrEmpty(toReturn.Usage[0]) && string.IsNullOrEmpty(toReturn.Usage[1]))
            toReturn.Usage = new[] { key, key };

        if (!ListCommand.Any(x=>x.Usage[1] == toReturn.Usage[1]))
            ListCommand.Add(toReturn);
        return toReturn;
    }

    public static List<Commands> ListCommand = new();

    public class Commands
    {
        public string Category { get; set; }
        public string MinDesc { get; set; }
        public string Desc { get; set; }
        public string[] Usage { get; set; }
    }
}
