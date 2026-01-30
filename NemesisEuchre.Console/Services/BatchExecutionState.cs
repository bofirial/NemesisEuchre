using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Services;

internal sealed class BatchExecutionState(int batchSize) : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public int BatchSize { get; } = batchSize;

    public List<Game> PendingGames { get; } = [];

    public int Team1Wins { get; set; }

    public int Team2Wins { get; set; }

    public int FailedGames { get; set; }

    public int CompletedGames { get; set; }

    public int SavedGames { get; set; }

    public int TotalDeals { get; set; }

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

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}
