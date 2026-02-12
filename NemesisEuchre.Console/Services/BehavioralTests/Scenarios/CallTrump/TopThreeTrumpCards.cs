using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.CallTrump;

public class TopThreeTrumpCards(
    ICallTrumpInferenceFeatureBuilder featureBuilder)
    : CallTrumpBehavioralTest(featureBuilder)
{
    public override string Name => "Top three trump cards";

    public override string Description => "Holding right bower, left bower, and ace of trump, should not pass";

    public override string AssertionDescription => "Should not pass";

    protected override Card[] GetCardsInHand()
    {
        return [
        new(Suit.Spades, Rank.Jack),
        new(Suit.Clubs, Rank.Jack),
        new(Suit.Spades, Rank.Ace),
        new(Suit.Hearts, Rank.Nine),
        new(Suit.Diamonds, Rank.Ten),
    ];
    }

    protected override Card GetUpCard()
    {
        return new(Suit.Spades, Rank.Nine);
    }

    protected override CallTrumpDecision[] GetValidDecisions()
    {
        return [
        CallTrumpDecision.Pass,
        CallTrumpDecision.OrderItUp,
        CallTrumpDecision.OrderItUpAndGoAlone,
    ];
    }

    protected override bool IsExpectedChoice(CallTrumpDecision chosenDecision)
    {
        return chosenDecision != CallTrumpDecision.Pass;
    }
}
