using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.GameEngine.Tests.Validation;

public class TrickPlayingValidatorTests
{
    private readonly TrickPlayingValidator _validator = new();

    [Fact]
    public void ValidatePreconditions_WithNullDeal_ThrowsArgumentNullException()
    {
        var act = () => _validator.ValidatePreconditions(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(DealStatus.NotStarted)]
    [InlineData(DealStatus.SelectingTrump)]
    [InlineData(DealStatus.Scoring)]
    [InlineData(DealStatus.Complete)]
    public void ValidatePreconditions_WithIncorrectStatus_ThrowsInvalidOperationException(DealStatus status)
    {
        var deal = new Deal { DealStatus = status };

        var act = () => _validator.ValidatePreconditions(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Deal must be in Playing status, but was {status}");
    }

    [Fact]
    public void ValidatePreconditions_WithNullTrump_ThrowsInvalidOperationException()
    {
        var deal = new Deal
        {
            DealStatus = DealStatus.Playing,
            Trump = null,
        };

        var act = () => _validator.ValidatePreconditions(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Trump must be set");
    }

    [Fact]
    public void ValidatePreconditions_WithNullCallingPlayer_ThrowsInvalidOperationException()
    {
        var deal = new Deal
        {
            DealStatus = DealStatus.Playing,
            Trump = Suit.Hearts,
            CallingPlayer = null,
        };

        var act = () => _validator.ValidatePreconditions(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("CallingPlayer must be set");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void ValidatePreconditions_WithWrongNumberOfPlayers_ThrowsInvalidOperationException(int playerCount)
    {
        var deal = new Deal
        {
            DealStatus = DealStatus.Playing,
            Trump = Suit.Hearts,
            CallingPlayer = PlayerPosition.North,
        };

        for (int i = 0; i < playerCount; i++)
        {
            deal.Players.Add((PlayerPosition)i, new DealPlayer());
        }

        var act = () => _validator.ValidatePreconditions(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Deal must have exactly 4 players, but had {playerCount}");
    }

    [Fact]
    public void ValidatePreconditions_WithValidDeal_DoesNotThrow()
    {
        var deal = CreateValidDeal();

        var act = () => _validator.ValidatePreconditions(deal);

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateCardChoice_WithCardNotInValidCards_ThrowsInvalidOperationException()
    {
        var chosenCard = new Card { Suit = Suit.Spades, Rank = Rank.Ace };
        var validCards = new[]
        {
            new Card { Suit = Suit.Hearts, Rank = Rank.Jack },
            new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
        };

        var act = () => _validator.ValidateCardChoice(chosenCard, validCards);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ChosenCard was not included in ValidCards");
    }

    [Fact]
    public void ValidateCardChoice_WithCardInValidCards_DoesNotThrow()
    {
        var aceOfHearts = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var validCards = new[]
        {
            new Card { Suit = Suit.Hearts, Rank = Rank.Jack },
            aceOfHearts,
        };

        var act = () => _validator.ValidateCardChoice(aceOfHearts, validCards);

        act.Should().NotThrow();
    }

    private static Deal CreateValidDeal()
    {
        var deal = new Deal
        {
            DealStatus = DealStatus.Playing,
            Trump = Suit.Hearts,
            CallingPlayer = PlayerPosition.North,
        };

        deal.Players.Add(PlayerPosition.North, new DealPlayer());
        deal.Players.Add(PlayerPosition.East, new DealPlayer());
        deal.Players.Add(PlayerPosition.South, new DealPlayer());
        deal.Players.Add(PlayerPosition.West, new DealPlayer());

        return deal;
    }
}
