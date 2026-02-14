using System.Threading.Channels;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Services;

public sealed class BatchExecutionState(int channelCapacity) : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private readonly Channel<Game> _channel = Channel.CreateBounded<Game>(
        new BoundedChannelOptions(channelCapacity)
        {
            SingleWriter = false,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait,
        });

    public ChannelWriter<Game> Writer => _channel.Writer;

    public ChannelReader<Game> Reader => _channel.Reader;

    public int Team1Wins { get; set; }

    public int Team2Wins { get; set; }

    public int FailedGames { get; set; }

    public int CompletedGames { get; set; }

    public int SavedGames { get; set; }

    public int TotalDeals { get; set; }

    public int TotalTricks { get; set; }

    public int TotalCallTrumpDecisions { get; set; }

    public int TotalDiscardCardDecisions { get; set; }

    public int TotalPlayCardDecisions { get; set; }

    public TimeSpan PersistenceDuration { get; set; }

    public TimeSpan IdvSaveDuration { get; set; }

    public async Task<T> ExecuteWithLockAsync<T>(Func<T> action, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return action();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ExecuteWithLockAsync(Action action, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            action();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task RecordGameCompletionAsync(
        Game game,
        Func<BatchProgressSnapshot> snapshotFactory,
        Action<BatchProgressSnapshot>? progressReporter,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWithLockAsync(
            () =>
            {
                if (game.WinningTeam == Team.Team1)
                {
                    Team1Wins++;
                }
                else if (game.WinningTeam == Team.Team2)
                {
                    Team2Wins++;
                }

                TotalDeals += game.CompletedDeals.Count;
                TotalTricks += game.CompletedDeals.Sum(d => d.CompletedTricks.Count);
                TotalCallTrumpDecisions += game.CompletedDeals.Sum(d => d.CallTrumpDecisions.Count);
                TotalDiscardCardDecisions += game.CompletedDeals.Sum(d => d.DiscardCardDecisions.Count);
                TotalPlayCardDecisions += game.CompletedDeals.Sum(d => d.CompletedTricks.Sum(t => t.PlayCardDecisions.Count));
                CompletedGames++;
                progressReporter?.Invoke(snapshotFactory());
            },
            cancellationToken);
    }

    public Task RecordGameFailureAsync(
        Func<BatchProgressSnapshot> snapshotFactory,
        Action<BatchProgressSnapshot>? progressReporter,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWithLockAsync(
            () =>
            {
                FailedGames++;
                CompletedGames++;
                progressReporter?.Invoke(snapshotFactory());
            },
            cancellationToken);
    }

    public async Task WriteGameAsync(Game game, CancellationToken cancellationToken = default)
    {
        await Writer.WriteAsync(game, cancellationToken).ConfigureAwait(false);
    }

    public void CompleteWriting()
    {
        Writer.Complete();
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}
