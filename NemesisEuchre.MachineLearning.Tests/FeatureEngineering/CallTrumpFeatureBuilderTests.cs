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
            validDecisions: [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp],
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

    [Fact]
    public void BuildFeatures_SetsValidityArrayForValidDecisions()
    {
        var cards = CreateDefaultHand();
        var validDecisions = new[]
        {
            CallTrumpDecision.Pass,
            CallTrumpDecision.CallSpades,
            CallTrumpDecision.CallSpadesAndGoAlone,
        };

        var result = CallTrumpFeatureBuilder.BuildFeatures(
            cards,
            upCard: new Card(Suit.Hearts, Rank.Ace),
            dealerPosition: RelativePlayerPosition.Self,
            teamScore: 0,
            opponentScore: 0,
            decisionOrder: 1.0f,
            validDecisions: validDecisions,
            chosenDecision: CallTrumpDecision.Pass);

        result.Decision0IsValid.Should().Be(1.0f);
        result.Decision1IsValid.Should().Be(1.0f);
        result.Decision2IsValid.Should().Be(0.0f);
        result.Decision3IsValid.Should().Be(0.0f);
        result.Decision4IsValid.Should().Be(0.0f);
        result.Decision5IsValid.Should().Be(1.0f);
        result.Decision6IsValid.Should().Be(0.0f);
        result.Decision7IsValid.Should().Be(0.0f);
        result.Decision8IsValid.Should().Be(0.0f);
        result.Decision9IsValid.Should().Be(0.0f);
        result.Decision10IsValid.Should().Be(0.0f);
    }

    [Theory]
    [InlineData(CallTrumpDecision.Pass, 0)]
    [InlineData(CallTrumpDecision.CallHearts, 2)]
    [InlineData(CallTrumpDecision.OrderItUp, 9)]
    [InlineData(CallTrumpDecision.OrderItUpAndGoAlone, 10)]
    public void BuildFeatures_SetsChosenDecisionOneHot(CallTrumpDecision decision, int expectedIndex)
    {
        var cards = CreateDefaultHand();
        var allDecisions = Enum.GetValues<CallTrumpDecision>();

        var result = CallTrumpFeatureBuilder.BuildFeatures(
            cards,
            upCard: new Card(Suit.Hearts, Rank.Ace),
            dealerPosition: RelativePlayerPosition.Self,
            teamScore: 0,
            opponentScore: 0,
            decisionOrder: 1.0f,
            validDecisions: allDecisions,
            chosenDecision: decision);

        result.Decision0Chosen.Should().Be(expectedIndex == 0 ? 1.0f : 0.0f);
        result.Decision1Chosen.Should().Be(expectedIndex == 1 ? 1.0f : 0.0f);
        result.Decision2Chosen.Should().Be(expectedIndex == 2 ? 1.0f : 0.0f);
        result.Decision3Chosen.Should().Be(expectedIndex == 3 ? 1.0f : 0.0f);
        result.Decision4Chosen.Should().Be(expectedIndex == 4 ? 1.0f : 0.0f);
        result.Decision5Chosen.Should().Be(expectedIndex == 5 ? 1.0f : 0.0f);
        result.Decision6Chosen.Should().Be(expectedIndex == 6 ? 1.0f : 0.0f);
        result.Decision7Chosen.Should().Be(expectedIndex == 7 ? 1.0f : 0.0f);
        result.Decision8Chosen.Should().Be(expectedIndex == 8 ? 1.0f : 0.0f);
        result.Decision9Chosen.Should().Be(expectedIndex == 9 ? 1.0f : 0.0f);
        result.Decision10Chosen.Should().Be(expectedIndex == 10 ? 1.0f : 0.0f);
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
            validDecisions: [CallTrumpDecision.Pass],
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
