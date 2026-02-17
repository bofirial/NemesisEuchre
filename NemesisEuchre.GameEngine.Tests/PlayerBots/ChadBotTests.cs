using FluentAssertions;

using Moq;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerBots;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;

namespace NemesisEuchre.GameEngine.Tests.PlayerBots;

public class ChadBotTests
{
    private readonly Mock<IRandomNumberGenerator> _mockRandom = new();
    private readonly ChadBot _bot;

    public ChadBotTests()
    {
        _bot = new ChadBot(_mockRandom.Object);
    }

    [Fact]
    public void ActorType_ShouldReturnChad()
    {
        _bot.ActorType.Should().Be(ActorType.Chad);
    }

    [Fact]
    public async Task CallTrumpAsync_ShouldPreferOrderItUpAndGoAlone_WhenPassIsAvailable()
    {
        var validDecisions = new[] { CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp, CallTrumpDecision.OrderItUpAndGoAlone };

        var result = await _bot.CallTrumpAsync([], 0, 0, RelativePlayerPosition.Self, new Card(Suit.Hearts, Rank.Nine), validDecisions, 1);

        result.ChosenCallTrumpDecision.Should().Be(CallTrumpDecision.OrderItUpAndGoAlone);
    }

    [Fact]
    public async Task CallTrumpAsync_ShouldSelectRandom_WhenPassIsNotAvailable()
    {
        var validDecisions = new[] { CallTrumpDecision.CallHearts, CallTrumpDecision.CallSpades };
        _mockRandom.Setup(x => x.NextInt(2)).Returns(0);

        var result = await _bot.CallTrumpAsync([], 0, 0, RelativePlayerPosition.Self, new Card(Suit.Hearts, Rank.Nine), validDecisions, 1);

        result.ChosenCallTrumpDecision.Should().Be(CallTrumpDecision.CallHearts);
    }

    [Fact]
    public async Task PlayCardAsync_ShouldSelectHighestTrumpCard_WhenTrumpAvailable()
    {
        var cards = new[]
        {
            new RelativeCard(Rank.Nine, RelativeSuit.Trump) { Card = new Card(Suit.Hearts, Rank.Nine) },
            new RelativeCard(Rank.Ace, RelativeSuit.Trump) { Card = new Card(Suit.Hearts, Rank.Ace) },
            new RelativeCard(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1) { Card = new Card(Suit.Clubs, Rank.Queen) },
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

        result.ChosenCard.Rank.Should().Be(Rank.Ace);
        result.ChosenCard.Suit.Should().Be(RelativeSuit.Trump);
    }

    [Fact]
    public async Task PlayCardAsync_ShouldSelectHighestCard_WhenNoTrumpAvailable()
    {
        var cards = new[]
        {
            new RelativeCard(Rank.Nine, RelativeSuit.NonTrumpOppositeColor1) { Card = new Card(Suit.Clubs, Rank.Nine) },
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

        result.ChosenCard.Rank.Should().Be(Rank.Queen);
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
}
