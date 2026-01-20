using Microsoft.Extensions.Logging;

namespace NemesisEuchre.Console;

public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "NemesisEuchre starting up")]
    public static partial void LogStartingUp(ILogger logger);
}
