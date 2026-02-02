using Microsoft.ML;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.Trainers;

public interface IModelTrainer<TData>
    where TData : class, new()
{
    Task<TrainingResult> TrainAsync(
        IEnumerable<TData> trainingData,
        CancellationToken cancellationToken = default);

    Task<EvaluationMetrics> EvaluateAsync(
        IDataView testData,
        CancellationToken cancellationToken = default);

    Task SaveModelAsync(
        string modelsDirectory,
        int generation,
        ActorType actorType,
        TrainingResult trainingResult,
        CancellationToken cancellationToken = default);
}
