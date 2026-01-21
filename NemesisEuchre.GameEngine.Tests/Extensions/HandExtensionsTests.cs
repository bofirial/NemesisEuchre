using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests.Extensions;

public class HandExtensionsTests
{
    [Fact]
    public void HandToRelativeShouldConvertUpCardToRelative()
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
        relative.UpCard.Should().NotBeNull();
        relative.UpCard!.Rank.Should().Be(Rank.Nine);
        relative.UpCard.Suit.Should().Be(RelativeSuit.Trump);
        relative.CallingPlayer.Should().Be(RelativePlayerPosition.Self);
    }

    [Fact]
    public void HandToRelativeShouldConvertTricksWithTrumpContext()
    {
        var hand = new Hand
        {
            Trump = Suit.Hearts,
            CurrentTrick = new Trick
            {
                LeadPosition = PlayerPosition.South,
                LeadSuit = Suit.Clubs,
            },
        };
        hand.CurrentTrick.CardsPlayed.Add(new PlayedCard
        {
            Card = new Card { Suit = Suit.Clubs, Rank = Rank.Ten },
            PlayerPosition = PlayerPosition.South,
        });

        var relative = hand.ToRelative(PlayerPosition.North);

        relative.CurrentTrick.Should().NotBeNull();
        relative.CurrentTrick!.LeadSuit.Should().Be(RelativeSuit.NonTrumpOppositeColor2);
        relative.CurrentTrick.CardsPlayed[0].RelativeCard.Suit.Should().Be(RelativeSuit.NonTrumpOppositeColor2);
    }

    [Fact]
    public void HandToRelativeShouldThrowWhenTrumpNotSet()
    {
        var hand = new Hand
        {
            HandStatus = HandStatus.SelectingTrump,
            Trump = null,
            CurrentTrick = new Trick
            {
                LeadPosition = PlayerPosition.South,
                LeadSuit = Suit.Clubs,
            },
        };

        var act = () => hand.ToRelative(PlayerPosition.North);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Cannot create a relative hand until trump has been called");
    }

    [Fact]
    public void HandToRelativeShouldConvertAllTricksWhenTrumpSet()
    {
        var hand = new Hand
        {
            Trump = Suit.Spades,
        };
        hand.CompletedTricks.Add(new Trick
        {
            LeadPosition = PlayerPosition.East,
            LeadSuit = Suit.Hearts,
        });

        var relative = hand.ToRelative(PlayerPosition.North);

        relative.CompletedTricks.Should().HaveCount(1);
        relative.CompletedTricks[0].LeadSuit.Should().Be(RelativeSuit.NonTrumpOppositeColor1);
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
            Trump = Suit.Hearts,
            DealerPosition = PlayerPosition.North,
            CallingPlayer = PlayerPosition.South,
        };

        var relative = hand.ToRelative(self);

        relative.DealerPosition.Should().Be(PlayerPosition.North.ToRelativePosition(self));
        relative.CallingPlayer.Should().Be(PlayerPosition.South.ToRelativePosition(self));
    }
}
