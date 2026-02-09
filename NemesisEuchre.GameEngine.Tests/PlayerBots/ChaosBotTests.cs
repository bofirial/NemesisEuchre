using FluentAssertions;

using Moq;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerBots;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;

namespace NemesisEuchre.GameEngine.Tests.PlayerBots;

public class ChaosBotTests
{
    private readonly Mock<IRandomNumberGenerator> _mockRandom = new();
    private readonly ChaosBot _bot;

    public ChaosBotTests()
    {
        _bot = new ChaosBot(_mockRandom.Object);
    }

    [Fact]
    public void ActorType_ShouldReturnChaos()
    {
        _bot.ActorType.Should().Be(ActorType.Chaos);
    }

    [Fact]
    public async Task CallTrumpAsync_ShouldSelectRandomDecision()
    {
        var validDecisions = new[] { CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp, CallTrumpDecision.OrderItUpAndGoAlone };
        _mockRandom.Setup(x => x.NextInt(3)).Returns(2);

        var result = await _bot.CallTrumpAsync([], 0, 0, RelativePlayerPosition.Self, new Card(Suit.Hearts, Rank.Nine), validDecisions);

        result.ChosenCallTrumpDecision.Should().Be(CallTrumpDecision.OrderItUpAndGoAlone);
    }

    [Fact]
    public async Task CallTrumpAsync_ShouldReturnZeroPredictedPointsForAllDecisions()
    {
        var validDecisions = new[] { CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp };
        _mockRandom.Setup(x => x.NextInt(2)).Returns(0);

        var result = await _bot.CallTrumpAsync([], 0, 0, RelativePlayerPosition.Self, new Card(Suit.Hearts, Rank.Nine), validDecisions);

        result.DecisionPredictedPoints.Should().HaveCount(2);
        result.DecisionPredictedPoints.Values.Should().AllBeEquivalentTo(0f);
    }

    [Fact]
    public async Task DiscardCardAsync_ShouldSelectRandomCard()
    {
        var cards = new[]
        {
            new RelativeCard(Rank.Ace, RelativeSuit.Trump) { Card = new Card(Suit.Hearts, Rank.Ace) },
            new RelativeCard(Rank.Nine, RelativeSuit.NonTrumpOppositeColor1) { Card = new Card(Suit.Clubs, Rank.Nine) },
        };
        _mockRandom.Setup(x => x.NextInt(2)).Returns(1);

        var result = await _bot.DiscardCardAsync(cards, 0, 0, RelativePlayerPosition.Self, false, cards);

        result.ChosenCard.Should().Be(cards[1]);
    }

    [Fact]
    public async Task PlayCardAsync_ShouldSelectRandomCard()
    {
        var cards = new[]
        {
            new RelativeCard(Rank.Ace, RelativeSuit.Trump) { Card = new Card(Suit.Hearts, Rank.Ace) },
            new RelativeCard(Rank.Nine, RelativeSuit.NonTrumpOppositeColor1) { Card = new Card(Suit.Clubs, Rank.Nine) },
            new RelativeCard(Rank.King, RelativeSuit.NonTrumpOppositeColor2) { Card = new Card(Suit.Diamonds, Rank.King) },
        };
        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>();
        _mockRandom.Setup(x => x.NextInt(3)).Returns(2);

        var result = await _bot.PlayCardAsync(
            cards,
            0,
            0,
            RelativePlayerPosition.Self,
            false,
            RelativePlayerPosition.LeftHandOpponent,
            null,
            RelativePlayerPosition.Self,
            null,
            [],
            [],
            playedCards,
            null,
            1,
            cards);

        result.ChosenCard.Should().Be(cards[2]);
    }
}
