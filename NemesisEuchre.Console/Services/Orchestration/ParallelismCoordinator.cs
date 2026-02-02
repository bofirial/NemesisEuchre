using Microsoft.Extensions.Options;

using NemesisEuchre.GameEngine.Options;

namespace NemesisEuchre.Console.Services.Orchestration;

public interface IParallelismCoordinator
{
    int CalculateEffectiveParallelism();

    IEnumerable<Task> CreateParallelTasks<TState>(
        int count,
        TState state,
        Func<int, TState, CancellationToken, Task> taskFactory,
        CancellationToken cancellationToken);
}

public class ParallelismCoordinator(IOptions<GameExecutionOptions> options) : IParallelismCoordinator
{
    private readonly GameExecutionOptions _options = options.Value;

    public int CalculateEffectiveParallelism()
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

    public IEnumerable<Task> CreateParallelTasks<TState>(
        int count,
        TState state,
        Func<int, TState, CancellationToken, Task> taskFactory,
        CancellationToken cancellationToken)
    {
        var effectiveParallelism = CalculateEffectiveParallelism();
        var semaphore = new SemaphoreSlim(effectiveParallelism);

        return Enumerable.Range(0, count).Select(async index =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await taskFactory(index, state, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        });
    }
}
