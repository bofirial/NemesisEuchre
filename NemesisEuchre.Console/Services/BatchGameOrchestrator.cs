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
        IProgress<int>? progress = null,
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
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (numberOfGames <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfGames), "Number of games must be greater than zero.");
        }

        var stopwatch = Stopwatch.StartNew();
        var semaphore = new SemaphoreSlim(_options.MaxDegreeOfParallelism);
        var state = new BatchState(_persistenceOptions.BatchSize);

        var tasks = Enumerable.Range(0, numberOfGames).Select(async gameNumber =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                await RunSingleGameAsync(gameNumber, state, progress, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        await SavePendingGamesAsync(state, force: true, cancellationToken);
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
        IProgress<int>? progress,
        CancellationToken cancellationToken = default)
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
                state.PendingGames.Add(game);
                progress?.Report(state.CompletedGames);
            }

            await SavePendingGamesAsync(state, force: false, cancellationToken);
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

    private async Task SavePendingGamesAsync(
        BatchState state,
        bool force,
        CancellationToken cancellationToken = default)
    {
        List<Game>? gamesToSave = null;

        lock (state.LockObject)
        {
            if ((force && state.PendingGames.Count > 0) ||
                state.PendingGames.Count >= state.BatchSize)
            {
                gamesToSave = [.. state.PendingGames];
                state.PendingGames.Clear();
            }
        }

        if (gamesToSave?.Count > 0)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var gameRepository = scope.ServiceProvider.GetRequiredService<IGameRepository>();
                await gameRepository.SaveCompletedGamesAsync(gamesToSave, cancellationToken);
            }
            catch (Exception ex)
            {
                LoggerMessages.LogGamePersistenceFailed(logger, ex);
            }
        }
    }

    private sealed class BatchState(int batchSize)
    {
        public object LockObject { get; } = new();

        public int BatchSize { get; } = batchSize;

        public List<Game> PendingGames { get; } = [];

        public int Team1Wins { get; set; }

        public int Team2Wins { get; set; }

        public int FailedGames { get; set; }

        public int CompletedGames { get; set; }

        public int TotalDeals { get; set; }
    }
}
