using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.CallTrump;

public class NoTrumpInHandShouldPass(
    ICallTrumpInferenceFeatureBuilder featureBuilder)
    : CallTrumpBehavioralTest(featureBuilder)
{
    public override string Name => "No trump in hand";

    public override string Description => "Holding no trump, should pass";

    public override string AssertionDescription => "Should pass";

    protected override IReadOnlyList<CallTrumpTestCase> GetTestCases()
    {
        return GenerateAllSuitVariants(
            Name,
            suit =>
            {
                var others = Enum.GetValues<Suit>().Where(s => s != suit).ToArray();
                return
                [
                    new(others[0], Rank.Nine),
                    new(others[0], Rank.Ten),
                    new(others[1], Rank.Nine),
                    new(others[1], Rank.Ten),
                    new(others[2], Rank.Nine),
                ];
            },
            suit => new Card(suit, Rank.Ace),
            [
                CallTrumpDecision.Pass,
                CallTrumpDecision.OrderItUp,
                CallTrumpDecision.OrderItUpAndGoAlone,
            ]);
    }

    protected override bool IsExpectedChoice(CallTrumpDecision chosenDecision)
    {
        return chosenDecision == CallTrumpDecision.Pass;
    }
}
