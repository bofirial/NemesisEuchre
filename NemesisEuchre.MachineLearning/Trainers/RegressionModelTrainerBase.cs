using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.Foundation;
using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.MachineLearning.Trainers;

public interface IModelTrainer<TData>
    where TData : class, new()
{
    Task<TrainingResult> TrainAsync(
        IDataView dataView,
        bool preShuffled = false,
        IProgress<TrainingIterationUpdate>? iterationProgress = null,
        CancellationToken cancellationToken = default);

    Task<EvaluationMetrics> EvaluateAsync(
        IDataView testData,
        CancellationToken cancellationToken = default);

    Task SaveModelAsync(
        string modelsDirectory,
        string modelName,
        TrainingResult trainingResult,
        CancellationToken cancellationToken = default);
}

public abstract partial class RegressionModelTrainerBase<TData>(
    MLContext mlContext,
    IDataSplitter dataSplitter,
    IModelPersistenceService persistenceService,
    IOptions<MachineLearningOptions> options,
    ILogger logger) : IModelTrainer<TData>
    where TData : class, new()
{
    protected MLContext MlContext { get; } = mlContext ?? throw new ArgumentNullException(nameof(mlContext));

    protected IDataSplitter DataSplitter { get; } = dataSplitter ?? throw new ArgumentNullException(nameof(dataSplitter));

    protected IModelPersistenceService PersistenceService { get; } = persistenceService ?? throw new ArgumentNullException(nameof(persistenceService));

    protected MachineLearningOptions Options { get; } = options?.Value ?? throw new ArgumentNullException(nameof(options));

    protected ILogger Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));

    protected ITransformer? TrainedModel { get; private set; }

    public Task<TrainingResult> TrainAsync(
        IDataView dataView,
        bool preShuffled = false,
        IProgress<TrainingIterationUpdate>? iterationProgress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dataView);

        var dataSplit = DataSplitter.Split(dataView, preShuffled: preShuffled);
        return TrainFromSplitAsync(dataSplit, iterationProgress, cancellationToken);
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
        string modelName,
        TrainingResult trainingResult,
        CancellationToken cancellationToken = default)
    {
        if (TrainedModel == null)
        {
            throw new InvalidOperationException("No trained model to save. Call TrainAsync first.");
        }

        var metadata = CreateModelMetadata(modelName, trainingResult);

        return PersistenceService.SaveModelAsync<TData>(
            TrainedModel,
            MlContext,
            modelsDirectory,
            modelName,
            GetModelType(),
            trainingResult,
            metadata,
            cancellationToken);
    }

#pragma warning disable RCS1158 // Static member in generic type - TryParseIterationProgress is intentionally type-independent
    internal static bool TryParseIterationProgress(string message, out int iteration, out double? metric)
    {
        iteration = 0;
        metric = null;

        var match = IterationLogPattern().Match(message);
        if (!match.Success)
        {
            return false;
        }

        iteration = int.Parse(match.Groups["iter"].Value, System.Globalization.CultureInfo.InvariantCulture);

        if (match.Groups["metric"].Success
            && double.TryParse(match.Groups["metric"].Value, System.Globalization.CultureInfo.InvariantCulture, out var metricValue))
        {
            metric = metricValue;
        }

        return true;
    }
#pragma warning restore RCS1158

    protected abstract IEstimator<ITransformer> BuildPipeline(IDataView trainingData);

    protected abstract string GetModelType();

    [GeneratedRegex(@"\[(?<iter>\d+)\].*?:\s*(?<metric>[\d.]+(?:e[+-]?\d+)?)", RegexOptions.Compiled)]
    private static partial Regex IterationLogPattern();

    private async Task<TrainingResult> TrainFromSplitAsync(
        DataSplit dataSplit,
        IProgress<TrainingIterationUpdate>? iterationProgress,
        CancellationToken cancellationToken)
    {
        if (Logger.IsEnabled(LogLevel.Information))
        {
            LoggerMessages.LogStartingTraining(Logger, GetModelType());
        }

        LoggerMessages.LogDataSplitComplete(
            Logger,
            dataSplit.TrainRowCount,
            dataSplit.ValidationRowCount,
            dataSplit.TestRowCount);

        var pipeline = BuildPipeline(dataSplit.Train);

        LoggerMessages.LogTrainingModel(Logger, Options.NumberOfIterations);

        EventHandler<LoggingEventArgs>? logHandler = null;
        if (iterationProgress != null)
        {
            logHandler = (_, e) =>
            {
                if (TryParseIterationProgress(e.RawMessage, out var iteration, out var metricValue))
                {
                    iterationProgress.Report(new TrainingIterationUpdate(
                        iteration, Options.NumberOfIterations, metricValue));
                }
            };
            MlContext.Log += logHandler;
        }

        try
        {
            TrainedModel = await Task.Run(() => pipeline.Fit(dataSplit.Train), cancellationToken);
        }
        finally
        {
            if (logHandler != null)
            {
                MlContext.Log -= logHandler;
            }
        }

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
        string modelName,
        TrainingResult trainingResult)
    {
        var regressionMetrics = trainingResult.ValidationMetrics as RegressionEvaluationMetrics
            ?? throw new InvalidOperationException("Expected RegressionEvaluationMetrics");

        return new ModelMetadata(
            GetModelType(),
            modelName,
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
                regressionMetrics.MeanSquaredError,
                regressionMetrics.LossFunction),
            "1.0");
    }
}
