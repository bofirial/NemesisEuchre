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
        var results = await ExecuteSingleBatchAsync(numberOfGames, progressReporter, doNotPersist, team1ActorTypes, team2ActorTypes, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();

        return new BatchGameResults
        {
            TotalGames = results.TotalGames,
            Team1Wins = results.Team1Wins,
            Team2Wins = results.Team2Wins,
            FailedGames = results.FailedGames,
            TotalDeals = results.TotalDeals,
            ElapsedTime = stopwatch.Elapsed,
        };
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

    private async Task<BatchGameResults> ExecuteSingleBatchAsync(
        int numberOfGames,
        IBatchProgressReporter? progressReporter,
        bool doNotPersist,
        ActorType[]? team1ActorTypes,
        ActorType[]? team2ActorTypes,
        CancellationToken cancellationToken)
    {
        using var state = new BatchExecutionState(_persistenceOptions.BatchSize);
        var tasks = parallelismCoordinator.CreateParallelTasks(
            numberOfGames,
            state,
            (gameNumber, s, ct) => RunSingleGameAsync(gameNumber, s, progressReporter, doNotPersist, team1ActorTypes, team2ActorTypes, ct),
            cancellationToken);

        await Task.WhenAll(tasks).ConfigureAwait(false);
        await persistenceCoordinator.SavePendingGamesAsync(state, progressReporter, doNotPersist, force: true, cancellationToken).ConfigureAwait(false);

        return AggregateResults(numberOfGames, state, TimeSpan.Zero);
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

            var batchResults = await ExecuteSingleBatchAsync(
                gamesInThisBatch,
                subProgressReporter,
                doNotPersist,
                team1ActorTypes,
                team2ActorTypes,
                cancellationToken).ConfigureAwait(false);

            totalTeam1Wins += batchResults.Team1Wins;
            totalTeam2Wins += batchResults.Team2Wins;
            totalFailedGames += batchResults.FailedGames;
            totalDeals += batchResults.TotalDeals;
            completedSoFar += gamesInThisBatch;
            savedSoFar += batchResults.TotalGames;
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
