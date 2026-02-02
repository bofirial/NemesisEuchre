using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.MachineLearning.DataAccess;

public interface ITrainingDataLoader<TTrainingData>
    where TTrainingData : class, new()
{
    Task<IEnumerable<TTrainingData>> LoadTrainingDataAsync(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        CancellationToken cancellationToken = default);
}
