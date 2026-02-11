using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation;

namespace NemesisEuchre.DataAccess.Repositories;

public interface ITrainingDataRepository
{
    IAsyncEnumerable<TEntity> GetDecisionDataAsync<TEntity>(
        int limit = 0,
        bool winningTeamOnly = false,
        CancellationToken cancellationToken = default)
        where TEntity : class, IDecisionEntity;

    IEnumerable<TEntity> GetDecisionData<TEntity>(
        int limit = 0,
        bool winningTeamOnly = false,
        bool shuffle = false)
        where TEntity : class, IDecisionEntity;

    int GetDecisionDataCount<TEntity>(
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
                .Include(e => e.CardsAccountedFor)),
    };

    public async IAsyncEnumerable<TEntity> GetDecisionDataAsync<TEntity>(
        int limit = 0,
        bool winningTeamOnly = false,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TEntity : class, IDecisionEntity
    {
        LoggerMessages.LogRetrievingTrainingData(
            logger,
            typeof(TEntity).Name,
            limit,
            winningTeamOnly);

        var query = BuildQuery<TEntity>(limit, winningTeamOnly);

        // Materialize fully so split queries complete before yielding entities.
        var entities = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        foreach (var entity in entities)
        {
            yield return entity;
        }
    }

    public IEnumerable<TEntity> GetDecisionData<TEntity>(
        int limit = 0,
        bool winningTeamOnly = false,
        bool shuffle = false)
        where TEntity : class, IDecisionEntity
    {
        LoggerMessages.LogRetrievingTrainingData(
            logger,
            typeof(TEntity).Name,
            limit,
            winningTeamOnly);

        var query = BuildQuery<TEntity>(limit, winningTeamOnly);

        if (shuffle)
        {
            query = query.OrderBy(_ => Guid.NewGuid());
        }

        return [.. query];
    }

    public int GetDecisionDataCount<TEntity>(
        int limit = 0,
        bool winningTeamOnly = false)
        where TEntity : class, IDecisionEntity
    {
        var query = BuildQuery<TEntity>(limit, winningTeamOnly);
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
        int limit,
        bool winningTeamOnly)
        where TEntity : class, IDecisionEntity
    {
        var config = GetConfig<TEntity>();
        var dbSet = ((EntityTypeConfig<TEntity>)config).GetDbSet(context);
        var query = ((EntityTypeConfig<TEntity>)config).ApplyIncludes(dbSet.AsNoTrackingWithIdentityResolution());

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
