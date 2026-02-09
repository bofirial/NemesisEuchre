using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Services;

namespace NemesisEuchre.GameEngine.Tests.Services;

public class CardAccountingServiceTests
{
    private readonly CardAccountingService _service = new();

    [Fact]
    public void GetAccountedForCards_WithNoTricks_ReturnsOnlyPlayerHand()
    {
        var deal = new Deal
        {
            CompletedTricks = [],
            ChosenDecision = CallTrumpDecision.OrderItUp,
            DealerPosition = PlayerPosition.South,
        };
        var currentTrick = new Trick();
        var playerHand = new[]
        {
            new Card(Suit.Hearts, Rank.Nine),
            new Card(Suit.Hearts, Rank.Ten),
        };

        var result = _service.GetAccountedForCards(deal, currentTrick, PlayerPosition.North, playerHand);

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(playerHand);
    }

    [Fact]
    public void GetAccountedForCards_IncludesCompletedTricksCards()
    {
        var completedTrick = new Trick();
        completedTrick.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.Nine), PlayerPosition.North));
        completedTrick.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.Ten), PlayerPosition.East));

        var deal = new Deal
        {
            CompletedTricks = [completedTrick],
            ChosenDecision = CallTrumpDecision.OrderItUp,
            DealerPosition = PlayerPosition.South,
        };
        var currentTrick = new Trick();
        var playerHand = new[]
        {
            new Card(Suit.Spades, Rank.Nine),
        };

        var result = _service.GetAccountedForCards(deal, currentTrick, PlayerPosition.North, playerHand);

        result.Should().HaveCount(3);
        result.Should().Contain(completedTrick.CardsPlayed[0].Card);
        result.Should().Contain(completedTrick.CardsPlayed[1].Card);
    }

    [Fact]
    public void GetAccountedForCards_IncludesCurrentTrickCards()
    {
        var currentTrick = new Trick();
        currentTrick.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.Ace), PlayerPosition.North));
        currentTrick.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.King), PlayerPosition.East));

        var deal = new Deal
        {
            CompletedTricks = [],
            ChosenDecision = CallTrumpDecision.OrderItUp,
            DealerPosition = PlayerPosition.South,
        };
        var playerHand = new[]
        {
            new Card(Suit.Spades, Rank.Nine),
        };

        var result = _service.GetAccountedForCards(deal, currentTrick, PlayerPosition.South, playerHand);

        result.Should().HaveCount(3);
        result.Should().Contain(currentTrick.CardsPlayed[0].Card);
        result.Should().Contain(currentTrick.CardsPlayed[1].Card);
    }

    [Fact]
    public void GetAccountedForCards_Round1_DoesNotIncludeUpCard()
    {
        var deal = new Deal
        {
            CompletedTricks = [],
            ChosenDecision = CallTrumpDecision.OrderItUp,
            UpCard = new Card(Suit.Diamonds, Rank.Jack),
            DealerPosition = PlayerPosition.South,
        };
        var currentTrick = new Trick();
        var playerHand = new[]
        {
            new Card(Suit.Hearts, Rank.Nine),
        };

        var result = _service.GetAccountedForCards(deal, currentTrick, PlayerPosition.North, playerHand);

        result.Should().HaveCount(1);
        result.Should().NotContain(deal.UpCard);
    }

    [Fact]
    public void GetAccountedForCards_Round1GoAlone_DoesNotIncludeUpCard()
    {
        var deal = new Deal
        {
            CompletedTricks = [],
            ChosenDecision = CallTrumpDecision.OrderItUpAndGoAlone,
            UpCard = new Card(Suit.Diamonds, Rank.Jack),
            DealerPosition = PlayerPosition.South,
        };
        var currentTrick = new Trick();
        var playerHand = new[]
        {
            new Card(Suit.Hearts, Rank.Nine),
        };

        var result = _service.GetAccountedForCards(deal, currentTrick, PlayerPosition.North, playerHand);

        result.Should().HaveCount(1);
        result.Should().NotContain(deal.UpCard);
    }

    [Fact]
    public void GetAccountedForCards_Round2_IncludesUpCard()
    {
        var deal = new Deal
        {
            CompletedTricks = [],
            ChosenDecision = CallTrumpDecision.CallSpades,
            UpCard = new Card(Suit.Diamonds, Rank.Jack),
            DealerPosition = PlayerPosition.South,
        };
        var currentTrick = new Trick();
        var playerHand = new[]
        {
            new Card(Suit.Hearts, Rank.Nine),
        };

        var result = _service.GetAccountedForCards(deal, currentTrick, PlayerPosition.North, playerHand);

        result.Should().HaveCount(2);
        result.Should().Contain(deal.UpCard);
    }

    [Fact]
    public void GetAccountedForCards_Round2GoAlone_IncludesUpCard()
    {
        var deal = new Deal
        {
            CompletedTricks = [],
            ChosenDecision = CallTrumpDecision.CallSpadesAndGoAlone,
            UpCard = new Card(Suit.Diamonds, Rank.Jack),
            DealerPosition = PlayerPosition.South,
        };
        var currentTrick = new Trick();
        var playerHand = new[]
        {
            new Card(Suit.Hearts, Rank.Nine),
        };

        var result = _service.GetAccountedForCards(deal, currentTrick, PlayerPosition.North, playerHand);

        result.Should().HaveCount(2);
        result.Should().Contain(deal.UpCard);
    }

    [Fact]
    public void GetAccountedForCards_Round2WithNullUpCard_DoesNotIncludeUpCard()
    {
        var deal = new Deal
        {
            CompletedTricks = [],
            ChosenDecision = CallTrumpDecision.CallSpades,
            UpCard = null,
            DealerPosition = PlayerPosition.South,
        };
        var currentTrick = new Trick();
        var playerHand = new[]
        {
            new Card(Suit.Hearts, Rank.Nine),
        };

        var result = _service.GetAccountedForCards(deal, currentTrick, PlayerPosition.North, playerHand);

        result.Should().HaveCount(1);
    }

    [Fact]
    public void GetAccountedForCards_DealerWithDiscardedCard_IncludesDiscardedCard()
    {
        var deal = new Deal
        {
            CompletedTricks = [],
            ChosenDecision = CallTrumpDecision.OrderItUp,
            DealerPosition = PlayerPosition.South,
            DiscardedCard = new Card(Suit.Clubs, Rank.Nine),
        };
        var currentTrick = new Trick();
        var playerHand = new[]
        {
            new Card(Suit.Hearts, Rank.Nine),
        };

        var result = _service.GetAccountedForCards(deal, currentTrick, PlayerPosition.South, playerHand);

        result.Should().HaveCount(2);
        result.Should().Contain(deal.DiscardedCard);
    }

    [Fact]
    public void GetAccountedForCards_NonDealerWithDiscardedCard_DoesNotIncludeDiscardedCard()
    {
        var deal = new Deal
        {
            CompletedTricks = [],
            ChosenDecision = CallTrumpDecision.OrderItUp,
            DealerPosition = PlayerPosition.South,
            DiscardedCard = new Card(Suit.Clubs, Rank.Nine),
        };
        var currentTrick = new Trick();
        var playerHand = new[]
        {
            new Card(Suit.Hearts, Rank.Nine),
        };

        var result = _service.GetAccountedForCards(deal, currentTrick, PlayerPosition.North, playerHand);

        result.Should().HaveCount(1);
        result.Should().NotContain(deal.DiscardedCard);
    }

    [Fact]
    public void GetAccountedForCards_DealerWithNullDiscardedCard_DoesNotIncludeDiscardedCard()
    {
        var deal = new Deal
        {
            CompletedTricks = [],
            ChosenDecision = CallTrumpDecision.OrderItUp,
            DealerPosition = PlayerPosition.South,
            DiscardedCard = null,
        };
        var currentTrick = new Trick();
        var playerHand = new[]
        {
            new Card(Suit.Hearts, Rank.Nine),
        };

        var result = _service.GetAccountedForCards(deal, currentTrick, PlayerPosition.South, playerHand);

        result.Should().HaveCount(1);
    }

    [Fact]
    public void GetAccountedForCards_ComplexScenario_IncludesAllApplicableCards()
    {
        var completedTrick1 = new Trick();
        completedTrick1.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.Nine), PlayerPosition.North));
        completedTrick1.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.Ten), PlayerPosition.East));
        completedTrick1.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.Jack), PlayerPosition.South));
        completedTrick1.CardsPlayed.Add(new PlayedCard(new Card(Suit.Hearts, Rank.Queen), PlayerPosition.West));

        var completedTrick2 = new Trick();
        completedTrick2.CardsPlayed.Add(new PlayedCard(new Card(Suit.Spades, Rank.Ace), PlayerPosition.East));
        completedTrick2.CardsPlayed.Add(new PlayedCard(new Card(Suit.Spades, Rank.King), PlayerPosition.South));

        var currentTrick = new Trick();
        currentTrick.CardsPlayed.Add(new PlayedCard(new Card(Suit.Diamonds, Rank.Ace), PlayerPosition.West));
        var deal = new Deal
        {
            CompletedTricks = [completedTrick1, completedTrick2],
            ChosenDecision = CallTrumpDecision.CallSpades,
            UpCard = new Card(Suit.Clubs, Rank.Jack),
            DealerPosition = PlayerPosition.South,
            DiscardedCard = new Card(Suit.Clubs, Rank.Nine),
        };
        var playerHand = new[]
        {
            new Card(Suit.Diamonds, Rank.Nine),
            new Card(Suit.Diamonds, Rank.Ten),
            new Card(Suit.Diamonds, Rank.King),
        };

        var result = _service.GetAccountedForCards(deal, currentTrick, PlayerPosition.South, playerHand);

        result.Should().HaveCount(12);
        result.Should().Contain(completedTrick1.CardsPlayed.Select(pc => pc.Card));
        result.Should().Contain(completedTrick2.CardsPlayed.Select(pc => pc.Card));
        result.Should().Contain(currentTrick.CardsPlayed.Select(pc => pc.Card));
        result.Should().Contain(playerHand);
        result.Should().Contain(deal.UpCard);
        result.Should().Contain(deal.DiscardedCard);
    }
}
