using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.Discard;

public class FiveTrumpPlusOneNonTrumpShouldDiscardNonTrump(
    IDiscardCardInferenceFeatureBuilder featureBuilder)
    : DiscardCardBehavioralTest(featureBuilder)
{
    public override string Name => "Five trump plus one non-trump";

    public override string Description => "Holding 5 trump and 1 non-trump, must discard the non-trump";

    public override string AssertionDescription => "Must discard non-trump card";

    protected override IReadOnlyList<DiscardCardTestCase> GetTestCases()
    {
        return GenerateAllSuitVariants(
            Name,
            _ =>
            [
                new(Rank.RightBower, RelativeSuit.Trump),
                new(Rank.LeftBower, RelativeSuit.Trump),
                new(Rank.Ace, RelativeSuit.Trump),
                new(Rank.King, RelativeSuit.Trump),
                new(Rank.Queen, RelativeSuit.Trump),
                new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor1),
            ]);
    }

    protected override bool IsExpectedChoice(RelativeCard chosenCard)
    {
        return chosenCard.Suit != RelativeSuit.Trump;
    }
}
