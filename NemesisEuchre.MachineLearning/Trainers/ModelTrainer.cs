using Microsoft.Extensions.Logging;

namespace NemesisEuchre.MachineLearning.Trainers;

public interface IModelTrainer
{
    Task TrainModelAsync(string trainingDataPath, string modelOutputPath, CancellationToken cancellationToken = default);
}

public partial class ModelTrainer(ILogger<ModelTrainer> logger) : IModelTrainer
{
    public Task TrainModelAsync(string trainingDataPath, string modelOutputPath, CancellationToken cancellationToken = default)
    {
        LogModelTrainingNotImplemented();
        throw new NotImplementedException("Model training will be implemented in v0.4");
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Model training will be implemented in v0.4")]
    private partial void LogModelTrainingNotImplemented();
}
