using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Foundation;

public static partial class LoggerMessages
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "NemesisEuchre starting up")]
    public static partial void LogStartingUp(ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Debug,
        Message = "Persisting completed game with status: {GameStatus}")]
    public static partial void LogPersistingCompletedGame(
        ILogger logger,
        GameStatus gameStatus);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Debug,
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

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Debug,
        Message = "Persisting batch of {BatchSize} completed games")]
    public static partial void LogPersistingBatchedGames(ILogger logger, int batchSize);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Debug,
        Message = "Batch of {BatchSize} games persisted successfully")]
    public static partial void LogBatchGamesPersisted(ILogger logger, int batchSize);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Error,
        Message = "Failed to persist batch of {BatchSize} games. Games will not be saved.")]
    public static partial void LogBatchGamePersistenceFailed(ILogger logger, int batchSize, Exception exception);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Debug,
        Message = "Retrieving {DecisionType} training data for {ActorType} (limit: {Limit}, winningTeamOnly: {WinningTeamOnly})")]
    public static partial void LogRetrievingTrainingData(
        ILogger logger,
        string decisionType,
        string actorType,
        int limit,
        bool winningTeamOnly);

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Error,
        Message = "Failed to retrieve {DecisionType} training data for {ActorType}")]
    public static partial void LogTrainingDataRetrievalFailed(
        ILogger logger,
        string decisionType,
        string actorType,
        Exception exception);
}
