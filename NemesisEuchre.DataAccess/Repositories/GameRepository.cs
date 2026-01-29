using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.Foundation;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Repositories;

public interface IGameRepository
{
    Task SaveCompletedGameAsync(Game game, CancellationToken cancellationToken = default);

    Task SaveCompletedGamesAsync(IEnumerable<Game> games, CancellationToken cancellationToken = default);
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

            await context.SaveChangesAsync(cancellationToken);

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

                await context.SaveChangesAsync(cancellationToken);

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
}
