using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.CallTrump;

public class ForcedCallShouldChooseBestTrump(
    ICallTrumpInferenceFeatureBuilder featureBuilder)
    : CallTrumpBehavioralTest(featureBuilder)
{
    public override string Name => "Forced call should choose best trump";

    public override string Description => "Stuck dealer with worst hand (4 nines + 1 ten) should call the suit of the ten";

    public override string AssertionDescription => "Should call suit of the ten";

    protected override RelativePlayerPosition DealerPosition => RelativePlayerPosition.Self;

    protected override IReadOnlyList<CallTrumpTestCase> GetTestCases()
    {
        return [.. Enum.GetValues<Suit>()
            .Select(bestSuit =>
            {
                var turnedDownSuit = bestSuit.GetSameColorSuit();
                var otherSuits = Enum.GetValues<Suit>()
                    .Where(s => s != turnedDownSuit)
                    .ToArray();

                var validDecisions = otherSuits
                    .SelectMany(s => new[]
                    {
                        (CallTrumpDecision)(int)s,
                        (CallTrumpDecision)((int)s + 4),
                    })
                    .ToArray();

                var callBestSuit = (CallTrumpDecision)(int)bestSuit;
                var callBestSuitAlone = (CallTrumpDecision)((int)bestSuit + 4);

                return new CallTrumpTestCase(
                    $"{Name} ({bestSuit})",
                    [
                        new(Suit.Spades, Rank.Nine),
                        new(Suit.Hearts, Rank.Nine),
                        new(Suit.Clubs, Rank.Nine),
                        new(Suit.Diamonds, Rank.Nine),
                        new(bestSuit, Rank.Ten),
                    ],
                    new Card(turnedDownSuit, Rank.Ace),
                    validDecisions,
                    decision => decision == callBestSuit || decision == callBestSuitAlone);
            })];
    }

    protected override bool IsExpectedChoice(CallTrumpDecision chosenDecision)
    {
        return chosenDecision != CallTrumpDecision.Pass;
    }
}
