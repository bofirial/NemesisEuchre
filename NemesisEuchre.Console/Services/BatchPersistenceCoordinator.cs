using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Services;

public interface IPersistenceCoordinator
{
    Task SavePendingGamesAsync(
        BatchExecutionState state,
        IBatchProgressReporter? progressReporter,
        bool doNotPersist,
        bool force,
        CancellationToken cancellationToken = default);
}

public class BatchPersistenceCoordinator(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<BatchPersistenceCoordinator> logger) : IPersistenceCoordinator
{
    public async Task SavePendingGamesAsync(
        BatchExecutionState state,
        IBatchProgressReporter? progressReporter,
        bool doNotPersist,
        bool force,
        CancellationToken cancellationToken = default)
    {
        var gamesToSave = await state.ExecuteWithLockAsync(
            () =>
        {
            if ((force && state.PendingGames.Count > 0) ||
                state.PendingGames.Count >= state.BatchSize)
            {
                var games = new List<Game>(state.PendingGames);
                state.PendingGames.Clear();
                return games;
            }

            return null;
        }, cancellationToken).ConfigureAwait(false);

        if (gamesToSave?.Count > 0)
        {
            if (doNotPersist)
            {
                LoggerMessages.LogBatchGamePersistenceSkipped(logger, gamesToSave.Count);

                state.SavedGames += gamesToSave.Count;
                progressReporter?.ReportGamesSaved(state.SavedGames);
            }
            else
            {
                try
                {
                    var saveProgress = new Progress<int>(count =>
                    {
                        state.SavedGames += count;
                        progressReporter?.ReportGamesSaved(state.SavedGames);
                    });

                    using var scope = serviceScopeFactory.CreateScope();
                    var gameRepository = scope.ServiceProvider.GetRequiredService<IGameRepository>();
                    await gameRepository.SaveCompletedGamesBulkAsync(gamesToSave, saveProgress, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LoggerMessages.LogGamePersistenceFailed(logger, ex);
                }
            }
        }
    }
}
