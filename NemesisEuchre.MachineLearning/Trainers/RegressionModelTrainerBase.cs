using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.MachineLearning.Trainers;

public abstract class RegressionModelTrainerBase<TData>(
    MLContext mlContext,
    IDataSplitter dataSplitter,
    IModelVersionManager versionManager,
    IOptions<MachineLearningOptions> options,
    ILogger logger) : IModelTrainer<TData>
    where TData : class, new()
{
    protected MLContext MlContext { get; } = mlContext ?? throw new ArgumentNullException(nameof(mlContext));

    protected IDataSplitter DataSplitter { get; } = dataSplitter ?? throw new ArgumentNullException(nameof(dataSplitter));

    protected IModelVersionManager VersionManager { get; } = versionManager ?? throw new ArgumentNullException(nameof(versionManager));

    protected MachineLearningOptions Options { get; } = options?.Value ?? throw new ArgumentNullException(nameof(options));

    protected ILogger Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));

    protected ITransformer? TrainedModel { get; private set; }

    public async Task<TrainingResult> TrainAsync(
        IEnumerable<TData> trainingData,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trainingData);

        LoggerMessages.LogStartingTraining(Logger, GetModelType());

        var dataSplit = DataSplitter.Split(trainingData);

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
        }, cancellationToken);
    }

    public async Task SaveModelAsync(
        string modelsDirectory,
        int generation,
        ActorType actorType,
        TrainingResult trainingResult,
        CancellationToken cancellationToken = default)
    {
        ValidateSaveModelParameters(modelsDirectory, generation, trainingResult);

        EnsureDirectoryExists(modelsDirectory);

        var decisionType = GetModelType().ToLowerInvariant();
        var version = DetermineNextVersion(modelsDirectory, generation, decisionType);
        var modelPath = VersionManager.GetModelPath(modelsDirectory, generation, decisionType, version);

        await SaveModelFileAsync(modelPath, generation, decisionType, version, cancellationToken);
        await SaveMetadataAsync(modelPath, generation, actorType, trainingResult, version, cancellationToken);
        await SaveEvaluationReportAsync(modelPath, trainingResult, cancellationToken);
    }

    protected abstract IEstimator<ITransformer> BuildPipeline(IDataView trainingData);

    protected abstract string GetModelType();

    private static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private void ValidateSaveModelParameters(string modelsDirectory, int generation, TrainingResult trainingResult)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelsDirectory);
        ArgumentNullException.ThrowIfNull(trainingResult);

        if (generation < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(generation), "Generation must be at least 1");
        }

        if (TrainedModel == null)
        {
            throw new InvalidOperationException("No trained model to save. Call TrainAsync first.");
        }
    }

    private int DetermineNextVersion(string modelsDirectory, int generation, string decisionType)
    {
        LoggerMessages.LogDeterminingNextVersion(Logger, generation, decisionType);
        var version = VersionManager.GetNextVersion(modelsDirectory, generation, decisionType);
        LoggerMessages.LogExistingVersionsFound(Logger, version - 1, generation, decisionType);
        return version;
    }

    private Task SaveModelFileAsync(
        string modelPath,
        int generation,
        string decisionType,
        int version,
        CancellationToken cancellationToken)
    {
        var schema = MlContext.Data.LoadFromEnumerable([new TData()]).Schema;

        LoggerMessages.LogSavingModelWithVersion(Logger, generation, decisionType, version);

        return Task.Run(
            () =>
        {
            MlContext.Model.Save(TrainedModel!, schema, modelPath);
            LoggerMessages.LogModelSaved(Logger, modelPath);
        }, cancellationToken);
    }

    private async Task SaveMetadataAsync(
        string modelPath,
        int generation,
        ActorType actorType,
        TrainingResult trainingResult,
        int version,
        CancellationToken cancellationToken)
    {
        var metadataPath = Path.ChangeExtension(modelPath, ".json");
        var metadata = CreateModelMetadata(generation, actorType, trainingResult, version);
        var json = JsonSerializer.Serialize(metadata, JsonSerializationOptions.Default);

        await File.WriteAllTextAsync(metadataPath, json, cancellationToken);
        LoggerMessages.LogMetadataSaved(Logger, metadataPath);
    }

    private async Task SaveEvaluationReportAsync(
        string modelPath,
        TrainingResult trainingResult,
        CancellationToken cancellationToken)
    {
        var evaluationPath = Path.ChangeExtension(modelPath, ".evaluation.json");
        var evaluationReport = CreateEvaluationReport(
            trainingResult.ValidationMetrics as RegressionEvaluationMetrics ?? throw new InvalidOperationException("Expected RegressionEvaluationMetrics"),
            trainingResult.ValidationSamples);
        var reportJson = JsonSerializer.Serialize(evaluationReport, JsonSerializationOptions.WithNaNHandling);

        await File.WriteAllTextAsync(evaluationPath, reportJson, cancellationToken);
        LoggerMessages.LogEvaluationReportSaved(Logger, evaluationPath);
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
