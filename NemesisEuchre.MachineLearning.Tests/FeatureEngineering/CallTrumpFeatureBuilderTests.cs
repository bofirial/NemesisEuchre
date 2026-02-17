using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.MachineLearning.Tests.FeatureEngineering;

public class CallTrumpFeatureBuilderTests
{
    [Fact]
    public void BuildFeatures_MapsAllCardRanksAndSuits()
    {
        var cards = new Card[]
        {
            new(Suit.Spades, Rank.Ace),
            new(Suit.Hearts, Rank.King),
            new(Suit.Clubs, Rank.Queen),
            new(Suit.Diamonds, Rank.Ten),
            new(Suit.Spades, Rank.Nine),
        };

        var result = CallTrumpFeatureBuilder.BuildFeatures(
            cards,
            upCard: new Card(Suit.Hearts, Rank.Jack),
            dealerPosition: RelativePlayerPosition.Partner,
            teamScore: 3,
            opponentScore: 5,
            decisionOrder: 2.0f,
            chosenDecision: CallTrumpDecision.Pass);

        result.Card1Rank.Should().Be((float)Rank.Ace);
        result.Card1Suit.Should().Be((float)Suit.Spades);
        result.Card2Rank.Should().Be((float)Rank.King);
        result.Card2Suit.Should().Be((float)Suit.Hearts);
        result.Card3Rank.Should().Be((float)Rank.Queen);
        result.Card3Suit.Should().Be((float)Suit.Clubs);
        result.Card4Rank.Should().Be((float)Rank.Ten);
        result.Card4Suit.Should().Be((float)Suit.Diamonds);
        result.Card5Rank.Should().Be((float)Rank.Nine);
        result.Card5Suit.Should().Be((float)Suit.Spades);
    }

    [Theory]
    [InlineData(CallTrumpDecision.Pass, 0)]
    [InlineData(CallTrumpDecision.CallSpades, 1)]
    [InlineData(CallTrumpDecision.CallHearts, 2)]
    [InlineData(CallTrumpDecision.CallClubs, 3)]
    [InlineData(CallTrumpDecision.CallDiamonds, 4)]
    [InlineData(CallTrumpDecision.CallSpadesAndGoAlone, 5)]
    [InlineData(CallTrumpDecision.CallHeartsAndGoAlone, 6)]
    [InlineData(CallTrumpDecision.CallClubsAndGoAlone, 7)]
    [InlineData(CallTrumpDecision.CallDiamondsAndGoAlone, 8)]
    [InlineData(CallTrumpDecision.OrderItUp, 9)]
    [InlineData(CallTrumpDecision.OrderItUpAndGoAlone, 10)]
    public void BuildFeatures_SetsChosenDecision(CallTrumpDecision decision, int expectedValue)
    {
        var cards = CreateDefaultHand();

        var result = CallTrumpFeatureBuilder.BuildFeatures(
            cards,
            upCard: new Card(Suit.Hearts, Rank.Ace),
            dealerPosition: RelativePlayerPosition.Self,
            teamScore: 0,
            opponentScore: 0,
            decisionOrder: 1.0f,
            chosenDecision: decision);

        result.ChosenDecision.Should().Be(expectedValue);
    }

    [Fact]
    public void BuildFeatures_MapsUpCardAndPositionalData()
    {
        var cards = CreateDefaultHand();

        var result = CallTrumpFeatureBuilder.BuildFeatures(
            cards,
            upCard: new Card(Suit.Diamonds, Rank.Queen),
            dealerPosition: RelativePlayerPosition.RightHandOpponent,
            teamScore: 7,
            opponentScore: 9,
            decisionOrder: 3.0f,
            chosenDecision: CallTrumpDecision.Pass);

        result.UpCardRank.Should().Be((float)Rank.Queen);
        result.UpCardSuit.Should().Be((float)Suit.Diamonds);
        result.DealerPosition.Should().Be((float)RelativePlayerPosition.RightHandOpponent);
        result.TeamScore.Should().Be(7);
        result.OpponentScore.Should().Be(9);
        result.DecisionOrder.Should().Be(3.0f);
    }

    private static Card[] CreateDefaultHand()
    {
        return
        [
            new(Suit.Spades, Rank.Ace),
            new(Suit.Hearts, Rank.King),
            new(Suit.Clubs, Rank.Queen),
            new(Suit.Diamonds, Rank.Ten),
            new(Suit.Spades, Rank.Nine),
        ];
    }
}
