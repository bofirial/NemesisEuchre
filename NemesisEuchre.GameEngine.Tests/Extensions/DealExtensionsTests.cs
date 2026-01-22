using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests.Extensions;

public class DealExtensionsTests
{
    [Fact]
    public void ToRelative_WithDeal_ConvertsUpCardToRelative()
    {
        var deal = new Deal
        {
            DealStatus = DealStatus.Playing,
            DealerPosition = PlayerPosition.West,
            UpCard = new Card { Suit = Suit.Diamonds, Rank = Rank.Nine },
            Trump = Suit.Diamonds,
            CallingPlayer = PlayerPosition.North,
            CallingPlayerIsGoingAlone = false,
        };

        var relative = deal.ToRelative(PlayerPosition.North);

        relative.DealStatus.Should().Be(DealStatus.Playing);
        relative.DealerPosition.Should().Be(RelativePlayerPosition.RightHandOpponent);
        relative.UpCard.Should().NotBeNull();
        relative.UpCard!.Rank.Should().Be(Rank.Nine);
        relative.UpCard.Suit.Should().Be(RelativeSuit.Trump);
        relative.CallingPlayer.Should().Be(RelativePlayerPosition.Self);
    }

    [Fact]
    public void ToRelative_WithTricks_ConvertsTricksWithTrumpContext()
    {
        var deal = new Deal
        {
            Trump = Suit.Hearts,
            CurrentTrick = new Trick
            {
                LeadPosition = PlayerPosition.South,
                LeadSuit = Suit.Clubs,
            },
        };
        deal.CurrentTrick.CardsPlayed.Add(new PlayedCard
        {
            Card = new Card { Suit = Suit.Clubs, Rank = Rank.Ten },
            PlayerPosition = PlayerPosition.South,
        });

        var relative = deal.ToRelative(PlayerPosition.North);

        relative.CurrentTrick.Should().NotBeNull();
        relative.CurrentTrick!.LeadSuit.Should().Be(RelativeSuit.NonTrumpOppositeColor2);
        relative.CurrentTrick.CardsPlayed[0].RelativeCard.Suit.Should().Be(RelativeSuit.NonTrumpOppositeColor2);
    }

    [Fact]
    public void ToRelative_WhenTrumpNotSet_ThrowsArgumentException()
    {
        var deal = new Deal
        {
            DealStatus = DealStatus.SelectingTrump,
            Trump = null,
            CurrentTrick = new Trick
            {
                LeadPosition = PlayerPosition.South,
                LeadSuit = Suit.Clubs,
            },
        };

        var act = () => deal.ToRelative(PlayerPosition.North);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Cannot create a relative deal until trump has been called");
    }

    [Fact]
    public void ToRelative_WithTrumpSet_ConvertsAllTricks()
    {
        var deal = new Deal
        {
            Trump = Suit.Spades,
            CompletedTricks =
            [
                new Trick
                {
                    LeadPosition = PlayerPosition.East,
                    LeadSuit = Suit.Hearts,
                },
            ],
        };

        var relative = deal.ToRelative(PlayerPosition.North);

        relative.CompletedTricks.Should().HaveCount(1);
        relative.CompletedTricks[0].LeadSuit.Should().Be(RelativeSuit.NonTrumpOppositeColor1);
    }

    [Theory]
    [InlineData(PlayerPosition.North)]
    [InlineData(PlayerPosition.East)]
    [InlineData(PlayerPosition.South)]
    [InlineData(PlayerPosition.West)]
    public void ToRelative_FromAnyPerspective_WorksCorrectly(PlayerPosition self)
    {
        var deal = new Deal
        {
            Trump = Suit.Hearts,
            DealerPosition = PlayerPosition.North,
            CallingPlayer = PlayerPosition.South,
        };

        var relative = deal.ToRelative(self);

        relative.DealerPosition.Should().Be(PlayerPosition.North.ToRelativePosition(self));
        relative.CallingPlayer.Should().Be(PlayerPosition.South.ToRelativePosition(self));
    }
}
