using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests.Extensions;

public class TrickExtensionsTests
{
    [Fact]
    public void ToRelative_WithTrick_ConvertsLeadSuitAndCards()
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.East,
            LeadSuit = Suit.Spades,
        };
        trick.CardsPlayed.Add(new PlayedCard(new Card(Suit.Spades, Rank.King), PlayerPosition.East));
        trick.CardsPlayed.Add(new PlayedCard(new Card(Suit.Clubs, Rank.Queen), PlayerPosition.South));

        var relative = trick.ToRelative(PlayerPosition.North, Suit.Hearts);

        relative.LeadPosition.Should().Be(RelativePlayerPosition.LeftHandOpponent);
        relative.LeadSuit.Should().Be(RelativeSuit.NonTrumpOppositeColor1);
        relative.CardsPlayed.Should().HaveCount(2);
        relative.CardsPlayed[0].RelativeCard.Suit.Should().Be(RelativeSuit.NonTrumpOppositeColor1);
        relative.CardsPlayed[1].RelativeCard.Suit.Should().Be(RelativeSuit.NonTrumpOppositeColor2);
    }
}
