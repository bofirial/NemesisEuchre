using FluentAssertions;

using NemesisEuchre.DataAccess.Extensions;
using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.DataAccess.Tests.Extensions;

public class DealResultExtensionsTests
{
    [Theory]
    [InlineData(PlayerPosition.North, Team.Team1, 1)]
    [InlineData(PlayerPosition.South, Team.Team1, 1)]
    [InlineData(PlayerPosition.East, Team.Team2, 1)]
    [InlineData(PlayerPosition.West, Team.Team2, 1)]
    [InlineData(PlayerPosition.North, Team.Team2, -1)]
    [InlineData(PlayerPosition.South, Team.Team2, -1)]
    [InlineData(PlayerPosition.East, Team.Team1, -1)]
    [InlineData(PlayerPosition.West, Team.Team1, -1)]
    public void CalculateRelativeDealPoints_WonStandardBid_ReturnsOneOrNegativeOne(PlayerPosition playerPosition, Team winningTeam, short expectedPoints)
    {
        DealResult? dealResult = DealResult.WonStandardBid;
        var result = dealResult.CalculateRelativeDealPoints(playerPosition, winningTeam);

        result.Should().Be(expectedPoints);
    }

    [Theory]
    [InlineData(PlayerPosition.North, Team.Team1, 2)]
    [InlineData(PlayerPosition.South, Team.Team1, 2)]
    [InlineData(PlayerPosition.East, Team.Team2, 2)]
    [InlineData(PlayerPosition.West, Team.Team2, 2)]
    [InlineData(PlayerPosition.North, Team.Team2, -2)]
    [InlineData(PlayerPosition.South, Team.Team2, -2)]
    [InlineData(PlayerPosition.East, Team.Team1, -2)]
    [InlineData(PlayerPosition.West, Team.Team1, -2)]
    public void CalculateRelativeDealPoints_WonGotAllTricks_ReturnsTwoOrNegativeTwo(PlayerPosition playerPosition, Team winningTeam, short expectedPoints)
    {
        DealResult? dealResult = DealResult.WonGotAllTricks;
        var result = dealResult.CalculateRelativeDealPoints(playerPosition, winningTeam);

        result.Should().Be(expectedPoints);
    }

    [Theory]
    [InlineData(PlayerPosition.North, Team.Team1, 4)]
    [InlineData(PlayerPosition.South, Team.Team1, 4)]
    [InlineData(PlayerPosition.East, Team.Team2, 4)]
    [InlineData(PlayerPosition.West, Team.Team2, 4)]
    [InlineData(PlayerPosition.North, Team.Team2, -4)]
    [InlineData(PlayerPosition.South, Team.Team2, -4)]
    [InlineData(PlayerPosition.East, Team.Team1, -4)]
    [InlineData(PlayerPosition.West, Team.Team1, -4)]
    public void CalculateRelativeDealPoints_WonAndWentAlone_ReturnsFourOrNegativeFour(PlayerPosition playerPosition, Team winningTeam, short expectedPoints)
    {
        DealResult? dealResult = DealResult.WonAndWentAlone;
        var result = dealResult.CalculateRelativeDealPoints(playerPosition, winningTeam);

        result.Should().Be(expectedPoints);
    }

    [Theory]
    [InlineData(PlayerPosition.North, Team.Team1, 2)]
    [InlineData(PlayerPosition.South, Team.Team1, 2)]
    [InlineData(PlayerPosition.East, Team.Team2, 2)]
    [InlineData(PlayerPosition.West, Team.Team2, 2)]
    [InlineData(PlayerPosition.North, Team.Team2, -2)]
    [InlineData(PlayerPosition.South, Team.Team2, -2)]
    [InlineData(PlayerPosition.East, Team.Team1, -2)]
    [InlineData(PlayerPosition.West, Team.Team1, -2)]
    public void CalculateRelativeDealPoints_OpponentsEuchred_ReturnsTwoOrNegativeTwo(PlayerPosition playerPosition, Team winningTeam, short expectedPoints)
    {
        DealResult? dealResult = DealResult.OpponentsEuchred;
        var result = dealResult.CalculateRelativeDealPoints(playerPosition, winningTeam);

        result.Should().Be(expectedPoints);
    }

    [Theory]
    [InlineData(PlayerPosition.North)]
    [InlineData(PlayerPosition.South)]
    [InlineData(PlayerPosition.East)]
    [InlineData(PlayerPosition.West)]
    public void CalculateRelativeDealPoints_ThrowIn_ReturnsZero(PlayerPosition playerPosition)
    {
        DealResult? dealResult = DealResult.ThrowIn;
        var result = dealResult.CalculateRelativeDealPoints(playerPosition, Team.Team1);

        result.Should().Be(0);
    }

    [Theory]
    [InlineData(PlayerPosition.North, Team.Team1)]
    [InlineData(PlayerPosition.South, Team.Team2)]
    [InlineData(PlayerPosition.East, Team.Team1)]
    [InlineData(PlayerPosition.West, Team.Team2)]
    public void CalculateRelativeDealPoints_NullDealResult_ReturnsNull(PlayerPosition playerPosition, Team winningTeam)
    {
        DealResult? dealResult = null;

        var result = dealResult.CalculateRelativeDealPoints(playerPosition, winningTeam);

        result.Should().BeNull();
    }

    [Theory]
    [InlineData(PlayerPosition.North, DealResult.WonStandardBid)]
    [InlineData(PlayerPosition.South, DealResult.WonGotAllTricks)]
    [InlineData(PlayerPosition.East, DealResult.WonAndWentAlone)]
    [InlineData(PlayerPosition.West, DealResult.OpponentsEuchred)]
    public void CalculateRelativeDealPoints_NullWinningTeam_ReturnsNull(PlayerPosition playerPosition, DealResult dealResult)
    {
        DealResult? nullableDealResult = dealResult;
        Team? winningTeam = null;

        var result = nullableDealResult.CalculateRelativeDealPoints(playerPosition, winningTeam);

        result.Should().BeNull();
    }

    [Fact]
    public void CalculateRelativeDealPoints_Team1NorthPlayerWinsStandardBid_ReturnsPositiveOne()
    {
        DealResult? dealResult = DealResult.WonStandardBid;
        var result = dealResult.CalculateRelativeDealPoints(PlayerPosition.North, Team.Team1);

        result.Should().Be(1);
    }

    [Fact]
    public void CalculateRelativeDealPoints_Team2EastPlayerWinsAndWentAlone_ReturnsPositiveFour()
    {
        DealResult? dealResult = DealResult.WonAndWentAlone;
        var result = dealResult.CalculateRelativeDealPoints(PlayerPosition.East, Team.Team2);

        result.Should().Be(4);
    }

    [Fact]
    public void CalculateRelativeDealPoints_Team1NorthPlayerLosesOpponentsEuchred_ReturnsNegativeTwo()
    {
        DealResult? dealResult = DealResult.OpponentsEuchred;
        var result = dealResult.CalculateRelativeDealPoints(PlayerPosition.North, Team.Team2);

        result.Should().Be(-2);
    }

    [Fact]
    public void CalculateRelativeDealPoints_Team2WestPlayerLosesGotAllTricks_ReturnsNegativeTwo()
    {
        DealResult? dealResult = DealResult.WonGotAllTricks;
        var result = dealResult.CalculateRelativeDealPoints(PlayerPosition.West, Team.Team1);

        result.Should().Be(-2);
    }
}
