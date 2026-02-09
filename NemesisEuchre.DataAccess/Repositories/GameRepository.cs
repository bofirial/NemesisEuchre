using System.Diagnostics;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.DataAccess.Services;
using NemesisEuchre.Foundation;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Repositories;

public interface IGameRepository
{
    Task<GameEntity?> GetGameByIdAsync(int gameId, bool includeDecisions, CancellationToken cancellationToken = default);

    Task SaveCompletedGameAsync(Game game, CancellationToken cancellationToken = default);

    Task SaveCompletedGamesAsync(IEnumerable<Game> games, CancellationToken cancellationToken = default);

    Task SaveCompletedGamesBulkAsync(
        IEnumerable<Game> games,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);
}

public class GameRepository(
    NemesisEuchreDbContext context,
    ILogger<GameRepository> logger,
    IGameToEntityMapper mapper,
    IBulkInsertService bulkInsertService,
    IOptions<PersistenceOptions> options) : IGameRepository
{
    private readonly PersistenceOptions _options = options.Value;

    public Task<GameEntity?> GetGameByIdAsync(int gameId, bool includeDecisions, CancellationToken cancellationToken = default)
    {
        var query = context.Games!
            .AsNoTracking()
            .Include(g => g.GamePlayers)
            .Include(g => g.Deals).ThenInclude(d => d.DealPlayers).ThenInclude(dp => dp.StartingHandCards)
            .Include(g => g.Deals).ThenInclude(d => d.DealDeckCards)
            .Include(g => g.Deals).ThenInclude(d => d.DealKnownPlayerSuitVoids)
            .Include(g => g.Deals).ThenInclude(d => d.Tricks).ThenInclude(t => t.TrickCardsPlayed)
            .AsSplitQuery();

        if (includeDecisions)
        {
            query = query
                .Include(g => g.Deals).ThenInclude(d => d.CallTrumpDecisions).ThenInclude(ct => ct.CardsInHand)
                .Include(g => g.Deals).ThenInclude(d => d.CallTrumpDecisions).ThenInclude(ct => ct.ValidDecisions)
                .Include(g => g.Deals).ThenInclude(d => d.CallTrumpDecisions).ThenInclude(ct => ct.PredictedPoints)
                .Include(g => g.Deals).ThenInclude(d => d.DiscardCardDecisions).ThenInclude(dc => dc.CardsInHand)
                .Include(g => g.Deals).ThenInclude(d => d.DiscardCardDecisions).ThenInclude(dc => dc.PredictedPoints)
                .Include(g => g.Deals).ThenInclude(d => d.Tricks).ThenInclude(t => t.PlayCardDecisions).ThenInclude(pc => pc.CardsInHand)
                .Include(g => g.Deals).ThenInclude(d => d.Tricks).ThenInclude(t => t.PlayCardDecisions).ThenInclude(pc => pc.PlayedCards)
                .Include(g => g.Deals).ThenInclude(d => d.Tricks).ThenInclude(t => t.PlayCardDecisions).ThenInclude(pc => pc.ValidCards)
                .Include(g => g.Deals).ThenInclude(d => d.Tricks).ThenInclude(t => t.PlayCardDecisions).ThenInclude(pc => pc.KnownVoids)
                .Include(g => g.Deals).ThenInclude(d => d.Tricks).ThenInclude(t => t.PlayCardDecisions).ThenInclude(pc => pc.CardsAccountedFor)
                .Include(g => g.Deals).ThenInclude(d => d.Tricks).ThenInclude(t => t.PlayCardDecisions).ThenInclude(pc => pc.PredictedPoints)
                .AsSplitQuery();
        }

        return query.FirstOrDefaultAsync(g => g.GameId == gameId, cancellationToken);
    }

    public async Task SaveCompletedGameAsync(Game game, CancellationToken cancellationToken = default)
    {
        try
        {
            LoggerMessages.LogPersistingCompletedGame(logger, game.GameStatus);

            var gameEntity = mapper.Map(game);

            context.Games!.Add(gameEntity);

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            LoggerMessages.LogGamePersistedSuccessfully(logger, gameEntity.GameId);
        }
        catch (Exception ex)
        {
            LoggerMessages.LogGamePersistenceFailed(logger, ex);
        }
    }

    public async Task SaveCompletedGamesAsync(IEnumerable<Game> games, CancellationToken cancellationToken = default)
    {
        var gamesList = games as List<Game> ?? [.. games];
        if (gamesList.Count == 0)
        {
            return;
        }

        try
        {
            LoggerMessages.LogPersistingBatchedGames(logger, gamesList.Count);

            await ExecuteWithChangeTrackerOptimizationAsync(
                gamesList,
                gamesList.Count >= _options.MaxBatchSizeForChangeTracking,
                clearTrackerAfter: false,
                cancellationToken).ConfigureAwait(false);

            LoggerMessages.LogBatchGamesPersisted(logger, gamesList.Count);
        }
        catch (Exception ex)
        {
            LoggerMessages.LogBatchGamePersistenceFailed(logger, gamesList.Count, ex);
        }
    }

    public async Task SaveCompletedGamesBulkAsync(
        IEnumerable<Game> games,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var gamesList = games as List<Game> ?? [.. games];
        if (gamesList.Count == 0)
        {
            return;
        }

        try
        {
            LoggerMessages.LogPersistingBatchedGames(logger, gamesList.Count);

            if (_options.UseBulkInsert && gamesList.Count >= _options.BulkInsertThreshold)
            {
                await ExecuteHybridBulkInsertAsync(gamesList, cancellationToken).ConfigureAwait(false);

                LoggerMessages.LogBatchGamesPersisted(logger, gamesList.Count);
                progress?.Report(gamesList.Count);
            }
            else
            {
                await SaveCompletedGamesAsync(games, cancellationToken).ConfigureAwait(false);
                progress?.Report(gamesList.Count);
            }
        }
        catch (Exception ex)
        {
            LoggerMessages.LogBatchGamePersistenceFailed(logger, gamesList.Count, ex);
        }
    }

    private async Task ExecuteHybridBulkInsertAsync(
        List<Game> gamesList,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var entities = gamesList.ConvertAll(mapper.Map);
        var cache = new LeafCollectionCache(entities);

        LoggerMessages.LogBulkInsertStarting(logger, entities.Count, cache.LeafCount);

        var originalChangeTrackingSetting = context.ChangeTracker.AutoDetectChangesEnabled;

        try
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            context.Games!.AddRange(entities);

            context.ChangeTracker.DetectChanges();

            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(
                async ct =>
                {
                    await using var transaction = await context.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

                    await context.SaveChangesAsync(ct).ConfigureAwait(false);

                    cache.PopulateForeignKeys();

                    var dbConnection = context.Database.GetDbConnection();
                    var sqlConnection = (SqlConnection)dbConnection;
                    var sqlTransaction = (SqlTransaction)transaction.GetDbTransaction();

                    await bulkInsertService.BulkInsertLeafEntitiesAsync(
                        cache,
                        sqlConnection,
                        sqlTransaction,
                        _options.BulkCopyTimeout,
                        ct).ConfigureAwait(false);

                    await transaction.CommitAsync(ct).ConfigureAwait(false);
                },
                cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();
            LoggerMessages.LogBulkInsertCompleted(logger, entities.Count, cache.LeafCount, stopwatch.Elapsed);
        }
        finally
        {
            context.ChangeTracker.AutoDetectChangesEnabled = originalChangeTrackingSetting;
            context.ChangeTracker.Clear();
        }
    }

    private async Task ExecuteWithChangeTrackerOptimizationAsync(
        List<Game> gamesList,
        bool disableChangeTracking,
        bool clearTrackerAfter,
        CancellationToken cancellationToken)
    {
        var originalChangeTrackingSetting = context.ChangeTracker.AutoDetectChangesEnabled;

        try
        {
            if (disableChangeTracking)
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;
            }

            foreach (var game in gamesList)
            {
                var gameEntity = mapper.Map(game);
                context.Games!.Add(gameEntity);
            }

            if (disableChangeTracking)
            {
                context.ChangeTracker.DetectChanges();
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            context.ChangeTracker.AutoDetectChangesEnabled = originalChangeTrackingSetting;

            if (clearTrackerAfter)
            {
                context.ChangeTracker.Clear();
            }
        }
    }
}
