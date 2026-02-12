using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.CallTrump;

public class PerfectHandGoAlone(
    ICallTrumpInferenceFeatureBuilder featureBuilder)
    : CallTrumpBehavioralTest(featureBuilder)
{
    public override string Name => "Perfect hand go alone";

    public override string Description => "Holding all 5 top trump cards, should go alone";

    public override string AssertionDescription => "Should go alone";

    protected override Card[] GetCardsInHand()
    {
        return [
        new(Suit.Spades, Rank.Jack),
        new(Suit.Clubs, Rank.Jack),
        new(Suit.Spades, Rank.Ace),
        new(Suit.Spades, Rank.King),
        new(Suit.Spades, Rank.Queen),
    ];
    }

    protected override Card GetUpCard()
    {
        return new(Suit.Spades, Rank.Ten);
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
        return chosenDecision == CallTrumpDecision.OrderItUpAndGoAlone;
    }
}
