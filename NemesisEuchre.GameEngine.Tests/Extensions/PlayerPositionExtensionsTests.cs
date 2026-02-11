using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests.Extensions;

public class PlayerPositionExtensionsTests
{
    [Theory]
    [InlineData(PlayerPosition.North, Team.Team1)]
    [InlineData(PlayerPosition.South, Team.Team1)]
    [InlineData(PlayerPosition.East, Team.Team2)]
    [InlineData(PlayerPosition.West, Team.Team2)]
    public void GetTeam_WithAnyPosition_ReturnsCorrectTeam(PlayerPosition position, Team expected)
    {
        var result = position.GetTeam();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(PlayerPosition.North, PlayerPosition.East)]
    [InlineData(PlayerPosition.East, PlayerPosition.South)]
    [InlineData(PlayerPosition.South, PlayerPosition.West)]
    [InlineData(PlayerPosition.West, PlayerPosition.North)]
    public void GetNextPosition_WithAnyPosition_ReturnsNextPositionClockwise(PlayerPosition position, PlayerPosition expected)
    {
        var result = position.GetNextPosition();

        result.Should().Be(expected);
    }

    [Fact]
    public void GetNextPosition_CalledMultipleTimes_CompletesFullCycle()
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

    [Theory]
    [InlineData(PlayerPosition.North, PlayerPosition.South)]
    [InlineData(PlayerPosition.South, PlayerPosition.North)]
    [InlineData(PlayerPosition.East, PlayerPosition.West)]
    [InlineData(PlayerPosition.West, PlayerPosition.East)]
    public void GetPartnerPosition_WithAnyPosition_ReturnsOppositePositionOnSameTeam(PlayerPosition position, PlayerPosition expected)
    {
        var result = position.GetPartnerPosition();

        result.Should().Be(expected);
    }

    [Fact]
    public void GetPartnerPosition_WithAnyPosition_ReturnsPlayerOnSameTeam()
    {
        foreach (var position in new[] { PlayerPosition.North, PlayerPosition.South, PlayerPosition.East, PlayerPosition.West })
        {
            var partner = position.GetPartnerPosition();
            position.GetTeam().Should().Be(partner.GetTeam());
        }
    }

