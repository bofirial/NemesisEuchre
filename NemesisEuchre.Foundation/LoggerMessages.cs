using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Foundation;

public static partial class LoggerMessages
{
    // EventID 1-10: Foundation/General operations
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

    // EventID 11-34: MachineLearning core operations
    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Information,
        Message = "Starting training for {ModelType} model")]
    public static partial void LogStartingTraining(ILogger logger, string modelType);

    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Information,
        Message = "Data split complete: {TrainCount} training, {ValidationCount} validation, {TestCount} test samples")]
    public static partial void LogDataSplitComplete(
        ILogger logger,
        int trainCount,
        int validationCount,
        int testCount);

    [LoggerMessage(
        EventId = 13,
        Level = LogLevel.Information,
        Message = "Training model with {Iterations} iterations...")]
    public static partial void LogTrainingModel(ILogger logger, int iterations);

    [LoggerMessage(
        EventId = 14,
        Level = LogLevel.Information,
        Message = "Model training complete. Evaluating on validation set...")]
    public static partial void LogTrainingComplete(ILogger logger);

    [LoggerMessage(
        EventId = 15,
        Level = LogLevel.Information,
        Message = "Validation metrics - Micro Accuracy: {MicroAccuracy:P2}, Macro Accuracy: {MacroAccuracy:P2}, Log Loss: {LogLoss:F4}")]
    public static partial void LogValidationMetrics(
        ILogger logger,
        double microAccuracy,
        double macroAccuracy,
        double logLoss);

    [LoggerMessage(
        EventId = 16,
        Level = LogLevel.Information,
        Message = "Model saved to {ModelPath}")]
    public static partial void LogModelSaved(ILogger logger, string modelPath);

    [LoggerMessage(
        EventId = 17,
        Level = LogLevel.Information,
        Message = "Model metadata saved to {MetadataPath}")]
    public static partial void LogMetadataSaved(ILogger logger, string metadataPath);

    [LoggerMessage(
        EventId = 18,
        Level = LogLevel.Information,
        Message = "Loading training data for {ActorType} (limit: {Limit}, winningTeamOnly: {WinningTeamOnly})")]
    public static partial void LogLoadingTrainingData(
        ILogger logger,
        string actorType,
        int limit,
        bool winningTeamOnly);

    [LoggerMessage(
        EventId = 19,
        Level = LogLevel.Debug,
        Message = "Processed {RecordCount} training records")]
    public static partial void LogTrainingDataProgress(ILogger logger, int recordCount);

    [LoggerMessage(
        EventId = 20,
        Level = LogLevel.Warning,
        Message = "Error during feature engineering, skipping record")]
    public static partial void LogFeatureEngineeringError(ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 21,
        Level = LogLevel.Information,
        Message = "Training data load complete: {SuccessCount} successful, {ErrorCount} errors")]
    public static partial void LogTrainingDataLoadComplete(
        ILogger logger,
        int successCount,
        int errorCount);

    [LoggerMessage(
        EventId = 22,
        Level = LogLevel.Information,
        Message = "Loading model from {ModelPath}")]
    public static partial void LogLoadingModel(ILogger logger, string modelPath);

    [LoggerMessage(
        EventId = 23,
        Level = LogLevel.Information,
        Message = "Model loaded successfully from {ModelPath}")]
    public static partial void LogModelLoadedSuccessfully(ILogger logger, string modelPath);

    [LoggerMessage(
        EventId = 24,
        Level = LogLevel.Information,
        Message = "Model cache invalidated for {ModelPath}")]
    public static partial void LogModelCacheInvalidated(ILogger logger, string modelPath);

    [LoggerMessage(
        EventId = 25,
        Level = LogLevel.Information,
        Message = "All model caches cleared")]
    public static partial void LogModelCacheCleared(ILogger logger);

    [LoggerMessage(
        EventId = 26,
        Level = LogLevel.Information,
        Message = "Evaluation report saved to {EvaluationPath}")]
    public static partial void LogEvaluationReportSaved(ILogger logger, string evaluationPath);

    [LoggerMessage(
        EventId = 27,
        Level = LogLevel.Information,
        Message = "Per-class validation metrics for {ModelType}:")]
    public static partial void LogPerClassMetricsHeader(ILogger logger, string modelType);

    [LoggerMessage(
        EventId = 28,
        Level = LogLevel.Information,
        Message = "  Class {ClassLabel}: Precision={Precision:P2}, Recall={Recall:P2}, F1={F1Score:P2}, Support={Support}")]
    public static partial void LogPerClassMetric(
        ILogger logger,
        int classLabel,
        double precision,
        double recall,
        double f1Score,
        int support);

    [LoggerMessage(
        EventId = 29,
        Level = LogLevel.Information,
        Message = "Weighted averages: Precision={WeightedPrecision:P2}, Recall={WeightedRecall:P2}, F1={WeightedF1Score:P2}")]
    public static partial void LogWeightedAverages(
        ILogger logger,
        double weightedPrecision,
        double weightedRecall,
        double weightedF1Score);

    [LoggerMessage(
        EventId = 30,
        Level = LogLevel.Information,
        Message = "Determining next version for gen{Generation} {DecisionType}")]
    public static partial void LogDeterminingNextVersion(ILogger logger, int generation, string decisionType);

    [LoggerMessage(
        EventId = 31,
        Level = LogLevel.Information,
        Message = "Saving model as gen{Generation}_{DecisionType}_v{Version}.zip")]
    public static partial void LogSavingModelWithVersion(ILogger logger, int generation, string decisionType, int version);

    [LoggerMessage(
        EventId = 32,
        Level = LogLevel.Information,
        Message = "Loading model gen{Generation} {DecisionType} v{Version}")]
    public static partial void LogLoadingModelWithVersion(ILogger logger, int generation, string decisionType, int version);

    [LoggerMessage(
        EventId = 33,
        Level = LogLevel.Information,
        Message = "Found {Count} existing versions for gen{Generation} {DecisionType}")]
    public static partial void LogExistingVersionsFound(ILogger logger, int count, int generation, string decisionType);

    [LoggerMessage(
        EventId = 34,
        Level = LogLevel.Information,
        Message = "Validation metrics - R²: {RSquared:F4}, MAE: {MeanAbsoluteError:F4}, RMSE: {RootMeanSquaredError:F4}")]
    public static partial void LogRegressionValidationMetrics(
        ILogger logger,
        double rSquared,
        double meanAbsoluteError,
        double rootMeanSquaredError);

    // EventID 35-39: MachineLearning.Bots operations
    [LoggerMessage(
        EventId = 35,
        Level = LogLevel.Warning,
        Message = "Model not found for {DecisionType}, will fall back to random selection")]
    public static partial void LogModelNotFound(ILogger logger, string decisionType, Exception exception);

    [LoggerMessage(
        EventId = 36,
        Level = LogLevel.Error,
        Message = "Failed to load model for {DecisionType}, will fall back to random selection")]
    public static partial void LogModelLoadFailed(ILogger logger, string decisionType, Exception exception);

    [LoggerMessage(
        EventId = 37,
        Level = LogLevel.Error,
        Message = "Error predicting CallTrump decision, falling back to random")]
    public static partial void LogCallTrumpPredictionError(ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 38,
        Level = LogLevel.Error,
        Message = "Error predicting DiscardCard decision, falling back to random")]
    public static partial void LogDiscardCardPredictionError(ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 39,
        Level = LogLevel.Error,
        Message = "Error predicting PlayCard decision, falling back to random")]
    public static partial void LogPlayCardPredictionError(ILogger logger, Exception exception);

    // EventID 40-50: Console/Training orchestration
    [LoggerMessage(
        EventId = 40,
        Level = LogLevel.Information,
        Message = "Starting training: ActorType={ActorType}, DecisionType={DecisionType}, Generation={Generation}")]
    public static partial void LogTrainingStarting(
        ILogger logger,
        ActorType actorType,
        DecisionType decisionType,
        int generation);

    [LoggerMessage(
        EventId = 41,
        Level = LogLevel.Warning,
        Message = "Training completed with {FailedCount} failures")]
    public static partial void LogTrainingCompletedWithFailures(
        ILogger logger,
        int failedCount);

    [LoggerMessage(
        EventId = 42,
        Level = LogLevel.Information,
        Message = "Training completed successfully")]
    public static partial void LogTrainingCompletedSuccessfully(ILogger logger);

    [LoggerMessage(
        EventId = 43,
        Level = LogLevel.Warning,
        Message = "No trainers found for decision type: {DecisionType}")]
    public static partial void LogNoTrainersFound(
        ILogger logger,
        DecisionType decisionType);

    [LoggerMessage(
        EventId = 44,
        Level = LogLevel.Information,
        Message = "Starting training for {ActorType} with {TrainerCount} trainer(s): {ModelTypes}")]
    public static partial void LogStartingTrainingWithTrainers(
        ILogger logger,
        ActorType actorType,
        int trainerCount,
        string modelTypes);

    [LoggerMessage(
        EventId = 45,
        Level = LogLevel.Information,
        Message = "Training {ModelType}...")]
    public static partial void LogTrainingModelType(
        ILogger logger,
        string modelType);

    [LoggerMessage(
        EventId = 46,
        Level = LogLevel.Warning,
        Message = "Training failed for {ModelType}: {Error}")]
    public static partial void LogTrainingFailed(
        ILogger logger,
        string modelType,
        string? error);

    [LoggerMessage(
        EventId = 47,
        Level = LogLevel.Information,
        Message = "Training complete: {SuccessCount} succeeded, {FailCount} failed, Duration: {Duration}")]
    public static partial void LogTrainingCompleteWithResults(
        ILogger logger,
        int successCount,
        int failCount,
        TimeSpan duration);

    [LoggerMessage(
        EventId = 48,
        Level = LogLevel.Warning,
        Message = "No training data found for {ActorType} {ModelType}")]
    public static partial void LogNoTrainingDataFound(
        ILogger logger,
        ActorType actorType,
        string modelType);

    [LoggerMessage(
        EventId = 49,
        Level = LogLevel.Information,
        Message = "Successfully trained {ModelType} for {ActorType} (MAE: {MeanAbsoluteError:F4}, R²: {RSquared:F4})")]
    public static partial void LogModelTrainedSuccessfully(
        ILogger logger,
        string modelType,
        ActorType actorType,
        double meanAbsoluteError,
        double rSquared);

    [LoggerMessage(
        EventId = 50,
        Level = LogLevel.Error,
        Message = "Failed to train {ModelType} for {ActorType}")]
    public static partial void LogModelTrainingFailed(
        ILogger logger,
        Exception exception,
        string modelType,
        ActorType actorType);

    // EventID 51-52: Persistence control
    [LoggerMessage(
        EventId = 51,
        Level = LogLevel.Information,
        Message = "Game persistence skipped (--do-not-persist flag enabled)")]
    public static partial void LogGamePersistenceSkipped(ILogger logger);

    [LoggerMessage(
        EventId = 52,
        Level = LogLevel.Information,
        Message = "Batch of {BatchSize} games persistence skipped (--do-not-persist flag enabled)")]
    public static partial void LogBatchGamePersistenceSkipped(ILogger logger, int batchSize);

    // EventID 53-55: ML Bot engine availability
    [LoggerMessage(
        EventId = 53,
        Level = LogLevel.Warning,
        Message = "CallTrump engine not available, selecting random decision.")]
    public static partial void LogCallTrumpEngineNotAvailable(ILogger logger);

    [LoggerMessage(
        EventId = 54,
        Level = LogLevel.Warning,
        Message = "DiscardCard engine not available, selecting random card to discard.")]
    public static partial void LogDiscardCardEngineNotAvailable(ILogger logger);

    [LoggerMessage(
        EventId = 55,
        Level = LogLevel.Warning,
        Message = "PlayCard engine not available, selecting random card to play.")]
    public static partial void LogPlayCardEngineNotAvailable(ILogger logger);

    [LoggerMessage(
        EventId = 56,
        Level = LogLevel.Debug,
        Message = "Bulk insert starting: {ParentCount} parent rows (EF Core), {LeafCount} leaf rows (SqlBulkCopy)")]
    public static partial void LogBulkInsertStarting(ILogger logger, int parentCount, int leafCount);

    [LoggerMessage(
        EventId = 57,
        Level = LogLevel.Debug,
        Message = "Bulk insert completed: {ParentCount} parent rows, {LeafCount} leaf rows in {Elapsed}")]
    public static partial void LogBulkInsertCompleted(ILogger logger, int parentCount, int leafCount, TimeSpan elapsed);
}
