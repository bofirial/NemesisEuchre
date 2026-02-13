using System.Diagnostics;

using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public interface ITrainerExecutor
{
    string ModelType { get; }

    DecisionType DecisionType { get; }

    Task<ModelTrainingResult> ExecuteAsync(
        string outputPath,
        string modelName,
        IProgress<TrainingProgress> progress,
        string idvFilePath,
        CancellationToken cancellationToken = default);
}

public abstract class RegressionTrainerExecutorBase<TTrainingData>(
    IModelTrainer<TTrainingData> trainer,
    IIdvFileService idvFileService,
    ILogger logger) : ITrainerExecutor
    where TTrainingData : class, new()
{
    private readonly IModelTrainer<TTrainingData> _trainer = trainer ?? throw new ArgumentNullException(nameof(trainer));
    private readonly IIdvFileService _idvFileService = idvFileService ?? throw new ArgumentNullException(nameof(idvFileService));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public abstract string ModelType { get; }

    public abstract DecisionType DecisionType { get; }

    public async Task<ModelTrainingResult> ExecuteAsync(
        string outputPath,
        string modelName,
        IProgress<TrainingProgress> progress,
        string idvFilePath,
        CancellationToken cancellationToken = default)
    {
        var modelStopwatch = Stopwatch.StartNew();
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(idvFilePath);

            if (!File.Exists(idvFilePath))
            {
                throw new FileNotFoundException($"IDV file not found: {idvFilePath}", idvFilePath);
            }

            var metadataPath = idvFilePath + FileExtensions.IdvMetadataSuffix;
            var metadata = _idvFileService.LoadMetadata(metadataPath);

            progress.Report(new TrainingProgress(ModelType, TrainingPhase.LoadingData, 0, "Streaming training data..."));

            LoggerMessages.LogIdvFileLoading(_logger, idvFilePath);
            var dataView = _idvFileService.Load(idvFilePath);

            var rowCount = (int)(dataView.GetRowCount() ?? -1);
            if (rowCount != metadata.RowCount)
            {
                throw new InvalidOperationException(
                    $"IDV row count mismatch for {idvFilePath}: metadata says {metadata.RowCount} but IDataView has {rowCount} rows");
            }

            LoggerMessages.LogIdvMetadataValidated(_logger, idvFilePath, metadata.RowCount, metadata.GameCount);

            progress.Report(new TrainingProgress(ModelType, TrainingPhase.Training, 25, "Training model (IDV)..."));

            var trainingResult = await _trainer.TrainAsync(dataView, preShuffled: true, cancellationToken);

            if (trainingResult.TrainingSamples == 0)
            {
                LoggerMessages.LogNoTrainingDataFound(_logger, ModelType);
                progress.Report(new TrainingProgress(ModelType, TrainingPhase.Failed, 0, "No training data available"));
                return new ModelTrainingResult(ModelType, false, ErrorMessage: "No training data available", Duration: modelStopwatch.Elapsed);
            }

            progress.Report(new TrainingProgress(ModelType, TrainingPhase.Training, 75, "Training complete"));

            var metrics = (RegressionEvaluationMetrics)trainingResult.ValidationMetrics;

            progress.Report(new TrainingProgress(ModelType, TrainingPhase.Saving, 75, "Saving model..."));
            await _trainer.SaveModelAsync(outputPath, modelName, trainingResult, cancellationToken);

            progress.Report(new TrainingProgress(
                ModelType,
                TrainingPhase.Complete,
                100,
                "Complete",
                ValidationMae: metrics.MeanAbsoluteError,
                ValidationRSquared: metrics.RSquared));

            LoggerMessages.LogModelTrainedSuccessfully(
                _logger,
                ModelType,
                metrics.MeanAbsoluteError,
                metrics.RSquared);

            return new ModelTrainingResult(
                ModelType,
                true,
                ModelPath: Path.Combine(outputPath, $"{ModelType}_{modelName}{FileExtensions.ModelZip}"),
                MeanAbsoluteError: metrics.MeanAbsoluteError,
                RSquared: metrics.RSquared,
                Duration: modelStopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            LoggerMessages.LogModelTrainingFailed(_logger, ex, ModelType);
            progress.Report(new TrainingProgress(ModelType, TrainingPhase.Failed, 0, $"Error: {ex.Message}"));
            return new ModelTrainingResult(ModelType, false, ErrorMessage: ex.Message, Duration: modelStopwatch.Elapsed);
        }
    }
}
