using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.GameEngine.Tests;

public class PlayerPositionExtensionsTests
{
    [Theory]
    [InlineData(PlayerPosition.North, Team.Team1)]
    [InlineData(PlayerPosition.South, Team.Team1)]
    [InlineData(PlayerPosition.East, Team.Team2)]
    [InlineData(PlayerPosition.West, Team.Team2)]
    public void GetTeamShouldReturnCorrectTeamForPosition(PlayerPosition position, Team expected)
    {
        var result = position.GetTeam();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(PlayerPosition.North, PlayerPosition.East)]
    [InlineData(PlayerPosition.East, PlayerPosition.South)]
    [InlineData(PlayerPosition.South, PlayerPosition.West)]
    [InlineData(PlayerPosition.West, PlayerPosition.North)]
    public void GetNextPositionShouldReturnNextPositionClockwise(PlayerPosition position, PlayerPosition expected)
    {
        var result = position.GetNextPosition();

        result.Should().Be(expected);
    }

    [Fact]
    public void GetNextPositionShouldCompleteFullCycle()
    {
        var current = PlayerPosition.North;

        current = current.GetNextPosition();
        current.Should().Be(PlayerPosition.East);

        current = current.GetNextPosition();
        current.Should().Be(PlayerPosition.South);

        current = current.GetNextPosition();
        current.Should().Be(PlayerPosition.West);

        current = current.GetNextPosition();
        current.Should().Be(PlayerPosition.North);
    }
}
