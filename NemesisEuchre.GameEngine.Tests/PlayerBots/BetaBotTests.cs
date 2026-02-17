using FluentAssertions;

using Moq;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerBots;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;

namespace NemesisEuchre.GameEngine.Tests.PlayerBots;

public class BetaBotTests
{
    private readonly Mock<IRandomNumberGenerator> _mockRandom = new();
    private readonly BetaBot _bot;

    public BetaBotTests()
    {
        _bot = new BetaBot(_mockRandom.Object);
    }

    [Fact]
    public void ActorType_ShouldReturnBeta()
    {
        _bot.ActorType.Should().Be(ActorType.Beta);
    }

    [Fact]
    public async Task CallTrumpAsync_ShouldPreferPass_WhenPassIsAvailable()
    {
        var validDecisions = new[] { CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp };

        var result = await _bot.CallTrumpAsync([], 0, 0, RelativePlayerPosition.Self, new Card(Suit.Hearts, Rank.Nine), validDecisions, 1);

        result.ChosenCallTrumpDecision.Should().Be(CallTrumpDecision.Pass);
    }

    [Fact]
    public async Task CallTrumpAsync_ShouldSelectRandom_WhenPassIsNotAvailable()
    {
        var validDecisions = new[] { CallTrumpDecision.CallHearts, CallTrumpDecision.CallSpades };
        _mockRandom.Setup(x => x.NextInt(2)).Returns(1);

        var result = await _bot.CallTrumpAsync([], 0, 0, RelativePlayerPosition.Self, new Card(Suit.Hearts, Rank.Nine), validDecisions, 1);

        result.ChosenCallTrumpDecision.Should().Be(CallTrumpDecision.CallSpades);
    }

    [Fact]
    public async Task DiscardCardAsync_ShouldSelectLowestNonTrumpCard()
    {
        var cards = new[]
        {
            new RelativeCard(Rank.King, RelativeSuit.Trump) { Card = new Card(Suit.Hearts, Rank.King) },
            new RelativeCard(Rank.Nine, RelativeSuit.NonTrumpOppositeColor1) { Card = new Card(Suit.Clubs, Rank.Nine) },
            new RelativeCard(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1) { Card = new Card(Suit.Clubs, Rank.Queen) },
        };

        var result = await _bot.DiscardCardAsync(cards, 0, 0, RelativePlayerPosition.Self, false, cards);

        result.ChosenCard.Rank.Should().Be(Rank.Nine);
        result.ChosenCard.Suit.Should().Be(RelativeSuit.NonTrumpOppositeColor1);
    }

    [Fact]
    public async Task PlayCardAsync_ShouldSelectLowestNonTrumpCard()
    {
        var cards = new[]
        {
            new RelativeCard(Rank.Ace, RelativeSuit.Trump) { Card = new Card(Suit.Hearts, Rank.Ace) },
            new RelativeCard(Rank.Ten, RelativeSuit.NonTrumpOppositeColor2) { Card = new Card(Suit.Diamonds, Rank.Ten) },
            new RelativeCard(Rank.Queen, RelativeSuit.NonTrumpOppositeColor2) { Card = new Card(Suit.Diamonds, Rank.Queen) },
        };
        var playedCards = new Dictionary<RelativePlayerPosition, RelativeCard>();

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
            0,
            0,
            cards);

        result.ChosenCard.Rank.Should().Be(Rank.Ten);
    }
}
