using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.Console;

public static partial class LoggerMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Starting training: ActorType={ActorType}, DecisionType={DecisionType}, Generation={Generation}")]
    public static partial void LogTrainingStarting(
        ILogger logger,
        ActorType actorType,
        DecisionType decisionType,
        int generation);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Training completed with {FailedCount} failures")]
    public static partial void LogTrainingCompletedWithFailures(
        ILogger logger,
        int failedCount);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Training completed successfully")]
    public static partial void LogTrainingCompletedSuccessfully(ILogger logger);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Warning,
        Message = "No trainers found for decision type: {DecisionType}")]
    public static partial void LogNoTrainersFound(
        ILogger logger,
        DecisionType decisionType);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Starting training for {ActorType} with {TrainerCount} trainer(s): {ModelTypes}")]
    public static partial void LogStartingTrainingWithTrainers(
        ILogger logger,
        ActorType actorType,
        int trainerCount,
        string modelTypes);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Information,
        Message = "Training {ModelType}...")]
    public static partial void LogTrainingModel(
        ILogger logger,
        string modelType);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Warning,
        Message = "Training failed for {ModelType}: {Error}")]
    public static partial void LogTrainingFailed(
        ILogger logger,
        string modelType,
        string? error);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Information,
        Message = "Training complete: {SuccessCount} succeeded, {FailCount} failed, Duration: {Duration}")]
    public static partial void LogTrainingComplete(
        ILogger logger,
        int successCount,
        int failCount,
        TimeSpan duration);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Warning,
        Message = "No training data found for {ActorType} {ModelType}")]
    public static partial void LogNoTrainingDataFound(
        ILogger logger,
        ActorType actorType,
        string modelType);

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Information,
        Message = "Successfully trained {ModelType} for {ActorType} (MAE: {MeanAbsoluteError:F4}, R²: {RSquared:F4})")]
    public static partial void LogModelTrainedSuccessfully(
        ILogger logger,
        string modelType,
        ActorType actorType,
        double meanAbsoluteError,
        double rSquared);

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Error,
        Message = "Failed to train {ModelType} for {ActorType}")]
    public static partial void LogModelTrainingFailed(
        ILogger logger,
        Exception exception,
        string modelType,
        ActorType actorType);
}