    [Theory]
    [InlineData(PlayerPosition.North, PlayerPosition.North, RelativePlayerPosition.Self)]
    [InlineData(PlayerPosition.North, PlayerPosition.East, RelativePlayerPosition.LeftHandOpponent)]
    [InlineData(PlayerPosition.North, PlayerPosition.South, RelativePlayerPosition.Partner)]
    [InlineData(PlayerPosition.North, PlayerPosition.West, RelativePlayerPosition.RightHandOpponent)]
    [InlineData(PlayerPosition.East, PlayerPosition.East, RelativePlayerPosition.Self)]
    [InlineData(PlayerPosition.East, PlayerPosition.South, RelativePlayerPosition.LeftHandOpponent)]
    [InlineData(PlayerPosition.East, PlayerPosition.West, RelativePlayerPosition.Partner)]
    [InlineData(PlayerPosition.East, PlayerPosition.North, RelativePlayerPosition.RightHandOpponent)]
    [InlineData(PlayerPosition.South, PlayerPosition.South, RelativePlayerPosition.Self)]
    [InlineData(PlayerPosition.South, PlayerPosition.West, RelativePlayerPosition.LeftHandOpponent)]
    [InlineData(PlayerPosition.South, PlayerPosition.North, RelativePlayerPosition.Partner)]
    [InlineData(PlayerPosition.South, PlayerPosition.East, RelativePlayerPosition.RightHandOpponent)]
    [InlineData(PlayerPosition.West, PlayerPosition.West, RelativePlayerPosition.Self)]
    [InlineData(PlayerPosition.West, PlayerPosition.North, RelativePlayerPosition.LeftHandOpponent)]
    [InlineData(PlayerPosition.West, PlayerPosition.East, RelativePlayerPosition.Partner)]
    [InlineData(PlayerPosition.West, PlayerPosition.South, RelativePlayerPosition.RightHandOpponent)]
    public void ToRelativePosition_WithAbsolutePosition_ConvertsCorrectly(
        PlayerPosition self,
        PlayerPosition absolute,
        RelativePlayerPosition expected)
    {
        var result = absolute.ToRelativePosition(self);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(PlayerPosition.North, RelativePlayerPosition.Self, PlayerPosition.North)]
    [InlineData(PlayerPosition.North, RelativePlayerPosition.LeftHandOpponent, PlayerPosition.East)]
    [InlineData(PlayerPosition.North, RelativePlayerPosition.Partner, PlayerPosition.South)]
    [InlineData(PlayerPosition.North, RelativePlayerPosition.RightHandOpponent, PlayerPosition.West)]
    [InlineData(PlayerPosition.East, RelativePlayerPosition.Self, PlayerPosition.East)]
    [InlineData(PlayerPosition.East, RelativePlayerPosition.LeftHandOpponent, PlayerPosition.South)]
    [InlineData(PlayerPosition.East, RelativePlayerPosition.Partner, PlayerPosition.West)]
    [InlineData(PlayerPosition.East, RelativePlayerPosition.RightHandOpponent, PlayerPosition.North)]
    [InlineData(PlayerPosition.South, RelativePlayerPosition.Self, PlayerPosition.South)]
    [InlineData(PlayerPosition.South, RelativePlayerPosition.LeftHandOpponent, PlayerPosition.West)]
    [InlineData(PlayerPosition.South, RelativePlayerPosition.Partner, PlayerPosition.North)]
    [InlineData(PlayerPosition.South, RelativePlayerPosition.RightHandOpponent, PlayerPosition.East)]
    [InlineData(PlayerPosition.West, RelativePlayerPosition.Self, PlayerPosition.West)]
    [InlineData(PlayerPosition.West, RelativePlayerPosition.LeftHandOpponent, PlayerPosition.North)]
    [InlineData(PlayerPosition.West, RelativePlayerPosition.Partner, PlayerPosition.East)]
    [InlineData(PlayerPosition.West, RelativePlayerPosition.RightHandOpponent, PlayerPosition.South)]
    public void ToAbsolutePosition_WithRelativePosition_ConvertsCorrectly(
        PlayerPosition self,
        RelativePlayerPosition relative,
        PlayerPosition expected)
    {
        var result = relative.ToAbsolutePosition(self);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(PlayerPosition.North)]
    [InlineData(PlayerPosition.East)]
    [InlineData(PlayerPosition.South)]
    [InlineData(PlayerPosition.West)]
    public void ToRelativePosition_WhenConvertedBackToAbsolute_IsReversible(PlayerPosition self)
    {
        foreach (var absolute in new[]
        {
            PlayerPosition.North, PlayerPosition.East,
            PlayerPosition.South, PlayerPosition.West,
        })
        {
            var relative = absolute.ToRelativePosition(self);
            var backToAbsolute = relative.ToAbsolutePosition(self);
            backToAbsolute.Should().Be(absolute);
        }
    }

    [Fact]
    public void GetPlayerAtRelativePosition_WithRelativePosition_ReturnsCorrectPlayer()
    {
        var players = new Dictionary<PlayerPosition, Player>
        {
            [PlayerPosition.North] = new Player { Position = PlayerPosition.North, Actor = new Actor(ActorType.Chaos, null) },
            [PlayerPosition.East] = new Player { Position = PlayerPosition.East, Actor = new Actor(ActorType.Chaos, null) },
            [PlayerPosition.South] = new Player { Position = PlayerPosition.South, Actor = new Actor(ActorType.Chaos, null) },
            [PlayerPosition.West] = new Player { Position = PlayerPosition.West, Actor = new Actor(ActorType.Chaos, null) },
        };

        var partner = players.GetPlayerAtRelativePosition(
            PlayerPosition.North,
            RelativePlayerPosition.Partner);

        partner.Position.Should().Be(PlayerPosition.South);

        var leftOpponent = players.GetPlayerAtRelativePosition(
            PlayerPosition.South,
            RelativePlayerPosition.LeftHandOpponent);

        leftOpponent.Position.Should().Be(PlayerPosition.West);
    }

