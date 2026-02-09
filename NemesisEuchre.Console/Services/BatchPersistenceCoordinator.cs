using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Services;

public interface IPersistenceCoordinator
{
    Task ConsumeAndPersistAsync(
        BatchExecutionState state,
        IBatchProgressReporter? progressReporter,
        bool doNotPersist,
        CancellationToken cancellationToken = default);
}

public class BatchPersistenceCoordinator(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<PersistenceOptions> persistenceOptions,
    ILogger<BatchPersistenceCoordinator> logger) : IPersistenceCoordinator
{
    private readonly PersistenceOptions _persistenceOptions = persistenceOptions.Value;

    public async Task ConsumeAndPersistAsync(
        BatchExecutionState state,
        IBatchProgressReporter? progressReporter,
        bool doNotPersist,
        CancellationToken cancellationToken = default)
    {
        var batch = new List<Game>(_persistenceOptions.BatchSize);

        await foreach (var game in state.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            batch.Add(game);

            if (batch.Count >= _persistenceOptions.BatchSize)
            {
                await FlushBatchAsync(batch, state, progressReporter, doNotPersist, cancellationToken).ConfigureAwait(false);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await FlushBatchAsync(batch, state, progressReporter, doNotPersist, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task FlushBatchAsync(
        List<Game> games,
        BatchExecutionState state,
        IBatchProgressReporter? progressReporter,
        bool doNotPersist,
        CancellationToken cancellationToken)
    {
        if (doNotPersist)
        {
            LoggerMessages.LogBatchGamePersistenceSkipped(logger, games.Count);

            state.SavedGames += games.Count;
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

                var snapshot = new List<Game>(games);
                using var scope = serviceScopeFactory.CreateScope();
                var gameRepository = scope.ServiceProvider.GetRequiredService<IGameRepository>();
                await gameRepository.SaveCompletedGamesBulkAsync(snapshot, saveProgress, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LoggerMessages.LogGamePersistenceFailed(logger, ex);
            }
        }
    }
}
