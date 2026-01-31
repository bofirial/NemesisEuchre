using Microsoft.ML;

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
        string modelPath,
        TrainingResult trainingResult,
        CancellationToken cancellationToken = default);
}
