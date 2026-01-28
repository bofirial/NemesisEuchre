using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Models;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine;

namespace NemesisEuchre.Console.Services;

public interface IBatchGameOrchestrator
{
    Task<BatchGameResults> RunBatchAsync(
        int numberOfGames,
        int maxConcurrentGames = 4,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);
}

public class BatchGameOrchestrator(
    IServiceScopeFactory serviceScopeFactory,
    IGameRepository gameRepository,
    ILogger<BatchGameOrchestrator> logger) : IBatchGameOrchestrator
{
    public async Task<BatchGameResults> RunBatchAsync(
        int numberOfGames,
        int maxConcurrentGames = 4,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (numberOfGames <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfGames), "Number of games must be greater than zero.");
        }

        if (maxConcurrentGames <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxConcurrentGames), "Max concurrent games must be greater than zero.");
        }

        var stopwatch = Stopwatch.StartNew();
        var semaphore = new SemaphoreSlim(maxConcurrentGames);
        var state = new BatchState();

        var tasks = Enumerable.Range(0, numberOfGames).Select(async gameNumber =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                await RunSingleGameAsync(gameNumber, state, progress);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        return new BatchGameResults
        {
            TotalGames = numberOfGames,
            Team1Wins = state.Team1Wins,
            Team2Wins = state.Team2Wins,
            FailedGames = state.FailedGames,
            TotalDeals = state.TotalDeals,
            ElapsedTime = stopwatch.Elapsed,
        };
    }

    private async Task RunSingleGameAsync(
        int gameNumber,
        BatchState state,
        IProgress<int>? progress)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var gameOrchestrator = scope.ServiceProvider.GetRequiredService<IGameOrchestrator>();
            var game = await gameOrchestrator.OrchestrateGameAsync();

            lock (state.LockObject)
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
                progress?.Report(state.CompletedGames);
            }

            try
            {
                await gameRepository.SaveCompletedGameAsync(game);
            }
            catch (Exception ex)
            {
                LoggerMessages.LogGamePersistenceFailed(logger, ex);
            }
        }
        catch (Exception ex)
        {
            LoggerMessages.LogGameFailed(logger, gameNumber, ex);
            lock (state.LockObject)
            {
                state.FailedGames++;
                state.CompletedGames++;
                progress?.Report(state.CompletedGames);
            }
        }
    }

    private sealed class BatchState
    {
        public object LockObject { get; } = new();

        public int Team1Wins { get; set; }

        public int Team2Wins { get; set; }

        public int FailedGames { get; set; }

        public int CompletedGames { get; set; }

        public int TotalDeals { get; set; }
    }
}
