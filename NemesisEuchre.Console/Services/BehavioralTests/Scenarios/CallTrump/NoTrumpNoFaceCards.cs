using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.CallTrump;

public class NoTrumpNoFaceCards(
    ICallTrumpInferenceFeatureBuilder featureBuilder)
    : CallTrumpBehavioralTest(featureBuilder)
{
    public override string Name => "No trump no face cards";

    public override string Description => "Holding no trump and no face cards, should pass";

    public override string AssertionDescription => "Should pass";

    protected override Card[] GetCardsInHand()
    {
        return [
        new(Suit.Hearts, Rank.Nine),
        new(Suit.Hearts, Rank.Ten),
        new(Suit.Diamonds, Rank.Nine),
        new(Suit.Diamonds, Rank.Ten),
        new(Suit.Clubs, Rank.Nine),
    ];
    }

    protected override Card GetUpCard()
    {
        return new(Suit.Spades, Rank.Ace);
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
        return chosenDecision == CallTrumpDecision.Pass;
    }
}
