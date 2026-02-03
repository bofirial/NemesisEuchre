using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.DataAccess.Repositories;

public interface ITrainingDataRepository
{
    IAsyncEnumerable<TEntity> GetDecisionDataAsync<TEntity>(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        CancellationToken cancellationToken = default)
        where TEntity : class, IDecisionEntity;
}
