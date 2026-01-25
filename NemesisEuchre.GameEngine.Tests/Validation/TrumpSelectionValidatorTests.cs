using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.GameEngine.Tests.Validation;

public class TrumpSelectionValidatorTests
{
    private readonly TrumpSelectionValidator _validator = new();

    [Fact]
    public void ValidatePreconditions_WithNullDeal_ThrowsArgumentNullException()
    {
        var act = () => _validator.ValidatePreconditions(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(DealStatus.NotStarted)]
    [InlineData(DealStatus.Playing)]
    [InlineData(DealStatus.Scoring)]
    [InlineData(DealStatus.Complete)]
    public void ValidatePreconditions_WithIncorrectStatus_ThrowsInvalidOperationException(DealStatus status)
    {
        var deal = new Deal { DealStatus = status };

        var act = () => _validator.ValidatePreconditions(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Deal must be in SelectingTrump status, but was {status}");
    }

    [Fact]
    public void ValidatePreconditions_WithNullDealerPosition_ThrowsInvalidOperationException()
    {
        var deal = new Deal
        {
            DealStatus = DealStatus.SelectingTrump,
            DealerPosition = null,
        };

        var act = () => _validator.ValidatePreconditions(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("DealerPosition must be set");
    }

    [Fact]
    public void ValidatePreconditions_WithNullUpCard_ThrowsInvalidOperationException()
    {
        var deal = new Deal
        {
            DealStatus = DealStatus.SelectingTrump,
            DealerPosition = PlayerPosition.North,
            UpCard = null,
        };

        var act = () => _validator.ValidatePreconditions(deal);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("UpCard must be set");
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
            DealStatus = DealStatus.SelectingTrump,
            DealerPosition = PlayerPosition.North,
            UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
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
    public void ValidateDecision_WithDecisionNotInValidDecisions_ThrowsInvalidOperationException()
    {
        const CallTrumpDecision decision = CallTrumpDecision.CallClubs;
        var validDecisions = new[] { CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp };

        var act = () => _validator.ValidateDecision(decision, validDecisions);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("CallTrumpDecision was not included in ValidDecisions");
    }

    [Fact]
    public void ValidateDecision_WithDecisionInValidDecisions_DoesNotThrow()
    {
        const CallTrumpDecision decision = CallTrumpDecision.OrderItUp;
        var validDecisions = new[] { CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp };

        var act = () => _validator.ValidateDecision(decision, validDecisions);

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateDiscard_WithCardNotInValidCards_ThrowsInvalidOperationException()
    {
        const Suit trump = Suit.Hearts;
        var cardToDiscard = new Card { Suit = Suit.Spades, Rank = Rank.Ace }.ToRelative(trump);
        var validCards = new[]
        {
            new Card { Suit = Suit.Hearts, Rank = Rank.Jack }.ToRelative(trump),
            new Card { Suit = Suit.Hearts, Rank = Rank.Ace }.ToRelative(trump),
        };

        var act = () => _validator.ValidateDiscard(cardToDiscard, validCards);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("CardToDiscard was not included in ValidCardsToDiscard");
    }

    [Fact]
    public void ValidateDiscard_WithCardInValidCards_DoesNotThrow()
    {
        const Suit trump = Suit.Hearts;
        var aceOfHearts = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };
        var relativeAce = aceOfHearts.ToRelative(trump);
        var validCards = new[]
        {
            new Card { Suit = Suit.Hearts, Rank = Rank.Jack }.ToRelative(trump),
            relativeAce,
        };

        var act = () => _validator.ValidateDiscard(relativeAce, validCards);

        act.Should().NotThrow();
    }

    private static Deal CreateValidDeal()
    {
        var deal = new Deal
        {
            DealStatus = DealStatus.SelectingTrump,
            DealerPosition = PlayerPosition.North,
            UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
        };

        deal.Players.Add(PlayerPosition.North, new DealPlayer());
        deal.Players.Add(PlayerPosition.East, new DealPlayer());
        deal.Players.Add(PlayerPosition.South, new DealPlayer());
        deal.Players.Add(PlayerPosition.West, new DealPlayer());

        return deal;
    }
}
