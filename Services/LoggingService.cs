using Microsoft.Extensions.Logging;

namespace XBOT.Services;

public class LoggingService
{
    public static Task OnLogAsync(ILogger logger, LogMessage msg)
    {
        switch (msg.Severity)
        {
            case LogSeverity.Info:
            case LogSeverity.Verbose:
                logger.LogInformation(msg.ToString());
                break;
            case LogSeverity.Warning:
                logger.LogWarning(msg.ToString());
                break;

            case LogSeverity.Error:
                logger.LogError(msg.ToString());
                break;

            case LogSeverity.Critical:
                logger.LogCritical(msg.ToString());
                break;
        }
        return Task.CompletedTask;
    }
}
