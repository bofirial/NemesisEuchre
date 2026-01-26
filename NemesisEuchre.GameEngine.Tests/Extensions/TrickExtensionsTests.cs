using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
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
        trick.CardsPlayed.Add(new PlayedCard
        {
            Card = new Card { Suit = Suit.Spades, Rank = Rank.King },
            PlayerPosition = PlayerPosition.East,
        });
        trick.CardsPlayed.Add(new PlayedCard
        {
            Card = new Card { Suit = Suit.Clubs, Rank = Rank.Queen },
            PlayerPosition = PlayerPosition.South,
        });

        var relative = trick.ToRelative(PlayerPosition.North, Suit.Hearts);

        relative.LeadPosition.Should().Be(RelativePlayerPosition.LeftHandOpponent);
        relative.LeadSuit.Should().Be(RelativeSuit.NonTrumpOppositeColor1);
        relative.CardsPlayed.Should().HaveCount(2);
        relative.CardsPlayed[0].RelativeCard.Suit.Should().Be(RelativeSuit.NonTrumpOppositeColor1);
        relative.CardsPlayed[1].RelativeCard.Suit.Should().Be(RelativeSuit.NonTrumpOppositeColor2);
    }
}
