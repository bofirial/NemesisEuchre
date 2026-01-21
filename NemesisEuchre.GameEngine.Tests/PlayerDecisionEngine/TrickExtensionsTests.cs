using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests.PlayerDecisionEngine;

public class TrickExtensionsTests
{
    [Fact]
    public void TrickToRelativeShouldConvertLeadPositionAndCards()
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
            Card = new Card { Suit = Suit.Spades, Rank = Rank.Queen },
            PlayerPosition = PlayerPosition.South,
        });

        var relative = trick.ToRelative(PlayerPosition.North);

        relative.LeadPosition.Should().Be(RelativePlayerPosition.LeftHandOpponent);
        relative.LeadSuit.Should().Be(Suit.Spades);
        relative.CardsPlayed.Should().HaveCount(2);
        relative.CardsPlayed[0].PlayerPosition.Should().Be(RelativePlayerPosition.LeftHandOpponent);
        relative.CardsPlayed[1].PlayerPosition.Should().Be(RelativePlayerPosition.Partner);
    }
}
