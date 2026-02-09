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

public partial class TrainingDataRepository(
    NemesisEuchreDbContext context,
    ILogger<TrainingDataRepository> logger) : ITrainingDataRepository
{
    private static readonly Dictionary<Type, IEntityTypeConfig> EntityConfigs = new()
    {
        [typeof(CallTrumpDecisionEntity)] = new EntityTypeConfig<CallTrumpDecisionEntity>(
            ctx => ctx.CallTrumpDecisions!,
            query => query
                .Include(e => e.CardsInHand)
                .Include(e => e.ValidDecisions)),
        [typeof(DiscardCardDecisionEntity)] = new EntityTypeConfig<DiscardCardDecisionEntity>(
            ctx => ctx.DiscardCardDecisions!,
            query => query
                .Include(e => e.CardsInHand)),
        [typeof(PlayCardDecisionEntity)] = new EntityTypeConfig<PlayCardDecisionEntity>(
            ctx => ctx.PlayCardDecisions!,
            query => query
                .Include(e => e.CardsInHand)
                .Include(e => e.PlayedCards)
                .Include(e => e.ValidCards)
                .Include(e => e.KnownVoids)
                .Include(e => e.CardsAccountedFor)
                .AsSplitQuery()),
    };

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

    private static IEntityTypeConfig GetConfig<TEntity>()
        where TEntity : class, IDecisionEntity
    {
        return EntityConfigs.TryGetValue(typeof(TEntity), out var config)
            ? config
            : throw new NotSupportedException($"Entity type {typeof(TEntity).Name} is not supported for training data retrieval");
    }

    private IQueryable<TEntity> BuildQuery<TEntity>(
        ActorType actorType,
        int limit,
        bool winningTeamOnly)
        where TEntity : class, IDecisionEntity
    {
        var config = GetConfig<TEntity>();
        var dbSet = ((EntityTypeConfig<TEntity>)config).GetDbSet(context);
        var query = ((EntityTypeConfig<TEntity>)config).ApplyIncludes(dbSet.AsNoTracking());

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
}
