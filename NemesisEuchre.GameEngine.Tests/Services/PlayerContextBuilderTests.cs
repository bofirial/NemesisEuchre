using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Services;

namespace NemesisEuchre.GameEngine.Tests.Services;

public class PlayerContextBuilderTests
{
    private readonly PlayerContextBuilder _builder = new();

    [Theory]
    [InlineData(PlayerPosition.North, 10, 20)]
    [InlineData(PlayerPosition.South, 10, 20)]
    [InlineData(PlayerPosition.East, 20, 10)]
    [InlineData(PlayerPosition.West, 20, 10)]
    public void GetScores_ReturnsCorrectScoresForPlayerTeam(
        PlayerPosition playerPosition,
        short expectedTeamScore,
        short expectedOpponentScore)
    {
        var deal = new Deal
        {
            Team1Score = 10,
            Team2Score = 20,
        };

        var (teamScore, opponentScore) = _builder.GetScores(deal, playerPosition);

        teamScore.Should().Be(expectedTeamScore);
        opponentScore.Should().Be(expectedOpponentScore);
    }

    [Theory]
    [InlineData(PlayerPosition.North, PlayerPosition.North, RelativePlayerPosition.Self)]
    [InlineData(PlayerPosition.North, PlayerPosition.East, RelativePlayerPosition.LeftHandOpponent)]
    [InlineData(PlayerPosition.North, PlayerPosition.South, RelativePlayerPosition.Partner)]
    [InlineData(PlayerPosition.North, PlayerPosition.West, RelativePlayerPosition.RightHandOpponent)]
    [InlineData(PlayerPosition.East, PlayerPosition.North, RelativePlayerPosition.RightHandOpponent)]
    [InlineData(PlayerPosition.East, PlayerPosition.East, RelativePlayerPosition.Self)]
    [InlineData(PlayerPosition.East, PlayerPosition.South, RelativePlayerPosition.LeftHandOpponent)]
    [InlineData(PlayerPosition.East, PlayerPosition.West, RelativePlayerPosition.Partner)]
    public void GetRelativeDealerPosition_ReturnsCorrectRelativePosition(
        PlayerPosition playerPosition,
        PlayerPosition dealerPosition,
        RelativePlayerPosition expectedRelativePosition)
    {
        var deal = new Deal
        {
            DealerPosition = dealerPosition,
        };

        var relativePosition = _builder.GetRelativeDealerPosition(deal, playerPosition);

        relativePosition.Should().Be(expectedRelativePosition);
    }
}
