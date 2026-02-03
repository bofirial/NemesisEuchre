using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services.Orchestration;
using NemesisEuchre.Console.Services.Persistence;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine;

namespace NemesisEuchre.Console.Services;

public interface IBatchGameOrchestrator
{
    Task<BatchGameResults> RunBatchAsync(
        int numberOfGames,
        IBatchProgressReporter? progressReporter = null,
        bool doNotPersist = false,
        ActorType[]? team1ActorTypes = null,
        ActorType[]? team2ActorTypes = null,
        CancellationToken cancellationToken = default);
}

public class BatchGameOrchestrator(
    IServiceScopeFactory serviceScopeFactory,
    IParallelismCoordinator parallelismCoordinator,
    ISubBatchStrategy subBatchStrategy,
    IPersistenceCoordinator persistenceCoordinator,
    IOptions<PersistenceOptions> persistenceOptions,
    ILogger<BatchGameOrchestrator> logger) : IBatchGameOrchestrator
{
    private readonly PersistenceOptions _persistenceOptions = persistenceOptions.Value;

    public async Task<BatchGameResults> RunBatchAsync(
        int numberOfGames,
        IBatchProgressReporter? progressReporter = null,
        bool doNotPersist = false,
        ActorType[]? team1ActorTypes = null,
        ActorType[]? team2ActorTypes = null,
        CancellationToken cancellationToken = default)
    {
        if (numberOfGames <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfGames), "Number of games must be greater than zero.");
        }

        const int maxGamesPerSubBatch = 10000;
        if (subBatchStrategy.ShouldUseSubBatches(numberOfGames, maxGamesPerSubBatch))
        {
            return await RunBatchesInSubBatchesAsync(numberOfGames, maxGamesPerSubBatch, progressReporter, doNotPersist, cancellationToken, team1ActorTypes, team2ActorTypes).ConfigureAwait(false);
        }

        var stopwatch = Stopwatch.StartNew();
        using var state = new BatchExecutionState(_persistenceOptions.BatchSize);
        var tasks = parallelismCoordinator.CreateParallelTasks(
            numberOfGames,
            state,
            (gameNumber, s, ct) => RunSingleGameAsync(gameNumber, s, progressReporter, doNotPersist, team1ActorTypes, team2ActorTypes, ct),
            cancellationToken);

        await Task.WhenAll(tasks).ConfigureAwait(false);
        await persistenceCoordinator.SavePendingGamesAsync(state, progressReporter, doNotPersist, force: true, cancellationToken).ConfigureAwait(false);
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
        bool doNotPersist,
        CancellationToken cancellationToken,
        ActorType[]? team1ActorTypes = null,
        ActorType[]? team2ActorTypes = null)
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
            var tasks = parallelismCoordinator.CreateParallelTasks(
                gamesInThisBatch,
                state,
                (gameNumber, s, ct) => RunSingleGameAsync(gameNumber, s, subProgressReporter, doNotPersist, team1ActorTypes, team2ActorTypes, ct),
                cancellationToken);

            await Task.WhenAll(tasks).ConfigureAwait(false);
            await persistenceCoordinator.SavePendingGamesAsync(state, subProgressReporter, doNotPersist, force: true, cancellationToken).ConfigureAwait(false);

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

    private async Task RunSingleGameAsync(
        int gameNumber,
        BatchExecutionState state,
        IBatchProgressReporter? progressReporter,
        bool doNotPersist,
        ActorType[]? team1ActorTypes = null,
        ActorType[]? team2ActorTypes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var gameOrchestrator = scope.ServiceProvider.GetRequiredService<IGameOrchestrator>();
            var game = await gameOrchestrator.OrchestrateGameAsync(team1ActorTypes, team2ActorTypes).ConfigureAwait(false);

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

            await persistenceCoordinator.SavePendingGamesAsync(state, progressReporter, doNotPersist, force: false, cancellationToken).ConfigureAwait(false);
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
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}
