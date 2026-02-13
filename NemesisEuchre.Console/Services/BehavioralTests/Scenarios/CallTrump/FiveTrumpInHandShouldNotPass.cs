using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.CallTrump;

public class FiveTrumpInHandShouldNotPass(
    ICallTrumpInferenceFeatureBuilder featureBuilder)
    : CallTrumpBehavioralTest(featureBuilder)
{
    public override string Name => "Five trump in hand";

    public override string Description => "Holding all 5 trump cards with a trump up card, should not pass";

    public override string AssertionDescription => "Should not pass";

    protected override IReadOnlyList<CallTrumpTestCase> GetTestCases()
    {
        return GenerateAllSuitVariants(
            Name,
            suit =>
            [
                new(suit, Rank.Jack),
                new(suit.GetSameColorSuit(), Rank.Jack),
                new(suit, Rank.Ace),
                new(suit, Rank.King),
                new(suit, Rank.Queen),
            ],
            suit => new Card(suit, Rank.Ten),
            [
                CallTrumpDecision.Pass,
                CallTrumpDecision.OrderItUp,
                CallTrumpDecision.OrderItUpAndGoAlone,
            ]);
    }

    protected override bool IsExpectedChoice(CallTrumpDecision chosenDecision)
    {
        return chosenDecision != CallTrumpDecision.Pass;
    }
}