    [Theory]
    [InlineData(PlayerPosition.North)]
    [InlineData(PlayerPosition.East)]
    [InlineData(PlayerPosition.South)]
    [InlineData(PlayerPosition.West)]
    public void ToAbsolutePosition_ForPartner_ReturnsPlayerOnSameTeam(PlayerPosition self)
    {
        var partnerAbsolute = RelativePlayerPosition.Partner.ToAbsolutePosition(self);
        self.GetTeam().Should().Be(partnerAbsolute.GetTeam());
    }

    [Theory]
    [InlineData(PlayerPosition.North)]
    [InlineData(PlayerPosition.East)]
    [InlineData(PlayerPosition.South)]
    [InlineData(PlayerPosition.West)]
    public void ToAbsolutePosition_ForOpponents_ReturnsPlayersOnDifferentTeam(PlayerPosition self)
    {
        var leftAbsolute = RelativePlayerPosition.LeftHandOpponent.ToAbsolutePosition(self);
        var rightAbsolute = RelativePlayerPosition.RightHandOpponent.ToAbsolutePosition(self);

        self.GetTeam().Should().NotBe(leftAbsolute.GetTeam());
        self.GetTeam().Should().NotBe(rightAbsolute.GetTeam());
    }

    [Theory]
    [InlineData(PlayerPosition.North, RelativePlayerPosition.Self, PlayerPosition.North)]
    [InlineData(PlayerPosition.North, RelativePlayerPosition.LeftHandOpponent, PlayerPosition.West)]
    [InlineData(PlayerPosition.North, RelativePlayerPosition.Partner, PlayerPosition.South)]
    [InlineData(PlayerPosition.North, RelativePlayerPosition.RightHandOpponent, PlayerPosition.East)]
    [InlineData(PlayerPosition.East, RelativePlayerPosition.Self, PlayerPosition.East)]
    [InlineData(PlayerPosition.East, RelativePlayerPosition.LeftHandOpponent, PlayerPosition.North)]
    [InlineData(PlayerPosition.East, RelativePlayerPosition.Partner, PlayerPosition.West)]
    [InlineData(PlayerPosition.East, RelativePlayerPosition.RightHandOpponent, PlayerPosition.South)]
    [InlineData(PlayerPosition.South, RelativePlayerPosition.Self, PlayerPosition.South)]
    [InlineData(PlayerPosition.South, RelativePlayerPosition.LeftHandOpponent, PlayerPosition.East)]
    [InlineData(PlayerPosition.South, RelativePlayerPosition.Partner, PlayerPosition.North)]
    [InlineData(PlayerPosition.South, RelativePlayerPosition.RightHandOpponent, PlayerPosition.West)]
    [InlineData(PlayerPosition.West, RelativePlayerPosition.Self, PlayerPosition.West)]
    [InlineData(PlayerPosition.West, RelativePlayerPosition.LeftHandOpponent, PlayerPosition.South)]
    [InlineData(PlayerPosition.West, RelativePlayerPosition.Partner, PlayerPosition.East)]
    [InlineData(PlayerPosition.West, RelativePlayerPosition.RightHandOpponent, PlayerPosition.North)]
    public void DeriveAbsolutePosition_WithReferenceAndRelative_ReturnsSelfPosition(
        PlayerPosition referenceAbsolute,
        RelativePlayerPosition referenceRelative,
        PlayerPosition expectedSelf)
    {
        var result = PlayerPositionExtensions.DeriveAbsolutePosition(referenceAbsolute, referenceRelative);

        result.Should().Be(expectedSelf);
    }

    [Theory]
    [InlineData(PlayerPosition.North)]
    [InlineData(PlayerPosition.East)]
    [InlineData(PlayerPosition.South)]
    [InlineData(PlayerPosition.West)]
    public void DeriveAbsolutePosition_RoundTrip_IsConsistentWithToRelativePosition(PlayerPosition dealer)
    {
        foreach (var self in new[] { PlayerPosition.North, PlayerPosition.East, PlayerPosition.South, PlayerPosition.West })
        {
            var dealerRelative = dealer.ToRelativePosition(self);
            var derivedSelf = PlayerPositionExtensions.DeriveAbsolutePosition(dealer, dealerRelative);

            derivedSelf.Should().Be(
                self,
                "DeriveAbsolutePosition should recover self={0} from dealer={1}, dealerRelative={2}",
                self,
                dealer,
                dealerRelative);
        }
    }
}
