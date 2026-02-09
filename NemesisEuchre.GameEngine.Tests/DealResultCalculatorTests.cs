using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.GameEngine.Tests;

public class DealResultCalculatorTests
{
    private readonly DealResultCalculator _calculator = new(new DealResultValidator());

    [Fact]
    public void CalculateDealResult_WithNullDeal_ThrowsArgumentNullException()
    {
        var act = () => _calculator.CalculateDealResult(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CalculateDealResult_WithNoCompletedTricks_ThrowsInvalidOperationException()
    {
        var deal = CreateTestDeal(PlayerPosition.North, false);

        var act = () => _calculator.CalculateDealResult(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Deal must have exactly 5 completed tricks");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(7)]
    public void CalculateDealResult_WithWrongNumberOfTricks_ThrowsInvalidOperationException(int trickCount)
    {
        var deal = CreateTestDeal(PlayerPosition.North, false, [.. Enumerable.Repeat(Team.Team1, trickCount)]);

        var act = () => _calculator.CalculateDealResult(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Deal must have exactly 5 completed tricks");
    }

    [Fact]
    public void CalculateDealResult_WithMissingCallingPlayer_ThrowsInvalidOperationException()
    {
        var deal = new Deal
        {
            CallingPlayer = null,
            CompletedTricks =
            [
                new Trick { WinningTeam = Team.Team1 },
                new Trick { WinningTeam = Team.Team1 },
                new Trick { WinningTeam = Team.Team1 },
                new Trick { WinningTeam = Team.Team2 },
                new Trick { WinningTeam = Team.Team2 },
            ],
        };

        var act = () => _calculator.CalculateDealResult(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Deal must have a CallingPlayer set");
    }

    [Fact]
    public void CalculateDealResult_WithTrickMissingWinningTeam_ThrowsInvalidOperationException()
    {
        var deal = new Deal
        {
            CallingPlayer = PlayerPosition.North,
            CompletedTricks =
            [
                new Trick { WinningTeam = Team.Team1 },
                new Trick { WinningTeam = Team.Team1 },
                new Trick { WinningTeam = null },
                new Trick { WinningTeam = Team.Team2 },
                new Trick { WinningTeam = Team.Team2 },
            ],
        };

        var act = () => _calculator.CalculateDealResult(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("All completed tricks must have WinningTeam set");
    }

    [Fact]
    public void CalculateDealResult_CallingTeamWinsExactly3Tricks_ReturnsWonStandardBid()
    {
        var deal = CreateTestDeal(PlayerPosition.North, false, Team.Team1, Team.Team1, Team.Team1, Team.Team2, Team.Team2);

        var (dealResult, winningTeam) = _calculator.CalculateDealResult(deal);

        dealResult.Should().Be(DealResult.WonStandardBid);
        winningTeam.Should().Be(Team.Team1);
    }

    [Fact]
    public void CalculateDealResult_CallingTeamWins4Tricks_ReturnsWonStandardBid()
    {
        var deal = CreateTestDeal(PlayerPosition.North, false, Team.Team1, Team.Team1, Team.Team1, Team.Team1, Team.Team2);

        var (dealResult, winningTeam) = _calculator.CalculateDealResult(deal);

        dealResult.Should().Be(DealResult.WonStandardBid);
        winningTeam.Should().Be(Team.Team1);
    }

    [Fact]
    public void CalculateDealResult_CallingTeamWinsAllTricksNotAlone_ReturnsWonGotAllTricks()
    {
        var deal = CreateTestDeal(PlayerPosition.North, false, Team.Team1, Team.Team1, Team.Team1, Team.Team1, Team.Team1);

        var (dealResult, winningTeam) = _calculator.CalculateDealResult(deal);

        dealResult.Should().Be(DealResult.WonGotAllTricks);
        winningTeam.Should().Be(Team.Team1);
    }

    [Fact]
    public void CalculateDealResult_CallingTeamWinsAllTricksGoingAlone_ReturnsWonAndWentAlone()
    {
        var deal = CreateTestDeal(PlayerPosition.North, true, Team.Team1, Team.Team1, Team.Team1, Team.Team1, Team.Team1);

        var (dealResult, winningTeam) = _calculator.CalculateDealResult(deal);

        dealResult.Should().Be(DealResult.WonAndWentAlone);
        winningTeam.Should().Be(Team.Team1);
    }

    [Fact]
    public void CalculateDealResult_CallingTeamWins0Tricks_ReturnsOpponentsEuchred()
    {
        var deal = CreateTestDeal(PlayerPosition.North, false, Team.Team2, Team.Team2, Team.Team2, Team.Team2, Team.Team2);

        var (dealResult, winningTeam) = _calculator.CalculateDealResult(deal);

        dealResult.Should().Be(DealResult.OpponentsEuchred);
        winningTeam.Should().Be(Team.Team2);
    }

    [Fact]
    public void CalculateDealResult_CallingTeamWins1Trick_ReturnsOpponentsEuchred()
    {
        var deal = CreateTestDeal(PlayerPosition.North, false, Team.Team1, Team.Team2, Team.Team2, Team.Team2, Team.Team2);

        var (dealResult, winningTeam) = _calculator.CalculateDealResult(deal);

        dealResult.Should().Be(DealResult.OpponentsEuchred);
        winningTeam.Should().Be(Team.Team2);
    }

    [Fact]
    public void CalculateDealResult_CallingTeamWins2Tricks_ReturnsOpponentsEuchred()
    {
        var deal = CreateTestDeal(PlayerPosition.North, false, Team.Team1, Team.Team1, Team.Team2, Team.Team2, Team.Team2);

        var (dealResult, winningTeam) = _calculator.CalculateDealResult(deal);

        dealResult.Should().Be(DealResult.OpponentsEuchred);
        winningTeam.Should().Be(Team.Team2);
    }

    [Fact]
    public void CalculateDealResult_WhenCallingTeamWinsBid_ReturnsCallingTeam()
    {
        var deal = CreateTestDeal(PlayerPosition.East, false, Team.Team2, Team.Team2, Team.Team2, Team.Team1, Team.Team1);

        var (dealResult, winningTeam) = _calculator.CalculateDealResult(deal);

        dealResult.Should().Be(DealResult.WonStandardBid);
        winningTeam.Should().Be(Team.Team2);
    }

    [Fact]
    public void CalculateDealResult_WhenEuchred_ReturnsOpposingTeam()
    {
        var deal = CreateTestDeal(PlayerPosition.East, false, Team.Team1, Team.Team1, Team.Team1, Team.Team2, Team.Team2);

        var (dealResult, winningTeam) = _calculator.CalculateDealResult(deal);

        dealResult.Should().Be(DealResult.OpponentsEuchred);
        winningTeam.Should().Be(Team.Team1);
    }

    [Theory]
    [InlineData(PlayerPosition.North, Team.Team1)]
    [InlineData(PlayerPosition.South, Team.Team1)]
    [InlineData(PlayerPosition.East, Team.Team2)]
    [InlineData(PlayerPosition.West, Team.Team2)]
    public void CalculateDealResult_AllPlayerPositionsAsCaller_ReturnsCorrectTeam(
        PlayerPosition callingPlayer, Team expectedWinningTeam)
    {
        var winningTeam = expectedWinningTeam;
        var deal = CreateTestDeal(callingPlayer, false, winningTeam, winningTeam, winningTeam, winningTeam, winningTeam);

        var result = _calculator.CalculateDealResult(deal);

        result.dealResult.Should().Be(DealResult.WonGotAllTricks);
        result.winningTeam.Should().Be(expectedWinningTeam);
    }

    [Fact]
    public void CalculateDealResult_GoingAloneButOnly4Tricks_ReturnsWonStandardBid()
    {
        var deal = CreateTestDeal(PlayerPosition.North, true, Team.Team1, Team.Team1, Team.Team1, Team.Team1, Team.Team2);

        var (dealResult, winningTeam) = _calculator.CalculateDealResult(deal);

        dealResult.Should().Be(DealResult.WonStandardBid);
        winningTeam.Should().Be(Team.Team1);
    }

    [Fact]
    public void CalculateDealResult_GoingAloneButOnly3Tricks_ReturnsWonStandardBid()
    {
        var deal = CreateTestDeal(PlayerPosition.North, true, Team.Team1, Team.Team1, Team.Team1, Team.Team2, Team.Team2);

        var (dealResult, winningTeam) = _calculator.CalculateDealResult(deal);

        dealResult.Should().Be(DealResult.WonStandardBid);
        winningTeam.Should().Be(Team.Team1);
    }

    [Fact]
    public void CalculateDealResult_GoingAloneButGetsEuchred_ReturnsOpponentsEuchredWithOpponentsWinning()
    {
        var deal = CreateTestDeal(PlayerPosition.North, true, Team.Team2, Team.Team2, Team.Team2, Team.Team1, Team.Team1);

        var (dealResult, winningTeam) = _calculator.CalculateDealResult(deal);

        dealResult.Should().Be(DealResult.OpponentsEuchred);
        winningTeam.Should().Be(Team.Team2);
    }

    private static Deal CreateTestDeal(
        PlayerPosition callingPlayer,
        bool isGoingAlone,
        params Team[] trickWinners)
    {
        var deal = new Deal
        {
            CallingPlayer = callingPlayer,
            CallingPlayerIsGoingAlone = isGoingAlone,
        };

        foreach (var winner in trickWinners)
        {
            deal.CompletedTricks.Add(new Trick { WinningTeam = winner });
        }

        return deal;
    }
}
