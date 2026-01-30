using NemesisEuchre.DataAccess.Repositories;

namespace NemesisEuchre.MachineLearning.DataAccess;

public interface ITrainingDataLoader
{
    Task<IEnumerable<object>> LoadTrainingDataAsync(int gameCount, CancellationToken cancellationToken = default);
}

public class TrainingDataLoader(IGameRepository gameRepository) : ITrainingDataLoader
{
    public Task<IEnumerable<object>> LoadTrainingDataAsync(int gameCount, CancellationToken cancellationToken = default)
    {
        _ = gameRepository;
        throw new NotImplementedException("Training data loading will be implemented in v0.4");
    }
}
