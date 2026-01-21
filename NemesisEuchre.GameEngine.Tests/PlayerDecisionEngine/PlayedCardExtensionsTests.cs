using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests.PlayerDecisionEngine;

public class PlayedCardExtensionsTests
{
    [Fact]
    public void PlayedCardToRelativeShouldConvertPosition()
    {
        var playedCard = new PlayedCard
        {
            Card = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
            PlayerPosition = PlayerPosition.South,
        };

        var relative = playedCard.ToRelative(PlayerPosition.North);

        relative.Card.Should().Be(playedCard.Card);
        relative.PlayerPosition.Should().Be(RelativePlayerPosition.Partner);
    }
}
