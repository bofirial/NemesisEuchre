using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests.PlayerDecisionEngine;

public class HandExtensionsTests
{
    [Fact]
    public void HandToRelativeShouldConvertAllPositions()
    {
        var hand = new Hand
        {
            HandStatus = HandStatus.Playing,
            DealerPosition = PlayerPosition.West,
            UpCard = new Card { Suit = Suit.Diamonds, Rank = Rank.Nine },
            Trump = Suit.Diamonds,
            CallingPlayer = PlayerPosition.North,
            CallingPlayerIsGoingAlone = false,
        };

        var relative = hand.ToRelative(PlayerPosition.North);

        relative.HandStatus.Should().Be(HandStatus.Playing);
        relative.DealerPosition.Should().Be(RelativePlayerPosition.RightHandOpponent);
        relative.UpCard.Should().Be(hand.UpCard);
        relative.Trump.Should().Be(Suit.Diamonds);
        relative.CallingPlayer.Should().Be(RelativePlayerPosition.Self);
        relative.CallingPlayerIsGoingAlone.Should().BeFalse();
    }

    [Fact]
    public void HandToRelativeShouldConvertCurrentTrick()
    {
        var hand = new Hand
        {
            CurrentTrick = new Trick
            {
                LeadPosition = PlayerPosition.South,
                LeadSuit = Suit.Clubs,
            },
        };

        var relative = hand.ToRelative(PlayerPosition.North);

        relative.CurrentTrick.Should().NotBeNull();
        relative.CurrentTrick!.LeadPosition.Should().Be(RelativePlayerPosition.Partner);
        relative.CurrentTrick.LeadSuit.Should().Be(Suit.Clubs);
    }

    [Fact]
    public void HandToRelativeShouldConvertCompletedTricks()
    {
        var hand = new Hand();
        hand.CompletedTricks.Add(new Trick
        {
            LeadPosition = PlayerPosition.East,
            LeadSuit = Suit.Hearts,
        });
        hand.CompletedTricks.Add(new Trick
        {
            LeadPosition = PlayerPosition.West,
            LeadSuit = Suit.Spades,
        });

        var relative = hand.ToRelative(PlayerPosition.North);

        relative.CompletedTricks.Should().HaveCount(2);
        relative.CompletedTricks[0].LeadPosition.Should().Be(RelativePlayerPosition.LeftHandOpponent);
        relative.CompletedTricks[1].LeadPosition.Should().Be(RelativePlayerPosition.RightHandOpponent);
    }

    [Theory]
    [InlineData(PlayerPosition.North)]
    [InlineData(PlayerPosition.East)]
    [InlineData(PlayerPosition.South)]
    [InlineData(PlayerPosition.West)]
    public void HandToRelativeShouldWorkFromAnyPerspective(PlayerPosition self)
    {
        var hand = new Hand
        {
            DealerPosition = PlayerPosition.North,
            CallingPlayer = PlayerPosition.South,
        };

        var relative = hand.ToRelative(self);

        relative.DealerPosition.Should().Be(PlayerPosition.North.ToRelativePosition(self));
        relative.CallingPlayer.Should().Be(PlayerPosition.South.ToRelativePosition(self));
    }
}
