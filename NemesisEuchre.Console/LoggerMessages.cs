using Microsoft.Extensions.Logging;

namespace NemesisEuchre.Console;

public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Application shutting down")]
    public static partial void LogApplicationShuttingDown(ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "NemesisEuchre starting up")]
    public static partial void LogStartingUp(ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Configuration loaded successfully")]
    public static partial void LogConfigurationLoaded(ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "An error occurred during application execution")]
    public static partial void LogApplicationError(ILogger logger, Exception exception);
}
