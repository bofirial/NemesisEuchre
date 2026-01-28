using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Models;
using NemesisEuchre.GameEngine;
using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.Console.Services;

public interface IBatchGameOrchestrator
{
    Task<BatchGameResults> RunBatchAsync(
        int numberOfGames,
        int maxConcurrentGames = 4,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);
}

public partial class BatchGameOrchestrator(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<BatchGameOrchestrator> logger) : IBatchGameOrchestrator
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<BatchGameOrchestrator> _logger = logger;

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
            using var scope = _serviceScopeFactory.CreateScope();
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

                state.CompletedGames++;
                progress?.Report(state.CompletedGames);
            }
        }
        catch (Exception ex)
        {
            LogGameFailed(gameNumber, ex);
            lock (state.LockObject)
            {
                state.FailedGames++;
                state.CompletedGames++;
                progress?.Report(state.CompletedGames);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Game {GameNumber} failed")]
    private partial void LogGameFailed(int gameNumber, Exception exception);

    private sealed class BatchState
    {
        public object LockObject { get; } = new();

        public int Team1Wins { get; set; }

        public int Team2Wins { get; set; }

        public int FailedGames { get; set; }

        public int CompletedGames { get; set; }
    }
}
