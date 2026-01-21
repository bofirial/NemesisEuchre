using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests.Extensions;

public class PlayedCardExtensionsTests
{
    [Fact]
    public void PlayedCardToRelativeShouldConvertPositionAndCard()
    {
        var playedCard = new PlayedCard
        {
            Card = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
            PlayerPosition = PlayerPosition.South,
        };

        var relative = playedCard.ToRelative(PlayerPosition.North, Suit.Spades);

        relative.RelativeCard.Rank.Should().Be(Rank.Ace);
        relative.RelativeCard.Suit.Should().Be(RelativeSuit.NonTrumpOppositeColor1);
        relative.PlayerPosition.Should().Be(RelativePlayerPosition.Partner);
    }
}
