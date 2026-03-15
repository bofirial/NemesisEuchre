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

    Task FinalizeAllIdvAsync(
        string baseGenerationName,
        Action<string>? onStatusUpdate = null,
        CancellationToken cancellationToken = default);
}

public class BatchPersistenceCoordinator(
    IServiceScopeFactory serviceScopeFactory,
    IGameToTrainingDataConverter trainingDataConverter,
    ITrainingDataAccumulatorFactory accumulatorFactory,
    IOptions<PersistenceOptions> persistenceOptions,
    ILogger<BatchPersistenceCoordinator> logger) : IPersistenceCoordinator
{
    private readonly PersistenceOptions _persistenceOptions = persistenceOptions.Value;
    private readonly Dictionary<string, ITrainingDataAccumulator> _actorAccumulators = [];

    public async Task ConsumeAndPersistAsync(
        BatchExecutionState state,
        GamePersistenceOptions? persistenceOptions,
        CancellationToken cancellationToken = default)
    {
        persistenceOptions ??= new GamePersistenceOptions(false, null);

        var persistenceStopwatch = Stopwatch.StartNew();
        var batch = new List<Game>(_persistenceOptions.BatchSize);
        Task<Dictionary<string, TrainingDataBatch>>? pendingConversion = null;

        await foreach (var game in state.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            batch.Add(game);

            if (batch.Count >= _persistenceOptions.BatchSize)
            {
                if (pendingConversion != null)
                {
                    AddActorBatches(await pendingConversion.ConfigureAwait(false));
                }

                pendingConversion = await FlushBatchAsync(batch, state, persistenceOptions, cancellationToken).ConfigureAwait(false);
                batch.Clear();
            }
        }

        if (pendingConversion != null)
        {
            AddActorBatches(await pendingConversion.ConfigureAwait(false));
        }

        if (batch.Count > 0)
        {
            var finalConversion = await FlushBatchAsync(batch, state, persistenceOptions, cancellationToken).ConfigureAwait(false);
            if (finalConversion != null)
            {
                AddActorBatches(await finalConversion.ConfigureAwait(false));
            }
        }

        persistenceStopwatch.Stop();
        state.PersistenceDuration = persistenceStopwatch.Elapsed;

        if (persistenceOptions.IdvGenerationName != null)
        {
            foreach (var (actorKey, accumulator) in _actorAccumulators)
            {
                var generationName = $"{persistenceOptions.IdvGenerationName}_{actorKey}";
                accumulator.SaveChunk(generationName, persistenceOptions.AllowOverwrite);
            }
        }
    }

    public async Task FinalizeAllIdvAsync(
        string baseGenerationName,
        Action<string>? onStatusUpdate = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var (actorKey, accumulator) in _actorAccumulators)
        {
            var generationName = $"{baseGenerationName}_{actorKey}";
            await accumulator.FinalizeAsync(generationName, onStatusUpdate, cancellationToken).ConfigureAwait(false);
        }
    }

    private void AddActorBatches(Dictionary<string, TrainingDataBatch> actorBatches)
    {
        foreach (var (actorKey, batch) in actorBatches)
        {
            if (!_actorAccumulators.TryGetValue(actorKey, out var accumulator))
            {
                accumulator = accumulatorFactory.Create();
                _actorAccumulators[actorKey] = accumulator;
            }

            accumulator.Add(batch);
        }
    }

    private async Task<Task<Dictionary<string, TrainingDataBatch>>?> FlushBatchAsync(
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

        Task<Dictionary<string, TrainingDataBatch>>? conversionTask = null;
        if (persistenceOptions.IdvGenerationName != null)
        {
            var snapshot = new List<Game>(games);
            conversionTask = Task.Run(() => trainingDataConverter.ConvertByActor(snapshot), cancellationToken);

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
