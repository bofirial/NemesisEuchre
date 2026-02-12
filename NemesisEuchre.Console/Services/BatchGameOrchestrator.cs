using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services.Orchestration;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine;

namespace NemesisEuchre.Console.Services;

public interface IBatchGameOrchestrator
{
    Task<BatchGameResults> RunBatchAsync(
        int numberOfGames,
        IBatchProgressReporter? progressReporter = null,
        GamePersistenceOptions? persistenceOptions = null,
        Actor[]? team1Actors = null,
        Actor[]? team2Actors = null,
        CancellationToken cancellationToken = default);
}

public class BatchGameOrchestrator(
    IServiceScopeFactory serviceScopeFactory,
    IParallelismCoordinator parallelismCoordinator,
    ISubBatchStrategy subBatchStrategy,
    IPersistenceCoordinator persistenceCoordinator,
    ITrainingDataAccumulator trainingDataAccumulator,
    IOptions<PersistenceOptions> persistenceOptions,
    ILogger<BatchGameOrchestrator> logger) : IBatchGameOrchestrator
{
    private readonly PersistenceOptions _persistenceOptions = persistenceOptions.Value;

    public async Task<BatchGameResults> RunBatchAsync(
        int numberOfGames,
        IBatchProgressReporter? progressReporter = null,
        GamePersistenceOptions? persistenceOptions = null,
        Actor[]? team1Actors = null,
        Actor[]? team2Actors = null,
        CancellationToken cancellationToken = default)
    {
        if (numberOfGames <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfGames), "Number of games must be greater than zero.");
        }

        const int maxGamesPerSubBatch = 100_000;
        if (subBatchStrategy.ShouldUseSubBatches(numberOfGames, maxGamesPerSubBatch))
        {
            return await RunBatchesInSubBatchesAsync(numberOfGames, maxGamesPerSubBatch, progressReporter, persistenceOptions, cancellationToken, team1Actors, team2Actors).ConfigureAwait(false);
        }

        var stopwatch = Stopwatch.StartNew();
        var (_, state) = await ExecuteSingleBatchAsync(numberOfGames, progressReporter, persistenceOptions, team1Actors, team2Actors, cancellationToken).ConfigureAwait(false);

        FinalizeIdv(persistenceOptions, state);

        stopwatch.Stop();

        var result = AggregateResults(numberOfGames, state, stopwatch.Elapsed);
        state.Dispose();
        return result;
    }

    private static BatchGameResults AggregateResults(
        int numberOfGames,
        BatchExecutionState state,
        TimeSpan elapsedTime)
    {
        var persistenceDuration = state.PersistenceDuration;
        var idvSaveDuration = state.IdvSaveDuration;
        var playingDuration = elapsedTime - persistenceDuration - idvSaveDuration;

        return new BatchGameResults
        {
            TotalGames = numberOfGames,
            Team1Wins = state.Team1Wins,
            Team2Wins = state.Team2Wins,
            FailedGames = state.FailedGames,
            TotalDeals = state.TotalDeals,
            TotalTricks = state.TotalTricks,
            TotalCallTrumpDecisions = state.TotalCallTrumpDecisions,
            TotalDiscardCardDecisions = state.TotalDiscardCardDecisions,
            TotalPlayCardDecisions = state.TotalPlayCardDecisions,
            ElapsedTime = elapsedTime,
            PlayingDuration = playingDuration,
            PersistenceDuration = persistenceDuration,
            IdvSaveDuration = idvSaveDuration > TimeSpan.Zero ? idvSaveDuration : null,
        };
    }

    private static int GetPlayCardDecisions(GameEngine.Models.Deal d)
    {
        return d.CompletedTricks.Sum(t => t.PlayCardDecisions.Count);
    }

    private void FinalizeIdv(GamePersistenceOptions? persistenceOptions, BatchExecutionState state)
    {
        if (persistenceOptions?.IdvGenerationName != null)
        {
            var idvStopwatch = Stopwatch.StartNew();
            trainingDataAccumulator.Finalize(persistenceOptions.IdvGenerationName);
            idvStopwatch.Stop();
            state.IdvSaveDuration = idvStopwatch.Elapsed;
        }
    }

    private async Task<(BatchGameResults results, BatchExecutionState state)> ExecuteSingleBatchAsync(
        int numberOfGames,
        IBatchProgressReporter? progressReporter,
        GamePersistenceOptions? persistenceOptions,
        Actor[]? team1Actors,
        Actor[]? team2Actors,
        CancellationToken cancellationToken)
    {
        var channelCapacity = _persistenceOptions.BatchSize * 8;
        var state = new BatchExecutionState(channelCapacity);

        var consumerTask = persistenceCoordinator.ConsumeAndPersistAsync(state, persistenceOptions, cancellationToken);

        var effectiveParallelism = parallelismCoordinator.CalculateEffectiveParallelism();
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = effectiveParallelism,
            CancellationToken = cancellationToken,
        };

        await Parallel.ForEachAsync(
            Enumerable.Range(0, numberOfGames),
            parallelOptions,
            async (gameNumber, ct) => await RunSingleGameAsync(gameNumber, state, progressReporter, team1Actors, team2Actors, ct).ConfigureAwait(false)).ConfigureAwait(false);

        state.Writer.Complete();
        await consumerTask.ConfigureAwait(false);

        return (AggregateResults(numberOfGames, state, TimeSpan.Zero), state);
    }

    private async Task<BatchGameResults> RunBatchesInSubBatchesAsync(
        int totalGames,
        int subBatchSize,
        IBatchProgressReporter? progressReporter,
        GamePersistenceOptions? persistenceOptions,
        CancellationToken cancellationToken,
        Actor[]? team1Actors = null,
        Actor[]? team2Actors = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var completedSoFar = 0;
        var totalTeam1Wins = 0;
        var totalTeam2Wins = 0;
        var totalFailedGames = 0;
        var totalDeals = 0;
        var totalTricks = 0;
        var totalCallTrumpDecisions = 0;
        var totalDiscardCardDecisions = 0;
        var totalPlayCardDecisions = 0;
        var totalPersistenceDuration = TimeSpan.Zero;
        var totalIdvSaveDuration = TimeSpan.Zero;

        while (completedSoFar < totalGames)
        {
            var gamesInThisBatch = Math.Min(subBatchSize, totalGames - completedSoFar);

            var subProgressReporter = progressReporter == null ? null : new SubBatchProgressReporter(
                progressReporter,
                completedSoFar);

            var (batchResults, batchState) = await ExecuteSingleBatchAsync(
                gamesInThisBatch,
                subProgressReporter,
                persistenceOptions,
                team1Actors,
                team2Actors,
                cancellationToken).ConfigureAwait(false);

            totalTeam1Wins += batchResults.Team1Wins;
            totalTeam2Wins += batchResults.Team2Wins;
            totalFailedGames += batchResults.FailedGames;
            totalDeals += batchResults.TotalDeals;
            totalTricks += batchResults.TotalTricks;
            totalCallTrumpDecisions += batchResults.TotalCallTrumpDecisions;
            totalDiscardCardDecisions += batchResults.TotalDiscardCardDecisions;
            totalPlayCardDecisions += batchResults.TotalPlayCardDecisions;
            totalPersistenceDuration += batchState.PersistenceDuration;
            totalIdvSaveDuration += batchState.IdvSaveDuration;
            completedSoFar += gamesInThisBatch;

            batchState.Dispose();
        }

        if (persistenceOptions?.IdvGenerationName != null)
        {
            var idvStopwatch = Stopwatch.StartNew();
            trainingDataAccumulator.Finalize(persistenceOptions.IdvGenerationName);
            idvStopwatch.Stop();
            totalIdvSaveDuration += idvStopwatch.Elapsed;
        }

        stopwatch.Stop();

        var playingDuration = stopwatch.Elapsed - totalPersistenceDuration - totalIdvSaveDuration;

        return new BatchGameResults
        {
            TotalGames = totalGames,
            Team1Wins = totalTeam1Wins,
            Team2Wins = totalTeam2Wins,
            FailedGames = totalFailedGames,
            TotalDeals = totalDeals,
            TotalTricks = totalTricks,
            TotalCallTrumpDecisions = totalCallTrumpDecisions,
            TotalDiscardCardDecisions = totalDiscardCardDecisions,
            TotalPlayCardDecisions = totalPlayCardDecisions,
            ElapsedTime = stopwatch.Elapsed,
            PlayingDuration = playingDuration,
            PersistenceDuration = totalPersistenceDuration,
            IdvSaveDuration = totalIdvSaveDuration > TimeSpan.Zero ? totalIdvSaveDuration : null,
        };
    }

    private async Task RunSingleGameAsync(
        int gameNumber,
        BatchExecutionState state,
        IBatchProgressReporter? progressReporter,
        Actor[]? team1Actors = null,
        Actor[]? team2Actors = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var gameOrchestrator = scope.ServiceProvider.GetRequiredService<IGameOrchestrator>();
            var game = await gameOrchestrator.OrchestrateGameAsync(team1Actors, team2Actors).ConfigureAwait(false);

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
                    state.TotalTricks += game.CompletedDeals.Sum(d => d.CompletedTricks.Count);
                    state.TotalCallTrumpDecisions += game.CompletedDeals.Sum(d => d.CallTrumpDecisions.Count);
                    state.TotalDiscardCardDecisions += game.CompletedDeals.Sum(d => d.DiscardCardDecisions.Count);
                    state.TotalPlayCardDecisions += game.CompletedDeals.Sum(d => GetPlayCardDecisions(d));
                    state.CompletedGames++;
                    progressReporter?.ReportGameCompleted(state.CompletedGames);
                },
                cancellationToken).ConfigureAwait(false);

            await state.Writer.WriteAsync(game, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Foundation.LoggerMessages.LogGameFailed(logger, gameNumber, ex);
            await state.ExecuteWithLockAsync(
                () =>
                    {
                        state.FailedGames++;
                        state.CompletedGames++;
                        progressReporter?.ReportGameCompleted(state.CompletedGames);
                    },
                cancellationToken).ConfigureAwait(false);
        }
    }
}
