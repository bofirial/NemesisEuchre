using FluentAssertions;

using NemesisEuchre.Console.Services;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Tests.Services;

public class BatchExecutionStateTests
{
    [Fact]
    public void Constructor_InitializesBatchSize()
    {
        using var state = new BatchExecutionState(100);

        state.BatchSize.Should().Be(100);
    }

    [Fact]
    public void Constructor_InitializesEmptyPendingGames()
    {
        using var state = new BatchExecutionState(100);

        state.PendingGames.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_InitializesCountersToZero()
    {
        using var state = new BatchExecutionState(100);

        state.Team1Wins.Should().Be(0);
        state.Team2Wins.Should().Be(0);
        state.FailedGames.Should().Be(0);
        state.CompletedGames.Should().Be(0);
        state.TotalDeals.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteWithLockAsync_ExecutesActionSafely()
    {
        using var state = new BatchExecutionState(100);
        var counter = 0;

        await state.ExecuteWithLockAsync(() => counter++);

        counter.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteWithLockAsync_WithReturnValue_ReturnsValue()
    {
        using var state = new BatchExecutionState(100);

        var result = await state.ExecuteWithLockAsync(() => 42);

        result.Should().Be(42);
    }

    [Fact]
    public void Team1Wins_CanBeIncremented()
    {
        using var state = new BatchExecutionState(100);

        state.Team1Wins++;
        state.Team1Wins++;

        state.Team1Wins.Should().Be(2);
    }

    [Fact]
    public void Team2Wins_CanBeIncremented()
    {
        using var state = new BatchExecutionState(100);

        state.Team2Wins++;
        state.Team2Wins++;
        state.Team2Wins++;

        state.Team2Wins.Should().Be(3);
    }

    [Fact]
    public void FailedGames_CanBeIncremented()
    {
        using var state = new BatchExecutionState(100);

        state.FailedGames++;

        state.FailedGames.Should().Be(1);
    }

    [Fact]
    public void CompletedGames_CanBeIncremented()
    {
        using var state = new BatchExecutionState(100);

        state.CompletedGames++;
        state.CompletedGames++;
        state.CompletedGames++;
        state.CompletedGames++;

        state.CompletedGames.Should().Be(4);
    }

    [Fact]
    public void TotalDeals_CanBeIncremented()
    {
        using var state = new BatchExecutionState(100);

        state.TotalDeals += 5;
        state.TotalDeals += 3;

        state.TotalDeals.Should().Be(8);
    }

    [Fact]
    public void PendingGames_CanAddGames()
    {
        using var state = new BatchExecutionState(100);
        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 5,
            WinningTeam = Team.Team1,
        };

        state.PendingGames.Add(game);

        state.PendingGames.Should().HaveCount(1);
        state.PendingGames[0].Should().Be(game);
    }

    [Fact]
    public void PendingGames_CanBeClearedAfterBatch()
    {
        using var state = new BatchExecutionState(2);
        var game1 = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 5,
            WinningTeam = Team.Team1,
        };
        var game2 = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 8,
            Team2Score = 10,
            WinningTeam = Team.Team2,
        };

        state.PendingGames.Add(game1);
        state.PendingGames.Add(game2);
        state.PendingGames.Clear();

        state.PendingGames.Should().BeEmpty();
    }
}
