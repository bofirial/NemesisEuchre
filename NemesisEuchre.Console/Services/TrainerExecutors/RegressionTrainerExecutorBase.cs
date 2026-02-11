using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public abstract class RegressionTrainerExecutorBase<TTrainingData>(
    IModelTrainer<TTrainingData> trainer,
    ITrainingDataLoader<TTrainingData> dataLoader,
    IIdvFileService idvFileService,
    ILogger logger) : ITrainerExecutor
    where TTrainingData : class, new()
{
    private readonly IModelTrainer<TTrainingData> _trainer = trainer ?? throw new ArgumentNullException(nameof(trainer));
    private readonly ITrainingDataLoader<TTrainingData> _dataLoader = dataLoader ?? throw new ArgumentNullException(nameof(dataLoader));
    private readonly IIdvFileService _idvFileService = idvFileService ?? throw new ArgumentNullException(nameof(idvFileService));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public abstract string ModelType { get; }

    public abstract DecisionType DecisionType { get; }

    public async Task<ModelTrainingResult> ExecuteAsync(
        string outputPath,
        int sampleLimit,
        int generation,
        IProgress<TrainingProgress> progress,
        string? idvFilePath = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            progress.Report(new TrainingProgress(ModelType, TrainingPhase.LoadingData, 0, "Streaming training data..."));

            TrainingResult trainingResult;

            if (idvFilePath != null && File.Exists(idvFilePath))
            {
                LoggerMessages.LogIdvFileLoading(_logger, idvFilePath);
                var dataView = _idvFileService.Load(idvFilePath);

                progress.Report(new TrainingProgress(ModelType, TrainingPhase.Training, 25, "Training model (IDV)..."));
                trainingResult = await _trainer.TrainAsync(dataView, preShuffled: true, cancellationToken);
            }
            else
            {
                var streamingData = _dataLoader.StreamTrainingData(
                    sampleLimit,
                    winningTeamOnly: false,
                    shuffle: true,
                    cancellationToken);

                progress.Report(new TrainingProgress(ModelType, TrainingPhase.Training, 25, "Training model (streaming)..."));
                trainingResult = await _trainer.TrainAsync(streamingData, preShuffled: true, cancellationToken);
            }

            if (trainingResult.TrainingSamples == 0)
            {
                LoggerMessages.LogNoTrainingDataFound(_logger, ModelType);
                progress.Report(new TrainingProgress(ModelType, TrainingPhase.Failed, 0, "No training data available"));
                return new ModelTrainingResult(ModelType, false, ErrorMessage: "No training data available");
            }

            progress.Report(new TrainingProgress(ModelType, TrainingPhase.Training, 75, "Training complete"));

            progress.Report(new TrainingProgress(ModelType, TrainingPhase.Saving, 75, "Saving model..."));
            await _trainer.SaveModelAsync(outputPath, generation, trainingResult, cancellationToken);

            progress.Report(new TrainingProgress(ModelType, TrainingPhase.Complete, 100, "Complete"));

            var metrics = (RegressionEvaluationMetrics)trainingResult.ValidationMetrics;

            LoggerMessages.LogModelTrainedSuccessfully(
                _logger,
                ModelType,
                metrics.MeanAbsoluteError,
                metrics.RSquared);

            return new ModelTrainingResult(
                ModelType,
                true,
                ModelPath: Path.Combine(outputPath, $"{ModelType}_Gen{generation}.zip"),
                MeanAbsoluteError: metrics.MeanAbsoluteError,
                RSquared: metrics.RSquared);
        }
        catch (Exception ex)
        {
            LoggerMessages.LogModelTrainingFailed(_logger, ex, ModelType);
            progress.Report(new TrainingProgress(ModelType, TrainingPhase.Failed, 0, $"Error: {ex.Message}"));
            return new ModelTrainingResult(ModelType, false, ErrorMessage: ex.Message);
        }
    }
}
