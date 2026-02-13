using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.CallTrump;

public class TopThreeTrumpCardsInHandShouldNotPass(
    ICallTrumpInferenceFeatureBuilder featureBuilder)
    : CallTrumpBehavioralTest(featureBuilder)
{
    public override string Name => "Top three trump cards in hand";

    public override string Description => "Holding right bower, left bower, and ace of trump, should not pass";

    public override string AssertionDescription => "Should not pass";

    protected override IReadOnlyList<CallTrumpTestCase> GetTestCases()
    {
        return GenerateAllSuitVariants(
            Name,
            suit =>
            {
                var opposites = GetOppositeColorSuits(suit);
                return
                [
                    new(suit, Rank.Jack),
                    new(suit.GetSameColorSuit(), Rank.Jack),
                    new(suit, Rank.Ace),
                    new(opposites[0], Rank.Nine),
                    new(opposites[1], Rank.Ten),
                ];
            },
            suit => new Card(suit, Rank.Nine),
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

    private static Suit[] GetOppositeColorSuits(Suit trump)
    {
        var sameColor = trump.GetSameColorSuit();
        return [.. Enum.GetValues<Suit>().Where(s => s != trump && s != sameColor)];
    }
}
