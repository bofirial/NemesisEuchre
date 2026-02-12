using System.Diagnostics;

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
        GamePersistenceOptions? persistenceOptions,
        CancellationToken cancellationToken = default)
    {
        persistenceOptions ??= new GamePersistenceOptions(false, null);

        var persistenceStopwatch = Stopwatch.StartNew();
        var batch = new List<Game>(_persistenceOptions.BatchSize);
        Task<TrainingDataBatch>? pendingConversion = null;

        await foreach (var game in state.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            batch.Add(game);

            if (batch.Count >= _persistenceOptions.BatchSize)
            {
                if (pendingConversion != null)
                {
                    trainingDataAccumulator.Add(await pendingConversion.ConfigureAwait(false));
                }

                pendingConversion = await FlushBatchAsync(batch, state, persistenceOptions, cancellationToken).ConfigureAwait(false);
                batch.Clear();
            }
        }

        if (pendingConversion != null)
        {
            trainingDataAccumulator.Add(await pendingConversion.ConfigureAwait(false));
        }

        if (batch.Count > 0)
        {
            var finalConversion = await FlushBatchAsync(batch, state, persistenceOptions, cancellationToken).ConfigureAwait(false);
            if (finalConversion != null)
            {
                trainingDataAccumulator.Add(await finalConversion.ConfigureAwait(false));
            }
        }

        persistenceStopwatch.Stop();
        state.PersistenceDuration = persistenceStopwatch.Elapsed;

        if (persistenceOptions.IdvGenerationName != null)
        {
            trainingDataAccumulator.SaveChunk(persistenceOptions.IdvGenerationName, persistenceOptions.AllowOverwrite);
        }
    }

    private async Task<Task<TrainingDataBatch>?> FlushBatchAsync(
        List<Game> games,
        BatchExecutionState state,
        GamePersistenceOptions persistenceOptions,
        CancellationToken cancellationToken)
    {
        var persisted = false;

        if (persistenceOptions.PersistToSql)
        {
            try
            {
                var saveProgress = new Progress<int>(count => state.SavedGames += count);

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

        Task<TrainingDataBatch>? conversionTask = null;
        if (persistenceOptions.IdvGenerationName != null)
        {
            var snapshot = new List<Game>(games);
            conversionTask = Task.Run(() => trainingDataConverter.Convert(snapshot), cancellationToken);

            if (!persisted)
            {
                state.SavedGames += games.Count;
            }
        }

        if (!persistenceOptions.PersistToSql && persistenceOptions.IdvGenerationName == null)
        {
            state.SavedGames += games.Count;
        }

        return conversionTask;
    }
}
