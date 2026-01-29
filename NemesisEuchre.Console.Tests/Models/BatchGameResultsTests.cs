using FluentAssertions;

using NemesisEuchre.Console.Models;

namespace NemesisEuchre.Console.Tests.Models;

public class BatchGameResultsTests
{
    [Fact]
    public void Team1WinRate_WhenAllTeam1Wins_Returns100Percent()
    {
        var results = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 10,
            Team2Wins = 0,
            FailedGames = 0,
            TotalDeals = 50,
            ElapsedTime = TimeSpan.FromSeconds(5),
        };

        results.Team1WinRate.Should().Be(1.0);
    }

    [Fact]
    public void Team2WinRate_WhenAllTeam2Wins_Returns100Percent()
    {
        var results = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 0,
            Team2Wins = 10,
            FailedGames = 0,
            TotalDeals = 50,
            ElapsedTime = TimeSpan.FromSeconds(5),
        };

        results.Team2WinRate.Should().Be(1.0);
    }

    [Fact]
    public void Team1WinRate_WhenZeroGames_ReturnsZero()
    {
        var results = new BatchGameResults
        {
            TotalGames = 0,
            Team1Wins = 0,
            Team2Wins = 0,
            FailedGames = 0,
            TotalDeals = 0,
            ElapsedTime = TimeSpan.Zero,
        };

        results.Team1WinRate.Should().Be(0.0);
    }

    [Fact]
    public void Team2WinRate_WhenZeroGames_ReturnsZero()
    {
        var results = new BatchGameResults
        {
            TotalGames = 0,
            Team1Wins = 0,
            Team2Wins = 0,
            FailedGames = 0,
            TotalDeals = 0,
            ElapsedTime = TimeSpan.Zero,
        };

        results.Team2WinRate.Should().Be(0.0);
    }

    [Fact]
    public void Team1WinRate_WhenOnlyFailedGames_ReturnsZero()
    {
        var results = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 0,
            Team2Wins = 0,
            FailedGames = 10,
            TotalDeals = 0,
            ElapsedTime = TimeSpan.FromSeconds(5),
        };

        results.Team1WinRate.Should().Be(0.0);
    }

    [Fact]
    public void Team2WinRate_WhenOnlyFailedGames_ReturnsZero()
    {
        var results = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 0,
            Team2Wins = 0,
            FailedGames = 10,
            TotalDeals = 0,
            ElapsedTime = TimeSpan.FromSeconds(5),
        };

        results.Team2WinRate.Should().Be(0.0);
    }

    [Fact]
    public void WinRates_WithMixedResults_CalculatesCorrectly()
    {
        var results = new BatchGameResults
        {
            TotalGames = 100,
            Team1Wins = 60,
            Team2Wins = 40,
            FailedGames = 0,
            TotalDeals = 500,
            ElapsedTime = TimeSpan.FromMinutes(1),
        };

        results.Team1WinRate.Should().BeApproximately(0.6, 0.0001);
        results.Team2WinRate.Should().BeApproximately(0.4, 0.0001);
    }

    [Fact]
    public void WinRates_WithMixedResultsAndFailures_IgnoresFailedGames()
    {
        var results = new BatchGameResults
        {
            TotalGames = 110,
            Team1Wins = 60,
            Team2Wins = 40,
            FailedGames = 10,
            TotalDeals = 500,
            ElapsedTime = TimeSpan.FromMinutes(1),
        };

        results.Team1WinRate.Should().BeApproximately(0.6, 0.0001);
        results.Team2WinRate.Should().BeApproximately(0.4, 0.0001);
    }

    [Fact]
    public void Team1WinRate_WithEvenSplit_Returns50Percent()
    {
        var results = new BatchGameResults
        {
            TotalGames = 100,
            Team1Wins = 50,
            Team2Wins = 50,
            FailedGames = 0,
            TotalDeals = 500,
            ElapsedTime = TimeSpan.FromMinutes(1),
        };

        results.Team1WinRate.Should().Be(0.5);
    }

    [Fact]
    public void Team2WinRate_WithEvenSplit_Returns50Percent()
    {
        var results = new BatchGameResults
        {
            TotalGames = 100,
            Team1Wins = 50,
            Team2Wins = 50,
            FailedGames = 0,
            TotalDeals = 500,
            ElapsedTime = TimeSpan.FromMinutes(1),
        };

        results.Team2WinRate.Should().Be(0.5);
    }
}
