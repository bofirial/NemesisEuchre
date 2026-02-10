using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.MachineLearning.Trainers;

public interface IModelTrainer<TData>
    where TData : class, new()
{
    Task<TrainingResult> TrainAsync(
        IEnumerable<TData> trainingData,
        bool preShuffled = false,
        CancellationToken cancellationToken = default);

    Task<TrainingResult> TrainAsync(
        IDataView dataView,
        bool preShuffled = false,
        CancellationToken cancellationToken = default);

    Task<EvaluationMetrics> EvaluateAsync(
        IDataView testData,
        CancellationToken cancellationToken = default);

    Task SaveModelAsync(
        string modelsDirectory,
        int generation,
        ActorType actorType,
        TrainingResult trainingResult,
        CancellationToken cancellationToken = default);
}

public abstract class RegressionModelTrainerBase<TData>(
    MLContext mlContext,
    IDataSplitter dataSplitter,
    IModelVersionManager versionManager,
    IModelPersistenceService persistenceService,
    IOptions<MachineLearningOptions> options,
    ILogger logger) : IModelTrainer<TData>
    where TData : class, new()
{
    protected MLContext MlContext { get; } = mlContext ?? throw new ArgumentNullException(nameof(mlContext));

    protected IDataSplitter DataSplitter { get; } = dataSplitter ?? throw new ArgumentNullException(nameof(dataSplitter));

    protected IModelVersionManager VersionManager { get; } = versionManager ?? throw new ArgumentNullException(nameof(versionManager));

    protected IModelPersistenceService PersistenceService { get; } = persistenceService ?? throw new ArgumentNullException(nameof(persistenceService));

    protected MachineLearningOptions Options { get; } = options?.Value ?? throw new ArgumentNullException(nameof(options));

    protected ILogger Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));

    protected ITransformer? TrainedModel { get; private set; }

    public Task<TrainingResult> TrainAsync(
        IEnumerable<TData> trainingData,
        bool preShuffled = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trainingData);

        var dataSplit = DataSplitter.Split(trainingData, preShuffled: preShuffled);
        return TrainFromSplitAsync(dataSplit, cancellationToken);
    }

    public Task<TrainingResult> TrainAsync(
        IDataView dataView,
        bool preShuffled = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dataView);

        var dataSplit = DataSplitter.Split(dataView, preShuffled: preShuffled);
        return TrainFromSplitAsync(dataSplit, cancellationToken);
    }

    Task<EvaluationMetrics> IModelTrainer<TData>.EvaluateAsync(
        IDataView testData,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Use EvaluateRegressionAsync for regression models");
    }

    public Task<RegressionEvaluationMetrics> EvaluateAsync(
        IDataView testData,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testData);

        if (TrainedModel == null)
        {
            throw new InvalidOperationException("Model must be trained before evaluation. Call TrainAsync first.");
        }

        return Task.Run(
            () =>
                {
                    var predictions = TrainedModel.Transform(testData);
                    var mlMetrics = MlContext.Regression.Evaluate(
                        predictions,
                        labelColumnName: "Label",
                        scoreColumnName: "Score");

                    return new RegressionEvaluationMetrics(
                        mlMetrics.RSquared,
                        mlMetrics.MeanAbsoluteError,
                        mlMetrics.RootMeanSquaredError,
                        mlMetrics.MeanSquaredError,
                        mlMetrics.LossFunction);
                },
            cancellationToken);
    }

    public Task SaveModelAsync(
        string modelsDirectory,
        int generation,
        ActorType actorType,
        TrainingResult trainingResult,
        CancellationToken cancellationToken = default)
    {
        if (TrainedModel == null)
        {
            throw new InvalidOperationException("No trained model to save. Call TrainAsync first.");
        }

        var version = VersionManager.GetNextVersion(modelsDirectory, generation, GetModelType().ToLowerInvariant());
        var metadata = CreateModelMetadata(generation, actorType, trainingResult, version);
        var evaluationReport = CreateEvaluationReport(
            trainingResult.ValidationMetrics as RegressionEvaluationMetrics ?? throw new InvalidOperationException("Expected RegressionEvaluationMetrics"),
            trainingResult.ValidationSamples);

        return PersistenceService.SaveModelAsync<TData>(
            TrainedModel,
            MlContext,
            modelsDirectory,
            generation,
            GetModelType(),
            actorType,
            trainingResult,
            metadata,
            evaluationReport,
            cancellationToken);
    }

    protected abstract IEstimator<ITransformer> BuildPipeline(IDataView trainingData);

    protected abstract string GetModelType();

    private async Task<TrainingResult> TrainFromSplitAsync(
        DataSplit dataSplit,
        CancellationToken cancellationToken)
    {
        LoggerMessages.LogStartingTraining(Logger, GetModelType());

        LoggerMessages.LogDataSplitComplete(
            Logger,
            dataSplit.TrainRowCount,
            dataSplit.ValidationRowCount,
            dataSplit.TestRowCount);

        var pipeline = BuildPipeline(dataSplit.Train);

        LoggerMessages.LogTrainingModel(Logger, Options.NumberOfIterations);

        TrainedModel = await Task.Run(() => pipeline.Fit(dataSplit.Train), cancellationToken);

        LoggerMessages.LogTrainingComplete(Logger);

        var validationMetrics = await EvaluateAsync(dataSplit.Validation, cancellationToken);

        LoggerMessages.LogRegressionValidationMetrics(
            Logger,
            validationMetrics.RSquared,
            validationMetrics.MeanAbsoluteError,
            validationMetrics.RootMeanSquaredError);

        return new TrainingResult(
            TrainedModel,
            validationMetrics,
            dataSplit.TrainRowCount,
            dataSplit.ValidationRowCount,
            dataSplit.TestRowCount);
    }

    private ModelMetadata CreateModelMetadata(
        int generation,
        ActorType actorType,
        TrainingResult trainingResult,
        int version)
    {
        var regressionMetrics = trainingResult.ValidationMetrics as RegressionEvaluationMetrics
            ?? throw new InvalidOperationException("Expected RegressionEvaluationMetrics");

        return new ModelMetadata(
            GetModelType(),
            actorType,
            generation,
            version,
            DateTime.UtcNow,
            trainingResult.TrainingSamples,
            trainingResult.ValidationSamples,
            trainingResult.TestSamples,
            new HyperparametersMetadata(
                "LightGbm",
                Options.NumberOfLeaves,
                Options.NumberOfIterations,
                Options.LearningRate,
                Options.RandomSeed),
            new RegressionMetricsMetadata(
                regressionMetrics.RSquared,
                regressionMetrics.MeanAbsoluteError,
                regressionMetrics.RootMeanSquaredError,
                regressionMetrics.MeanSquaredError),
            "1.0");
    }

    private RegressionEvaluationReport CreateEvaluationReport(
        RegressionEvaluationMetrics metrics,
        int testSamples)
    {
        return new RegressionEvaluationReport(
            GetModelType(),
            DateTime.UtcNow,
            testSamples,
            metrics.RSquared,
            metrics.MeanAbsoluteError,
            metrics.RootMeanSquaredError,
            metrics.MeanSquaredError,
            metrics.LossFunction);
    }
}
