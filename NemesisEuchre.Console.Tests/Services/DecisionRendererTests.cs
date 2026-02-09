using FluentAssertions;

using NemesisEuchre.Console.Services;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.Console.Tests.Services;

public class DecisionRendererTests
{
    private readonly DecisionRenderer _renderer = new();

    [Fact]
    public void RenderDecisions_ShouldReturnRenderables_ForDealWithCallTrumpDecisions()
    {
        var deal = CreateDealWithCallTrumpDecisions();

        var result = _renderer.RenderDecisions(deal);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public void RenderDecisions_ShouldIncludePlayCardSection_ForDealWithTricks()
    {
        var deal = CreateDealWithCallTrumpDecisions();

        var result = _renderer.RenderDecisions(deal);

        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void RenderDecisions_ShouldIncludeDiscardSection_WhenDiscardDecisionsExist()
    {
        var deal = CreateDealWithCallTrumpDecisions();
        deal.DiscardCardDecisions.Add(new DiscardCardDecisionRecord
        {
            PlayerPosition = PlayerPosition.North,
            CardsInHand = [new Card(Suit.Hearts, Rank.Ace), new Card(Suit.Spades, Rank.King)],
            ChosenCard = new Card(Suit.Hearts, Rank.Ace),
            DecisionPredictedPoints = new Dictionary<Card, float>
            {
                { new Card(Suit.Hearts, Rank.Ace), 1.5f },
                { new Card(Suit.Spades, Rank.King), 0.8f },
            },
        });

        var result = _renderer.RenderDecisions(deal);

        result.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void RenderDecisions_ShouldNotIncludeDiscardSection_WhenNoDiscardDecisions()
    {
        var deal = CreateDealWithCallTrumpDecisions();

        var withoutDiscard = _renderer.RenderDecisions(deal);

        deal.DiscardCardDecisions.Add(new DiscardCardDecisionRecord
        {
            PlayerPosition = PlayerPosition.North,
            CardsInHand = [new Card(Suit.Hearts, Rank.Ace)],
            ChosenCard = new Card(Suit.Hearts, Rank.Ace),
            DecisionPredictedPoints = new Dictionary<Card, float>
            {
                { new Card(Suit.Hearts, Rank.Ace), 1.0f },
            },
        });

        var withDiscard = _renderer.RenderDecisions(deal);

        withDiscard.Count.Should().BeGreaterThan(withoutDiscard.Count);
    }

    [Fact]
    public void RenderDecisions_ShouldHandleRound1Decisions()
    {
        var deal = CreateDealWithCallTrumpDecisions();
        deal.CallTrumpDecisions.Add(new CallTrumpDecisionRecord
        {
            PlayerPosition = PlayerPosition.North,
            DecisionOrder = 1,
            CardsInHand = [new Card(Suit.Hearts, Rank.Ace)],
            UpCard = new Card(Suit.Hearts, Rank.Nine),
            ChosenDecision = CallTrumpDecision.Pass,
            ValidCallTrumpDecisions = [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp],
            DecisionPredictedPoints = new Dictionary<CallTrumpDecision, float>
            {
                { CallTrumpDecision.Pass, 1.0f },
                { CallTrumpDecision.OrderItUp, 0.5f },
            },
        });

        var result = _renderer.RenderDecisions(deal);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public void RenderDecisions_ShouldHandleRound2Decisions()
    {
        var deal = CreateDealWithCallTrumpDecisions();
        deal.CallTrumpDecisions.Add(new CallTrumpDecisionRecord
        {
            PlayerPosition = PlayerPosition.North,
            DecisionOrder = 5,
            CardsInHand = [new Card(Suit.Hearts, Rank.Ace)],
            UpCard = new Card(Suit.Hearts, Rank.Nine),
            ChosenDecision = CallTrumpDecision.CallSpades,
            ValidCallTrumpDecisions = [CallTrumpDecision.Pass, CallTrumpDecision.CallSpades, CallTrumpDecision.CallClubs],
            DecisionPredictedPoints = new Dictionary<CallTrumpDecision, float>
            {
                { CallTrumpDecision.Pass, 0.5f },
                { CallTrumpDecision.CallSpades, 1.0f },
                { CallTrumpDecision.CallClubs, 0.3f },
            },
        });

        var result = _renderer.RenderDecisions(deal);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public void RenderDecisions_ShouldHandleRound2Decision_WithoutPassAvailable()
    {
        var deal = CreateDealWithCallTrumpDecisions();
        deal.CallTrumpDecisions.Add(new CallTrumpDecisionRecord
        {
            PlayerPosition = PlayerPosition.North,
            DecisionOrder = 8,
            CardsInHand = [new Card(Suit.Hearts, Rank.Ace)],
            UpCard = new Card(Suit.Hearts, Rank.Nine),
            ChosenDecision = CallTrumpDecision.CallSpades,
            ValidCallTrumpDecisions = [CallTrumpDecision.CallSpades, CallTrumpDecision.CallClubs],
            DecisionPredictedPoints = new Dictionary<CallTrumpDecision, float>
            {
                { CallTrumpDecision.CallSpades, 1.0f },
            },
        });

        var result = _renderer.RenderDecisions(deal);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public void RenderDecisions_ShouldHandlePlayCardDecisions()
    {
        var deal = CreateDealWithCallTrumpDecisions();
        var trick = deal.CompletedTricks[0];
        trick.PlayCardDecisions.Add(new PlayCardDecisionRecord
        {
            PlayerPosition = PlayerPosition.North,
            CardsInHand = [new Card(Suit.Hearts, Rank.Ace)],
            ChosenCard = new Card(Suit.Hearts, Rank.Ace),
            LeadSuit = Suit.Hearts,
            ValidCardsToPlay = [new Card(Suit.Hearts, Rank.Ace)],
            DecisionPredictedPoints = new Dictionary<Card, float>
            {
                { new Card(Suit.Hearts, Rank.Ace), 1.5f },
            },
        });

        var result = _renderer.RenderDecisions(deal);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public void RenderDecisions_ShouldHandleEmptyDeal()
    {
        var deal = CreateDealWithCallTrumpDecisions();

        var result = _renderer.RenderDecisions(deal);

        result.Should().NotBeNull();
    }

    [Fact]
    public void RenderDecisions_ShouldHandleMultipleCardsInPlayCardDecision()
    {
        var deal = CreateDealWithCallTrumpDecisions();
        var trick = deal.CompletedTricks[0];
        var card1 = new Card(Suit.Hearts, Rank.Ace);
        var card2 = new Card(Suit.Spades, Rank.King);
        var card3 = new Card(Suit.Clubs, Rank.Queen);
        trick.PlayCardDecisions.Add(new PlayCardDecisionRecord
        {
            PlayerPosition = PlayerPosition.North,
            CardsInHand = [card1, card2, card3],
            ChosenCard = card1,
            LeadSuit = Suit.Hearts,
            ValidCardsToPlay = [card1, card2, card3],
            DecisionPredictedPoints = new Dictionary<Card, float>
            {
                { card1, 1.5f },
                { card2, 0.8f },
                { card3, 0.3f },
            },
        });

        var result = _renderer.RenderDecisions(deal);

        result.Should().NotBeEmpty();
    }

    private static Deal CreateDealWithCallTrumpDecisions()
    {
        var trick = new Trick { TrickNumber = 1, LeadPosition = PlayerPosition.North, WinningPosition = PlayerPosition.North };

        return new Deal
        {
            Trump = Suit.Hearts,
            DealerPosition = PlayerPosition.North,
            CallingPlayer = PlayerPosition.East,
            ChosenDecision = CallTrumpDecision.CallHearts,
            UpCard = new Card(Suit.Hearts, Rank.Nine),
            WinningTeam = Team.Team1,
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                { PlayerPosition.North, new DealPlayer { Position = PlayerPosition.North, StartingHand = [], ActorType = ActorType.Chaos } },
                { PlayerPosition.East, new DealPlayer { Position = PlayerPosition.East, StartingHand = [], ActorType = ActorType.Chaos } },
                { PlayerPosition.South, new DealPlayer { Position = PlayerPosition.South, StartingHand = [], ActorType = ActorType.Chaos } },
                { PlayerPosition.West, new DealPlayer { Position = PlayerPosition.West, StartingHand = [], ActorType = ActorType.Chaos } },
            },
            CompletedTricks = [trick],
        };
    }
}
