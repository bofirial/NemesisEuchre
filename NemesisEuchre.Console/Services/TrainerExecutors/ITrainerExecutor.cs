using NemesisEuchre.Console.Models;
using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Services.TrainerExecutors;

public interface ITrainerExecutor
{
    string ModelType { get; }

    DecisionType DecisionType { get; }

    Task<ModelTrainingResult> ExecuteAsync(
        string outputPath,
        int sampleLimit,
        int generation,
        IProgress<TrainingProgress> progress,
        string? idvFilePath = null,
        CancellationToken cancellationToken = default);
}
