using NemesisEuchre.Console.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public interface ITrainerExecutor
{
    string ModelType { get; }

    DecisionType DecisionType { get; }

    Task<ModelTrainingResult> ExecuteAsync(
        ActorType actorType,
        string outputPath,
        int sampleLimit,
        int generation,
        IProgress<TrainingProgress> progress,
        CancellationToken cancellationToken = default);
}
