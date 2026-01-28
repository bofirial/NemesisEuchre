using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Foundation;

public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "NemesisEuchre starting up")]
    public static partial void LogStartingUp(ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Persisting completed game with status: {GameStatus}")]
    public static partial void LogPersistingCompletedGame(
        ILogger logger,
        GameStatus gameStatus);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Game persisted successfully with ID: {GameId}")]
    public static partial void LogGamePersistedSuccessfully(
        ILogger logger,
        int gameId);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Failed to persist completed game. Game results will still be displayed.")]
    public static partial void LogGamePersistenceFailed(
        ILogger logger,
        Exception exception);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "Game {GameNumber} failed")]
    public static partial void LogGameFailed(ILogger logger, int gameNumber, Exception exception);
}
