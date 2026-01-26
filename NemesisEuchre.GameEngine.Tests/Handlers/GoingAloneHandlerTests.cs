using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Handlers;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests.Handlers;

public class GoingAloneHandlerTests
{
    private readonly GoingAloneHandler _handler = new();

    [Fact]
    public void ShouldPlayerSit_WhenNotGoingAlone_ReturnsFalse()
    {
        var deal = new Deal
        {
            CallingPlayer = PlayerPosition.North,
            CallingPlayerIsGoingAlone = false,
        };

        var result = _handler.ShouldPlayerSit(deal, PlayerPosition.South);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(PlayerPosition.North, PlayerPosition.South, true)]
    [InlineData(PlayerPosition.East, PlayerPosition.West, true)]
    [InlineData(PlayerPosition.South, PlayerPosition.North, true)]
    [InlineData(PlayerPosition.West, PlayerPosition.East, true)]
    [InlineData(PlayerPosition.North, PlayerPosition.North, false)]
    [InlineData(PlayerPosition.North, PlayerPosition.East, false)]
    [InlineData(PlayerPosition.North, PlayerPosition.West, false)]
    public void ShouldPlayerSit_WhenGoingAlone_ReturnsCorrectValue(
        PlayerPosition callingPlayer,
        PlayerPosition position,
        bool expectedToSit)
    {
        var deal = new Deal
        {
            CallingPlayer = callingPlayer,
            CallingPlayerIsGoingAlone = true,
        };

        var result = _handler.ShouldPlayerSit(deal, position);

        result.Should().Be(expectedToSit);
    }

    [Fact]
    public void GetNextActivePlayer_WhenNotGoingAlone_ReturnsNextPosition()
    {
        var deal = new Deal
        {
            CallingPlayer = PlayerPosition.North,
            CallingPlayerIsGoingAlone = false,
        };

        var result = _handler.GetNextActivePlayer(PlayerPosition.North, deal);

        result.Should().Be(PlayerPosition.East);
    }

    [Theory]
    [InlineData(PlayerPosition.North, PlayerPosition.East)]
    [InlineData(PlayerPosition.East, PlayerPosition.West)]
    [InlineData(PlayerPosition.South, PlayerPosition.West)]
    [InlineData(PlayerPosition.West, PlayerPosition.North)]
    public void GetNextActivePlayer_WhenPartnerShouldSit_SkipsPartner(
        PlayerPosition current,
        PlayerPosition expectedNext)
    {
        var deal = new Deal
        {
            CallingPlayer = PlayerPosition.North,
            CallingPlayerIsGoingAlone = true,
        };

        var result = _handler.GetNextActivePlayer(current, deal);

        result.Should().Be(expectedNext);
    }

    [Fact]
    public void GetNumberOfCardsToPlay_WhenNotGoingAlone_ReturnsFour()
    {
        var deal = new Deal
        {
            CallingPlayerIsGoingAlone = false,
        };

        var result = _handler.GetNumberOfCardsToPlay(deal);

        result.Should().Be(4);
    }

    [Fact]
    public void GetNumberOfCardsToPlay_WhenGoingAlone_ReturnsThree()
    {
        var deal = new Deal
        {
            CallingPlayerIsGoingAlone = true,
        };

        var result = _handler.GetNumberOfCardsToPlay(deal);

        result.Should().Be(3);
    }
}
