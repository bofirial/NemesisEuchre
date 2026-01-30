using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.Console.Models;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Options;

namespace NemesisEuchre.Console.Services;

public interface IBatchGameOrchestrator
{
    Task<BatchGameResults> RunBatchAsync(
        int numberOfGames,
        IBatchProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default);
}

public class BatchGameOrchestrator(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<GameExecutionOptions> options,
    IOptions<PersistenceOptions> persistenceOptions,
    ILogger<BatchGameOrchestrator> logger) : IBatchGameOrchestrator
{
    private readonly GameExecutionOptions _options = options.Value;
    private readonly PersistenceOptions _persistenceOptions = persistenceOptions.Value;

    public async Task<BatchGameResults> RunBatchAsync(
        int numberOfGames,
        IBatchProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default)
    {
        if (numberOfGames <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfGames), "Number of games must be greater than zero.");
        }

        const int maxGamesPerSubBatch = 10000;
        if (numberOfGames > maxGamesPerSubBatch)
        {
            return await RunBatchesInSubBatchesAsync(numberOfGames, maxGamesPerSubBatch, progressReporter, cancellationToken).ConfigureAwait(false);
        }

        var stopwatch = Stopwatch.StartNew();
        using var state = new BatchExecutionState(_persistenceOptions.BatchSize);
        var tasks = CreateGameExecutionTasks(numberOfGames, state, progressReporter, cancellationToken);

        await Task.WhenAll(tasks).ConfigureAwait(false);
        await SavePendingGamesAsync(state, progressReporter, force: true, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();

        return AggregateResults(numberOfGames, state, stopwatch.Elapsed);
    }

    private static BatchGameResults AggregateResults(
        int numberOfGames,
        BatchExecutionState state,
        TimeSpan elapsedTime)
    {
        return new BatchGameResults
        {
            TotalGames = numberOfGames,
            Team1Wins = state.Team1Wins,
            Team2Wins = state.Team2Wins,
            FailedGames = state.FailedGames,
            TotalDeals = state.TotalDeals,
            ElapsedTime = elapsedTime,
        };
    }

    private async Task<BatchGameResults> RunBatchesInSubBatchesAsync(
        int totalGames,
        int subBatchSize,
        IBatchProgressReporter? progressReporter,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var completedSoFar = 0;
        var savedSoFar = 0;
        var totalTeam1Wins = 0;
        var totalTeam2Wins = 0;
        var totalFailedGames = 0;
        var totalDeals = 0;

        while (completedSoFar < totalGames)
        {
            var gamesInThisBatch = Math.Min(subBatchSize, totalGames - completedSoFar);

            var subProgressReporter = progressReporter == null ? null : new SubBatchProgressReporter(
                progressReporter,
                completedSoFar,
                savedSoFar);

            using var state = new BatchExecutionState(_persistenceOptions.BatchSize);
            var tasks = CreateGameExecutionTasks(gamesInThisBatch, state, subProgressReporter, cancellationToken);

            await Task.WhenAll(tasks).ConfigureAwait(false);
            await SavePendingGamesAsync(state, subProgressReporter, force: true, cancellationToken).ConfigureAwait(false);

            totalTeam1Wins += state.Team1Wins;
            totalTeam2Wins += state.Team2Wins;
            totalFailedGames += state.FailedGames;
            totalDeals += state.TotalDeals;
            completedSoFar += gamesInThisBatch;
            savedSoFar += state.SavedGames;
        }

        stopwatch.Stop();

        return new BatchGameResults
        {
            TotalGames = totalGames,
            Team1Wins = totalTeam1Wins,
            Team2Wins = totalTeam2Wins,
            FailedGames = totalFailedGames,
            TotalDeals = totalDeals,
            ElapsedTime = stopwatch.Elapsed,
        };
    }

    private IEnumerable<Task> CreateGameExecutionTasks(
        int numberOfGames,
        BatchExecutionState state,
        IBatchProgressReporter? progressReporter,
        CancellationToken cancellationToken)
    {
        var effectiveParallelism = CalculateEffectiveParallelism();
        var semaphore = new SemaphoreSlim(effectiveParallelism);

        return Enumerable.Range(0, numberOfGames).Select(async gameNumber =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await RunSingleGameAsync(gameNumber, state, progressReporter, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        });
    }

    private int CalculateEffectiveParallelism()
    {
        if (_options.Strategy == ParallelismStrategy.Fixed || _options.MaxDegreeOfParallelism > 0)
        {
            return _options.MaxDegreeOfParallelism;
        }

        var coreCount = Environment.ProcessorCount;
        var baseParallelism = Math.Max(1, coreCount - _options.ReservedCores);

        return _options.Strategy == ParallelismStrategy.Conservative
            ? baseParallelism
            : Math.Min(baseParallelism * 2, _options.MaxThreads);
    }

    private async Task RunSingleGameAsync(
        int gameNumber,
        BatchExecutionState state,
        IBatchProgressReporter? progressReporter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var gameOrchestrator = scope.ServiceProvider.GetRequiredService<IGameOrchestrator>();
            var game = await gameOrchestrator.OrchestrateGameAsync().ConfigureAwait(false);

            await state.ExecuteWithLockAsync(
                () =>
            {
                if (game.WinningTeam == Team.Team1)
                {
                    state.Team1Wins++;
                }
                else if (game.WinningTeam == Team.Team2)
                {
                    state.Team2Wins++;
                }

                state.TotalDeals += game.CompletedDeals.Count;
                state.CompletedGames++;
                state.PendingGames.Add(game);
                progressReporter?.ReportGameCompleted(state.CompletedGames);
            }, cancellationToken).ConfigureAwait(false);

            await SavePendingGamesAsync(state, progressReporter, force: false, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LoggerMessages.LogGameFailed(logger, gameNumber, ex);
            await state.ExecuteWithLockAsync(
                () =>
            {
                state.FailedGames++;
                state.CompletedGames++;
                progressReporter?.ReportGameCompleted(state.CompletedGames);
            }, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task SavePendingGamesAsync(
        BatchExecutionState state,
        IBatchProgressReporter? progressReporter,
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
