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

    IEnumerable<TEntity> GetDecisionData<TEntity>(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        bool shuffle = false)
        where TEntity : class, IDecisionEntity;

    int GetDecisionDataCount<TEntity>(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false)
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

        var query = BuildQuery<TEntity>(actorType, limit, winningTeamOnly);

        await foreach (var entity in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return entity;
        }
    }

    public IEnumerable<TEntity> GetDecisionData<TEntity>(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        bool shuffle = false)
        where TEntity : class, IDecisionEntity
    {
        LoggerMessages.LogRetrievingTrainingData(
            logger,
            typeof(TEntity).Name,
            actorType.ToString(),
            limit,
            winningTeamOnly);

        var query = BuildQuery<TEntity>(actorType, limit, winningTeamOnly);

        if (shuffle)
        {
            query = query.OrderBy(_ => Guid.NewGuid());
        }

        return query.AsEnumerable();
    }

    public int GetDecisionDataCount<TEntity>(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false)
        where TEntity : class, IDecisionEntity
    {
        var query = BuildQuery<TEntity>(actorType, limit, winningTeamOnly);
        return query.Count();
    }

    private static IQueryable<TEntity> ApplyIncludes<TEntity>(IQueryable<TEntity> query)
        where TEntity : class, IDecisionEntity
    {
        return typeof(TEntity).Name switch
        {
            nameof(CallTrumpDecisionEntity) =>
                (IQueryable<TEntity>)(object)((IQueryable<CallTrumpDecisionEntity>)(object)query)
                    .Include(e => e.CardsInHand)
                    .Include(e => e.ValidDecisions),

            nameof(DiscardCardDecisionEntity) =>
                (IQueryable<TEntity>)(object)((IQueryable<DiscardCardDecisionEntity>)(object)query)
                    .Include(e => e.CardsInHand),

            nameof(PlayCardDecisionEntity) =>
                (IQueryable<TEntity>)(object)((IQueryable<PlayCardDecisionEntity>)(object)query)
                    .Include(e => e.CardsInHand)
                    .Include(e => e.PlayedCards)
                    .Include(e => e.ValidCards)
                    .Include(e => e.KnownVoids)
                    .Include(e => e.CardsAccountedFor)
                    .AsSplitQuery(),

            _ => query,
        };
    }

    private IQueryable<TEntity> BuildQuery<TEntity>(
        ActorType actorType,
        int limit,
        bool winningTeamOnly)
        where TEntity : class, IDecisionEntity
    {
        var dbSet = GetDbSet<TEntity>();
        var query = ApplyIncludes(dbSet.AsNoTracking());

        query = query.Where(d => d.ActorTypeId == (int)actorType);

        if (winningTeamOnly)
        {
            query = query.Where(d => d.DidTeamWinGame == true);
        }

        if (limit > 0)
        {
            query = query.Take(limit);
        }

        return query;
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
