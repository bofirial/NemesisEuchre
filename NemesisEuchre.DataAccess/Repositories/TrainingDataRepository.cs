using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation;
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

public class TrainingDataRepository(
    NemesisEuchreDbContext context,
    ILogger<TrainingDataRepository> logger) : ITrainingDataRepository
{
    public async IAsyncEnumerable<TEntity> GetDecisionDataAsync<TEntity>(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TEntity : class, IDecisionEntity
    {
        LoggerMessages.LogRetrievingTrainingData(
            logger,
            typeof(TEntity).Name,
            actorType.ToString(),
            limit,
            winningTeamOnly);

        var dbSet = GetDbSet<TEntity>();
        var query = dbSet.AsNoTracking().Where(d => d.ActorType == actorType);

        if (winningTeamOnly)
        {
            query = query.Where(d => d.DidTeamWinGame == true);
        }

        if (limit > 0)
        {
            query = query.Take(limit);
        }

        await foreach (var entity in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return entity;
        }
    }

    private DbSet<TEntity> GetDbSet<TEntity>()
        where TEntity : class, IDecisionEntity
    {
        return typeof(TEntity).Name switch
        {
            nameof(CallTrumpDecisionEntity) => (DbSet<TEntity>)(object)context.CallTrumpDecisions!,
            nameof(DiscardCardDecisionEntity) => (DbSet<TEntity>)(object)context.DiscardCardDecisions!,
            nameof(PlayCardDecisionEntity) => (DbSet<TEntity>)(object)context.PlayCardDecisions!,
            _ => throw new NotSupportedException($"Entity type {typeof(TEntity).Name} is not supported for training data retrieval"),
        };
    }
}
