using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests.Extensions;

public class PlayedCardExtensionsTests
{
    [Fact]
    public void ToRelative_WithPlayedCard_ConvertsPositionAndCardToRelative()
    {
        var playedCard = new PlayedCard(new Card(Suit.Hearts, Rank.Ace), PlayerPosition.South);

        var relative = playedCard.ToRelative(PlayerPosition.North, Suit.Spades);

        relative.RelativeCard.Rank.Should().Be(Rank.Ace);
        relative.RelativeCard.Suit.Should().Be(RelativeSuit.NonTrumpOppositeColor1);
        relative.PlayerPosition.Should().Be(RelativePlayerPosition.Partner);
    }
}
