using System.Diagnostics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.Console.Models;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Options;

namespace NemesisEuchre.Console.Services;

public interface IModelTrainingOrchestrator
{
    Task<TrainingResults> TrainModelsAsync(
        DecisionType decisionType,
        string outputPath,
        string modelName,
        IProgress<TrainingProgress> progress,
        string idvName,
        bool allowOverwrite = false,
        IOptions<MachineLearningOptions>? optionsOverride = null,
        CancellationToken cancellationToken = default);
}

public class ModelTrainingOrchestrator(
    ITrainerFactory trainerFactory,
    IOptions<PersistenceOptions> persistenceOptions,
    ILogger<ModelTrainingOrchestrator> logger) : IModelTrainingOrchestrator
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S1215:\"GC.Collect\" should not be called", Justification = "Intentional GC between model trainings to reclaim ~6 GB IDataView buffers")]
    public async Task<TrainingResults> TrainModelsAsync(
        DecisionType decisionType,
        string outputPath,
        string modelName,
        IProgress<TrainingProgress> progress,
        string idvName,
        bool allowOverwrite = false,
        IOptions<MachineLearningOptions>? optionsOverride = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var trainers = trainerFactory.GetTrainers(decisionType).ToList();

        if (trainers.Count == 0)
        {
            LoggerMessages.LogNoTrainersFound(logger, decisionType);
            return new TrainingResults(0, 0, [], stopwatch.Elapsed);
        }

        var trainerNames = string.Join(", ", trainers.Select(t => t.ModelType));
        LoggerMessages.LogStartingTrainingWithTrainers(logger, trainers.Count, trainerNames);

        if (!allowOverwrite)
        {
            var conflictingFiles = trainers
                .SelectMany(t =>
                {
                    var baseName = $"{modelName}_{t.ModelType.ToLowerInvariant()}";
                    return new[]
                    {
                        Path.Combine(outputPath, baseName + FileExtensions.ModelZip),
                        Path.Combine(outputPath, baseName + FileExtensions.ModelMetadataJson),
                    };
                })
                .Where(File.Exists)
                .ToList();

            if (conflictingFiles.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Model files already exist and would be overwritten. Use --overwrite to replace them.{Environment.NewLine}" +
                    string.Join(Environment.NewLine, conflictingFiles));
            }
        }

        var results = new List<ModelTrainingResult>();

        foreach (var trainer in trainers)
        {
            LoggerMessages.LogTrainingModelType(logger, trainer.ModelType);

            var idvFilePath = Path.Combine(
                persistenceOptions.Value.IdvOutputPath,
                $"{idvName}_{GetIdvFilePrefix(trainer.DecisionType)}{FileExtensions.Idv}");

            var result = await trainer.ExecuteAsync(
                outputPath,
                modelName,
                progress,
                idvFilePath,
                optionsOverride,
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

    private static string GetIdvFilePrefix(DecisionType type)
    {
        return type switch
        {
            DecisionType.Play => "PlayCard",
            DecisionType.CallTrump => "CallTrump",
            DecisionType.Discard => "DiscardCard",
            DecisionType.All => throw new ArgumentOutOfRangeException(nameof(type), type, "DecisionType.All is not a valid individual decision type"),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported decision type for IDV file prefix"),
        };
    }
}
