using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.Console.Models;
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
        GamePersistenceOptions? persistenceOptions,
        CancellationToken cancellationToken = default);
}

public class BatchPersistenceCoordinator(
    IServiceScopeFactory serviceScopeFactory,
    IGameToTrainingDataConverter trainingDataConverter,
    ITrainingDataAccumulator trainingDataAccumulator,
    IOptions<PersistenceOptions> persistenceOptions,
    ILogger<BatchPersistenceCoordinator> logger) : IPersistenceCoordinator
{
    private readonly PersistenceOptions _persistenceOptions = persistenceOptions.Value;

    public async Task ConsumeAndPersistAsync(
        BatchExecutionState state,
        IBatchProgressReporter? progressReporter,
        GamePersistenceOptions? persistenceOptions,
        CancellationToken cancellationToken = default)
    {
        persistenceOptions ??= new GamePersistenceOptions(false, null);

        var batch = new List<Game>(_persistenceOptions.BatchSize);

        await foreach (var game in state.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            batch.Add(game);

            if (batch.Count >= _persistenceOptions.BatchSize)
            {
                await FlushBatchAsync(batch, state, progressReporter, persistenceOptions, cancellationToken).ConfigureAwait(false);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await FlushBatchAsync(batch, state, progressReporter, persistenceOptions, cancellationToken).ConfigureAwait(false);
        }

        if (persistenceOptions.IdvGenerationName != null)
        {
            trainingDataAccumulator.Save(persistenceOptions.IdvGenerationName, persistenceOptions.AllowOverwrite);
        }
    }

    private async Task FlushBatchAsync(
        List<Game> games,
        BatchExecutionState state,
        IBatchProgressReporter? progressReporter,
        GamePersistenceOptions persistenceOptions,
        CancellationToken cancellationToken)
    {
        var persisted = false;

        if (persistenceOptions.PersistToSql)
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
                persisted = true;
            }
            catch (Exception ex)
            {
                LoggerMessages.LogGamePersistenceFailed(logger, ex);
            }
        }

        if (persistenceOptions.IdvGenerationName != null)
        {
            LoggerMessages.LogIdvGeneratingBatch(logger, games.Count);
            var trainingBatch = trainingDataConverter.Convert(games);
            trainingDataAccumulator.Add(trainingBatch);

            if (!persisted)
            {
                state.SavedGames += games.Count;
                progressReporter?.ReportGamesSaved(state.SavedGames);
            }
        }

        if (!persistenceOptions.PersistToSql && persistenceOptions.IdvGenerationName == null)
        {
            LoggerMessages.LogBatchGamePersistenceSkipped(logger, games.Count);

            state.SavedGames += games.Count;
            progressReporter?.ReportGamesSaved(state.SavedGames);
        }
    }
}
