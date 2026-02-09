using System.Diagnostics;

using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Services;

public interface IModelTrainingOrchestrator
{
    Task<TrainingResults> TrainModelsAsync(
        ActorType actorType,
        DecisionType decisionType,
        string outputPath,
        int sampleLimit,
        int generation,
        IProgress<TrainingProgress> progress,
        CancellationToken cancellationToken = default);
}

public class ModelTrainingOrchestrator(
    ITrainerFactory trainerFactory,
    ILogger<ModelTrainingOrchestrator> logger) : IModelTrainingOrchestrator
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S1215:\"GC.Collect\" should not be called", Justification = "Intentional GC between model trainings to reclaim ~6 GB IDataView buffers")]
    public async Task<TrainingResults> TrainModelsAsync(
        ActorType actorType,
        DecisionType decisionType,
        string outputPath,
        int sampleLimit,
        int generation,
        IProgress<TrainingProgress> progress,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var trainers = trainerFactory.GetTrainers(decisionType).ToList();

        if (trainers.Count == 0)
        {
            LoggerMessages.LogNoTrainersFound(logger, decisionType);
            return new TrainingResults(0, 0, [], stopwatch.Elapsed);
        }

        LoggerMessages.LogStartingTrainingWithTrainers(
            logger,
            actorType,
            trainers.Count,
            string.Join(", ", trainers.Select(t => t.ModelType)));

        var results = new List<ModelTrainingResult>();

        foreach (var trainer in trainers)
        {
            LoggerMessages.LogTrainingModelType(logger, trainer.ModelType);

            var result = await trainer.ExecuteAsync(
                actorType,
                outputPath,
                sampleLimit,
                generation,
                progress,
                cancellationToken);

            results.Add(result);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!result.Success)
            {
                LoggerMessages.LogTrainingFailed(logger, trainer.ModelType, result.ErrorMessage);
            }
        }

        stopwatch.Stop();

        var successCount = results.Count(r => r.Success);
        var failCount = results.Count(r => !r.Success);

        LoggerMessages.LogTrainingCompleteWithResults(logger, successCount, failCount, stopwatch.Elapsed);

        return new TrainingResults(successCount, failCount, results, stopwatch.Elapsed);
    }
}
