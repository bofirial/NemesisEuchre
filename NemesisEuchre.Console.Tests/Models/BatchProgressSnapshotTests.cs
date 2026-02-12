using FluentAssertions;

using NemesisEuchre.Console.Models;

namespace NemesisEuchre.Console.Tests.Models;

public class BatchProgressSnapshotTests
{
    [Fact]
    public void Empty_ReturnsAllZeros()
    {
        var snapshot = BatchProgressSnapshot.Empty;

        snapshot.CompletedGames.Should().Be(0);
        snapshot.Team1Wins.Should().Be(0);
        snapshot.Team2Wins.Should().Be(0);
        snapshot.FailedGames.Should().Be(0);
        snapshot.TotalDeals.Should().Be(0);
        snapshot.TotalTricks.Should().Be(0);
        snapshot.TotalCallTrumpDecisions.Should().Be(0);
        snapshot.TotalDiscardCardDecisions.Should().Be(0);
        snapshot.TotalPlayCardDecisions.Should().Be(0);
    }

    [Fact]
    public void Add_CombinesBothSnapshots()
    {
        var a = new BatchProgressSnapshot(10, 6, 3, 1, 50, 200, 80, 10, 600);
        var b = new BatchProgressSnapshot(5, 2, 2, 1, 25, 100, 40, 5, 300);

        var result = a.Add(b);

        result.CompletedGames.Should().Be(15);
        result.Team1Wins.Should().Be(8);
        result.Team2Wins.Should().Be(5);
        result.FailedGames.Should().Be(2);
        result.TotalDeals.Should().Be(75);
        result.TotalTricks.Should().Be(300);
        result.TotalCallTrumpDecisions.Should().Be(120);
        result.TotalDiscardCardDecisions.Should().Be(15);
        result.TotalPlayCardDecisions.Should().Be(900);
    }

    [Fact]
    public void Add_WithEmpty_ReturnsOriginal()
    {
        var snapshot = new BatchProgressSnapshot(10, 6, 3, 1, 50, 200, 80, 10, 600);

        var result = snapshot.Add(BatchProgressSnapshot.Empty);

        result.Should().Be(snapshot);
    }

    [Fact]
    public void Empty_Add_ReturnsOther()
    {
        var other = new BatchProgressSnapshot(5, 2, 2, 1, 25, 100, 40, 5, 300);

        var result = BatchProgressSnapshot.Empty.Add(other);

        result.Should().Be(other);
    }
}
