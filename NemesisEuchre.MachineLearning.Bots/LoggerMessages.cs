using Microsoft.Extensions.Logging;

namespace NemesisEuchre.MachineLearning.Bots;

public static partial class LoggerMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "Model not found for {DecisionType}, will fall back to random selection")]
    public static partial void LogModelNotFound(ILogger logger, string decisionType, Exception exception);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Failed to load model for {DecisionType}, will fall back to random selection")]
    public static partial void LogModelLoadFailed(ILogger logger, string decisionType, Exception exception);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Error predicting CallTrump decision, falling back to random")]
    public static partial void LogCallTrumpPredictionError(ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Error predicting DiscardCard decision, falling back to random")]
    public static partial void LogDiscardCardPredictionError(ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "Error predicting PlayCard decision, falling back to random")]
    public static partial void LogPlayCardPredictionError(ILogger logger, Exception exception);
}
