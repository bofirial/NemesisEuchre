using Microsoft.Extensions.Logging;

namespace NemesisEuchre.MachineLearning;

public static partial class LoggerMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Starting training for {ModelType} model")]
    public static partial void LogStartingTraining(ILogger logger, string modelType);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Data split complete: {TrainCount} training, {ValidationCount} validation, {TestCount} test samples")]
    public static partial void LogDataSplitComplete(
        ILogger logger,
        int trainCount,
        int validationCount,
        int testCount);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Training model with {Iterations} iterations...")]
    public static partial void LogTrainingModel(ILogger logger, int iterations);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Model training complete. Evaluating on validation set...")]
    public static partial void LogTrainingComplete(ILogger logger);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Validation metrics - Micro Accuracy: {MicroAccuracy:P2}, Macro Accuracy: {MacroAccuracy:P2}, Log Loss: {LogLoss:F4}")]
    public static partial void LogValidationMetrics(
        ILogger logger,
        double microAccuracy,
        double macroAccuracy,
        double logLoss);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Information,
        Message = "Model saved to {ModelPath}")]
    public static partial void LogModelSaved(ILogger logger, string modelPath);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "Model metadata saved to {MetadataPath}")]
    public static partial void LogMetadataSaved(ILogger logger, string metadataPath);
}
