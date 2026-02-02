using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Repositories;

public interface IGameRepository
{
    Task SaveCompletedGameAsync(Game game, CancellationToken cancellationToken = default);

    Task SaveCompletedGamesAsync(IEnumerable<Game> games, CancellationToken cancellationToken = default);

    Task SaveCompletedGamesBulkAsync(
        IEnumerable<Game> games,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    [Obsolete("Use ITrainingDataRepository.GetDecisionDataAsync<CallTrumpDecisionEntity> instead")]
    IAsyncEnumerable<CallTrumpDecisionEntity> GetCallTrumpTrainingDataAsync(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        CancellationToken cancellationToken = default);

    [Obsolete("Use ITrainingDataRepository.GetDecisionDataAsync<DiscardCardDecisionEntity> instead")]
    IAsyncEnumerable<DiscardCardDecisionEntity> GetDiscardCardTrainingDataAsync(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        CancellationToken cancellationToken = default);

    [Obsolete("Use ITrainingDataRepository.GetDecisionDataAsync<PlayCardDecisionEntity> instead")]
    IAsyncEnumerable<PlayCardDecisionEntity> GetPlayCardTrainingDataAsync(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        CancellationToken cancellationToken = default);
}

public class GameRepository(
    NemesisEuchreDbContext context,
    ILogger<GameRepository> logger,
    IGameToEntityMapper mapper,
    IOptions<PersistenceOptions> options) : IGameRepository
{
    private readonly PersistenceOptions _options = options.Value;

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

            var shouldDisableChangeTracking = gamesList.Count >= _options.MaxBatchSizeForChangeTracking;
            var originalChangeTrackingSetting = context.ChangeTracker.AutoDetectChangesEnabled;

            try
            {
                if (shouldDisableChangeTracking)
                {
                    context.ChangeTracker.AutoDetectChangesEnabled = false;
                }

                foreach (var game in gamesList)
                {
                    var gameEntity = mapper.Map(game);
                    context.Games!.Add(gameEntity);
                }

                if (shouldDisableChangeTracking)
                {
                    context.ChangeTracker.DetectChanges();
                }

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                LoggerMessages.LogBatchGamesPersisted(logger, gamesList.Count);
            }
            finally
            {
                context.ChangeTracker.AutoDetectChangesEnabled = originalChangeTrackingSetting;
            }
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
                var originalChangeTrackingSetting = context.ChangeTracker.AutoDetectChangesEnabled;

                try
                {
                    context.ChangeTracker.AutoDetectChangesEnabled = false;

                    foreach (var game in gamesList)
                    {
                        var gameEntity = mapper.Map(game);
                        context.Games!.Add(gameEntity);
                    }

                    context.ChangeTracker.DetectChanges();
                    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    LoggerMessages.LogBatchGamesPersisted(logger, gamesList.Count);
                    progress?.Report(gamesList.Count);
                }
                finally
                {
                    context.ChangeTracker.AutoDetectChangesEnabled = originalChangeTrackingSetting;
                    context.ChangeTracker.Clear();
                }
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

    [Obsolete("Use ITrainingDataRepository.GetDecisionDataAsync<CallTrumpDecisionEntity> instead")]
    public async IAsyncEnumerable<CallTrumpDecisionEntity> GetCallTrumpTrainingDataAsync(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        LoggerMessages.LogRetrievingTrainingData(
            logger,
            nameof(CallTrumpDecisionEntity),
            actorType.ToString(),
            limit,
            winningTeamOnly);

        var query = context.CallTrumpDecisions!
            .AsNoTracking()
            .Where(d => d.ActorType == actorType);

        if (winningTeamOnly)
        {
            query = query.Where(d => d.DidTeamWinGame == true);
        }

        if (limit > 0)
        {
            query = query.Take(limit);
        }

        await foreach (var decision in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return decision;
        }
    }

    [Obsolete("Use ITrainingDataRepository.GetDecisionDataAsync<DiscardCardDecisionEntity> instead")]
    public async IAsyncEnumerable<DiscardCardDecisionEntity> GetDiscardCardTrainingDataAsync(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        LoggerMessages.LogRetrievingTrainingData(
            logger,
            nameof(DiscardCardDecisionEntity),
            actorType.ToString(),
            limit,
            winningTeamOnly);

        var query = context.DiscardCardDecisions!
            .AsNoTracking()
            .Where(d => d.ActorType == actorType);

        if (winningTeamOnly)
        {
            query = query.Where(d => d.DidTeamWinGame == true);
        }

        if (limit > 0)
        {
            query = query.Take(limit);
        }

        await foreach (var decision in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return decision;
        }
    }

    [Obsolete("Use ITrainingDataRepository.GetDecisionDataAsync<PlayCardDecisionEntity> instead")]
    public async IAsyncEnumerable<PlayCardDecisionEntity> GetPlayCardTrainingDataAsync(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        LoggerMessages.LogRetrievingTrainingData(
            logger,
            nameof(PlayCardDecisionEntity),
            actorType.ToString(),
            limit,
            winningTeamOnly);

        var query = context.PlayCardDecisions!
            .AsNoTracking()
            .Where(d => d.ActorType == actorType);

        if (winningTeamOnly)
        {
            query = query.Where(d => d.DidTeamWinGame == true);
        }

        if (limit > 0)
        {
            query = query.Take(limit);
        }

        await foreach (var decision in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return decision;
        }
    }
}
