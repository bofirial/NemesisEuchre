using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public class PlayCardRegressionTrainerExecutor(
    IModelTrainer<PlayCardTrainingData> trainer,
    ITrainingDataLoader<PlayCardTrainingData> dataLoader,
    ILogger<PlayCardRegressionTrainerExecutor> logger) : ITrainerExecutor
{
    public string ModelType => "PlayCardRegression";

    public DecisionType DecisionType => DecisionType.Play;

    public async Task<ModelTrainingResult> ExecuteAsync(
        ActorType actorType,
        string outputPath,
        int sampleLimit,
        int generation,
        IProgress<TrainingProgress> progress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            progress.Report(new TrainingProgress(ModelType, TrainingPhase.LoadingData, 0, "Loading training data..."));

            var trainingData = await dataLoader.LoadTrainingDataAsync(
                actorType,
                sampleLimit,
                winningTeamOnly: false,
                cancellationToken);

            var dataList = trainingData.ToList();
            if (dataList.Count == 0)
            {
                LoggerMessages.LogNoTrainingDataFound(logger, actorType, ModelType);
                progress.Report(new TrainingProgress(ModelType, TrainingPhase.Failed, 0, "No training data available"));
                return new ModelTrainingResult(ModelType, false, ErrorMessage: "No training data available");
            }

            progress.Report(new TrainingProgress(ModelType, TrainingPhase.LoadingData, 25, $"Loaded {dataList.Count:N0} samples"));

            progress.Report(new TrainingProgress(ModelType, TrainingPhase.Training, 25, "Training model..."));
            var trainingResult = await trainer.TrainAsync(dataList, cancellationToken);

            progress.Report(new TrainingProgress(ModelType, TrainingPhase.Training, 75, "Training complete"));

            progress.Report(new TrainingProgress(ModelType, TrainingPhase.Saving, 75, "Saving model..."));
            await trainer.SaveModelAsync(outputPath, generation, actorType, trainingResult, cancellationToken);

            progress.Report(new TrainingProgress(ModelType, TrainingPhase.Complete, 100, "Complete"));

            var metrics = (RegressionEvaluationMetrics)trainingResult.ValidationMetrics;

            LoggerMessages.LogModelTrainedSuccessfully(
                logger,
                ModelType,
                actorType,
                metrics.MeanAbsoluteError,
                metrics.RSquared);

            return new ModelTrainingResult(
                ModelType,
                true,
                ModelPath: Path.Combine(outputPath, $"{actorType}_{ModelType}_Gen{generation}.zip"),
                MeanAbsoluteError: metrics.MeanAbsoluteError,
                RSquared: metrics.RSquared);
        }
        catch (Exception ex)
        {
            LoggerMessages.LogModelTrainingFailed(logger, ex, ModelType, actorType);
            progress.Report(new TrainingProgress(ModelType, TrainingPhase.Failed, 0, $"Error: {ex.Message}"));
            return new ModelTrainingResult(ModelType, false, ErrorMessage: ex.Message);
        }
    }
}
