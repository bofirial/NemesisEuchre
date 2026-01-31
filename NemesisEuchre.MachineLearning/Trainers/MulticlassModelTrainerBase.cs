using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Data;

using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.MachineLearning.Trainers;

public abstract class MulticlassModelTrainerBase<TData>(
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

        LoggerMessages.LogValidationMetrics(
            Logger,
            validationMetrics.MicroAccuracy,
            validationMetrics.MacroAccuracy,
            validationMetrics.LogLoss);

        var evaluationReport = CreateEvaluationReport(validationMetrics, dataSplit.ValidationRowCount);
        LogPerClassMetrics(validationMetrics, evaluationReport);

        return new TrainingResult(
            TrainedModel,
            validationMetrics,
            dataSplit.TrainRowCount,
            dataSplit.ValidationRowCount,
            dataSplit.TestRowCount);
    }

    public Task<EvaluationMetrics> EvaluateAsync(
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
            var mlMetrics = MlContext.MulticlassClassification.Evaluate(
                predictions,
                labelColumnName: "Label",
                predictedLabelColumnName: "PredictedLabel");

            var numberOfClasses = GetNumberOfClasses();
            var perClassLogLoss = ExtractPerClassMetric(mlMetrics.PerClassLogLoss, numberOfClasses);
            var confusionMatrix = ConvertConfusionMatrix(mlMetrics.ConfusionMatrix, numberOfClasses);
            var perClassMetrics = MetricsCalculator.CalculatePerClassMetrics(confusionMatrix);

            return new EvaluationMetrics(
                mlMetrics.MicroAccuracy,
                mlMetrics.MacroAccuracy,
                mlMetrics.LogLoss,
                mlMetrics.LogLossReduction,
                perClassLogLoss,
                confusionMatrix,
                perClassMetrics);
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

    protected abstract int GetNumberOfClasses();

    private static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static double[] ExtractPerClassMetric(IReadOnlyList<double> metrics, int numberOfClasses)
    {
        if (metrics == null || metrics.Count == 0)
        {
            return new double[numberOfClasses];
        }

        var result = new double[numberOfClasses];
        for (var i = 0; i < Math.Min(metrics.Count, numberOfClasses); i++)
        {
            result[i] = metrics[i];
        }

        return result;
    }

    private static int[][] ConvertConfusionMatrix(ConfusionMatrix confusionMatrix, int numberOfClasses)
    {
        if (confusionMatrix == null)
        {
            return CreateEmptyConfusionMatrix(numberOfClasses);
        }

        var counts = confusionMatrix.Counts;
        var matrix = new int[numberOfClasses][];

        for (var i = 0; i < numberOfClasses; i++)
        {
            matrix[i] = new int[numberOfClasses];
            for (var j = 0; j < numberOfClasses; j++)
            {
                if (i < counts.Count && j < counts[i].Count)
                {
                    matrix[i][j] = (int)counts[i][j];
                }
            }
        }

        return matrix;
    }

    private static int[][] CreateEmptyConfusionMatrix(int numberOfClasses)
    {
        var matrix = new int[numberOfClasses][];
        for (var i = 0; i < numberOfClasses; i++)
        {
            matrix[i] = new int[numberOfClasses];
        }

        return matrix;
    }

    private static double CalculateWeightedAverage(
        PerClassMetrics[] metrics,
        Func<PerClassMetrics, double> valueSelector,
        Func<PerClassMetrics, int> weightSelector)
    {
        double weightedSum = 0;
        int totalWeight = 0;

        foreach (var metric in metrics)
        {
            var value = valueSelector(metric);
            if (!double.IsNaN(value))
            {
                var weight = weightSelector(metric);
                weightedSum += value * weight;
                totalWeight += weight;
            }
        }

        return totalWeight > 0 ? weightedSum / totalWeight : double.NaN;
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
            trainingResult.ValidationMetrics,
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
            new MetricsMetadata(
                trainingResult.ValidationMetrics.MicroAccuracy,
                trainingResult.ValidationMetrics.MacroAccuracy,
                trainingResult.ValidationMetrics.LogLoss,
                trainingResult.ValidationMetrics.LogLossReduction),
            "1.0");
    }

    private void LogPerClassMetrics(EvaluationMetrics metrics, EvaluationReport report)
    {
        LoggerMessages.LogPerClassMetricsHeader(Logger, GetModelType());

        foreach (var classMetric in metrics.PerClassMetrics)
        {
            LoggerMessages.LogPerClassMetric(
                Logger,
                classMetric.ClassLabel,
                classMetric.Precision,
                classMetric.Recall,
                classMetric.F1Score,
                classMetric.Support);
        }

        LoggerMessages.LogWeightedAverages(
            Logger,
            report.Overall.WeightedPrecision,
            report.Overall.WeightedRecall,
            report.Overall.WeightedF1Score);
    }

    private EvaluationReport CreateEvaluationReport(
        EvaluationMetrics metrics,
        int testSamples)
    {
        var weightedPrecision = CalculateWeightedAverage(metrics.PerClassMetrics, m => m.Precision, m => m.Support);
        var weightedRecall = CalculateWeightedAverage(metrics.PerClassMetrics, m => m.Recall, m => m.Support);
        var weightedF1Score = CalculateWeightedAverage(metrics.PerClassMetrics, m => m.F1Score, m => m.Support);

        var overallMetrics = new OverallMetrics(
            metrics.MicroAccuracy,
            metrics.MacroAccuracy,
            metrics.LogLoss,
            metrics.LogLossReduction,
            weightedPrecision,
            weightedRecall,
            weightedF1Score);

        return new EvaluationReport(
            GetModelType(),
            DateTime.UtcNow,
            testSamples,
            overallMetrics,
            metrics.PerClassMetrics,
            metrics.ConfusionMatrix);
    }
}
