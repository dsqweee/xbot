using Microsoft.Extensions.Logging;

namespace XBOT.Services;

public class LoggingService
{
    private static string LogDirectory => Path.Combine(AppContext.BaseDirectory, "logs");
    private static string LogFile => Path.Combine(LogDirectory, $"{DateTime.Now:dd-MM-yy}.txt");

    public async static Task OnLogAsync(/*ILogger logger, */LogMessage msg)
    {
        //switch (msg.Severity)
        //{
        //    case LogSeverity.Info:
        //    case LogSeverity.Verbose:
        //        logger.LogInformation(msg.ToString());
        //        break;
        //    case LogSeverity.Warning:
        //        logger.LogWarning(msg.ToString());
        //        break;

        //    case LogSeverity.Error:
        //        logger.LogError(msg.ToString());
        //        break;

        //    case LogSeverity.Critical:
        //        logger.LogCritical(msg.ToString());
        //        break;
        //}

        if (!Directory.Exists(LogDirectory))
            Directory.CreateDirectory(LogDirectory);
        if (!File.Exists(LogFile))
            File.Create(LogFile).Dispose();

        string logText = $"{DateTime.Now:dd.MM.yy HH:mm:ss} [{msg.Severity}]: {msg.Exception?.ToString() ?? msg.Message}";
        Console.WriteLine(logText);
        await File.AppendAllTextAsync(LogFile, logText + "\n");
    }
}
