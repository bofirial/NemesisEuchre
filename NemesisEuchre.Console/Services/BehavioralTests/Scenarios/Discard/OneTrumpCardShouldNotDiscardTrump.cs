using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.Discard;

public class OneTrumpCardShouldNotDiscardTrump(
    IDiscardCardInferenceFeatureBuilder featureBuilder)
    : DiscardCardBehavioralTest(featureBuilder)
{
    public override string Name => "Keep lone trump card";

    public override string Description => "Holding 1 trump and 5 off-suit, must not discard the trump";

    public override string AssertionDescription => "Must not discard trump card";

    protected override IReadOnlyList<DiscardCardTestCase> GetTestCases()
    {
        return GenerateAllSuitVariants(
            Name,
            _ =>
            [
                new(Rank.Queen, RelativeSuit.Trump),
                new(Rank.Ace, RelativeSuit.NonTrumpSameColor),
                new(Rank.King, RelativeSuit.NonTrumpSameColor),
                new(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Jack, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor2),
            ]);
    }

    protected override bool IsExpectedChoice(RelativeCard chosenCard)
    {
        return chosenCard.Suit != RelativeSuit.Trump;
    }
}
