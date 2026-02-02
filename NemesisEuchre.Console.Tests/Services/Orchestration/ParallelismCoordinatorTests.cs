using FluentAssertions;

using Microsoft.Extensions.Options;

using NemesisEuchre.Console.Services;
using NemesisEuchre.Console.Services.Orchestration;
using NemesisEuchre.GameEngine.Options;

namespace NemesisEuchre.Console.Tests.Services.Orchestration;

public class ParallelismCoordinatorTests
{
    private readonly ParallelismCoordinator _coordinator;

    public ParallelismCoordinatorTests()
    {
        var options = Options.Create(new GameExecutionOptions
        {
            MaxDegreeOfParallelism = 4,
        });

        _coordinator = new ParallelismCoordinator(options);
    }

    [Fact]
    public void CalculateEffectiveParallelism_ReturnsConfiguredValue()
    {
        var result = _coordinator.CalculateEffectiveParallelism();

        result.Should().Be(4);
    }

    [Fact]
    public async Task CreateParallelTasks_CreatesCorrectNumberOfTasks()
    {
        var state = new BatchExecutionState(100);
        var executedCount = 0;

        var tasks = _coordinator.CreateParallelTasks(
            10,
            state,
            async (_, _, ct) =>
            {
                Interlocked.Increment(ref executedCount);
                await Task.Delay(10, ct);
            },
            CancellationToken.None);

        await Task.WhenAll(tasks);

        executedCount.Should().Be(10);
    }

    [Fact]
    public async Task CreateParallelTasks_RespectsSemaphoreLimit()
    {
        var state = new BatchExecutionState(100);
        var maxConcurrent = 0;
        var currentConcurrent = 0;
        var lockObj = new object();

        var tasks = _coordinator.CreateParallelTasks(
            20,
            state,
            async (_, _, ct) =>
            {
                lock (lockObj)
                {
                    currentConcurrent++;
                    if (currentConcurrent > maxConcurrent)
                    {
                        maxConcurrent = currentConcurrent;
                    }
                }

                await Task.Delay(50, ct);

                lock (lockObj)
                {
                    currentConcurrent--;
                }
            },
            CancellationToken.None);

        await Task.WhenAll(tasks);

        maxConcurrent.Should().BeLessThanOrEqualTo(4, "should respect semaphore limit");
    }

    [Fact]
    public async Task CreateParallelTasks_HandlesExceptionsInTasks()
    {
        var state = new BatchExecutionState(100);
        var successfulTasks = 0;

        var tasks = _coordinator.CreateParallelTasks(
            10,
            state,
            async (index, _, ct) =>
            {
                await Task.Delay(10, ct);
                if (index == 5)
                {
                    throw new InvalidOperationException("Test exception");
                }

                Interlocked.Increment(ref successfulTasks);
            },
            CancellationToken.None);

        var act = async () => await Task.WhenAll(tasks);

        await act.Should().ThrowAsync<InvalidOperationException>();
        successfulTasks.Should().Be(9);
    }

    [Fact]
    public async Task CreateParallelTasks_RespectsCancellationToken()
    {
        var state = new BatchExecutionState(100);
        var cts = new CancellationTokenSource();
        var executedCount = 0;

        cts.CancelAfter(50);

        var tasks = _coordinator.CreateParallelTasks(
            100,
            state,
            async (_, _, ct) =>
            {
                Interlocked.Increment(ref executedCount);
                await Task.Delay(100, ct);
            },
            cts.Token);

        var act = async () => await Task.WhenAll(tasks);

        await act.Should().ThrowAsync<OperationCanceledException>();
        executedCount.Should().BeLessThan(100, "should not complete all tasks");

        cts.Dispose();
    }

    [Fact]
    public async Task CreateParallelTasks_PassesCorrectIndex()
    {
        var state = new BatchExecutionState(100);
        var indices = new List<int>();
        var lockObj = new object();

        var tasks = _coordinator.CreateParallelTasks(
            5,
            state,
            async (index, _, ct) =>
            {
                await Task.Delay(10, ct);
                lock (lockObj)
                {
                    indices.Add(index);
                }
            },
            CancellationToken.None);

        await Task.WhenAll(tasks);

        indices.Should().BeEquivalentTo([0, 1, 2, 3, 4]);
    }

    [Fact]
    public async Task CreateParallelTasks_PassesStateToAction()
    {
        var state = new BatchExecutionState(100);
        BatchExecutionState? capturedState = null;

        var tasks = _coordinator.CreateParallelTasks(
            1,
            state,
            async (_, s, ct) =>
            {
                await Task.Delay(10, ct);
                capturedState = s;
            },
            CancellationToken.None);

        await Task.WhenAll(tasks);

        capturedState.Should().BeSameAs(state);
    }
}
