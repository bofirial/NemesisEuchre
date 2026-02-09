using FluentAssertions;

using NemesisEuchre.Console.Services;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Tests.Services;

public class BatchExecutionStateTests
{
    [Fact]
    public void Constructor_InitializesCountersToZero()
    {
        using var state = new BatchExecutionState(10);

        state.Team1Wins.Should().Be(0);
        state.Team2Wins.Should().Be(0);
        state.FailedGames.Should().Be(0);
        state.CompletedGames.Should().Be(0);
        state.TotalDeals.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteWithLockAsync_ExecutesActionSafely()
    {
        using var state = new BatchExecutionState(10);
        var counter = 0;

        await state.ExecuteWithLockAsync(() => counter++, cancellationToken: TestContext.Current.CancellationToken);

        counter.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteWithLockAsync_WithReturnValue_ReturnsValue()
    {
        using var state = new BatchExecutionState(10);

        var result = await state.ExecuteWithLockAsync(() => 42, cancellationToken: TestContext.Current.CancellationToken);

        result.Should().Be(42);
    }

    [Fact]
    public void Team1Wins_CanBeIncremented()
    {
        using var state = new BatchExecutionState(10);

        state.Team1Wins++;
        state.Team1Wins++;

        state.Team1Wins.Should().Be(2);
    }

    [Fact]
    public void Team2Wins_CanBeIncremented()
    {
        using var state = new BatchExecutionState(10);

        state.Team2Wins++;
        state.Team2Wins++;
        state.Team2Wins++;

        state.Team2Wins.Should().Be(3);
    }

    [Fact]
    public void FailedGames_CanBeIncremented()
    {
        using var state = new BatchExecutionState(10);

        state.FailedGames++;

        state.FailedGames.Should().Be(1);
    }

    [Fact]
    public void CompletedGames_CanBeIncremented()
    {
        using var state = new BatchExecutionState(10);

        state.CompletedGames++;
        state.CompletedGames++;
        state.CompletedGames++;
        state.CompletedGames++;

        state.CompletedGames.Should().Be(4);
    }

    [Fact]
    public void TotalDeals_CanBeIncremented()
    {
        using var state = new BatchExecutionState(10);

        state.TotalDeals += 5;
        state.TotalDeals += 3;

        state.TotalDeals.Should().Be(8);
    }

    [Fact]
    public async Task Channel_CanWriteAndReadGames()
    {
        using var state = new BatchExecutionState(10);
        var game = new Game();

        await state.Writer.WriteAsync(game, TestContext.Current.CancellationToken);
        state.Writer.Complete();

        var items = new List<Game>();
        await foreach (var item in state.Reader.ReadAllAsync(TestContext.Current.CancellationToken))
        {
            items.Add(item);
        }

        items.Should().HaveCount(1);
        items[0].Should().Be(game);
    }

    [Fact]
    public async Task Channel_RespectsCapacity()
    {
        using var state = new BatchExecutionState(2);

        await state.Writer.WriteAsync(new Game(), TestContext.Current.CancellationToken);
        await state.Writer.WriteAsync(new Game(), TestContext.Current.CancellationToken);

        var writeTask = state.Writer.WriteAsync(new Game(), TestContext.Current.CancellationToken);

        writeTask.IsCompleted.Should().BeFalse("channel is bounded at capacity 2");

        await state.Reader.ReadAsync(TestContext.Current.CancellationToken);
        await writeTask;
    }

    [Fact]
    public async Task Channel_WriterComplete_SignalsNoMoreData()
    {
        using var state = new BatchExecutionState(10);
        await state.Writer.WriteAsync(new Game(), TestContext.Current.CancellationToken);
        state.Writer.Complete();

        var items = new List<Game>();
        await foreach (var item in state.Reader.ReadAllAsync(TestContext.Current.CancellationToken))
        {
            items.Add(item);
        }

        items.Should().HaveCount(1);
        state.Reader.Completion.IsCompleted.Should().BeTrue();
    }
}
