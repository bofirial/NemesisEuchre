using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.Discard;

public class LoneSuitShouldBeDiscardedToReduceToTwoSuits(
    IDiscardCardInferenceFeatureBuilder featureBuilder)
    : DiscardCardBehavioralTest(featureBuilder)
{
    public override string Name => "Discard lone suit to reduce to two suits";

    public override string Description => "Holding 3 Suited hand must discard the lone non-trump suit";

    public override string AssertionDescription => "Must discard the lone non-trump card";

    protected override IReadOnlyList<DiscardCardTestCase> GetTestCases()
    {
        return [
            new DiscardCardTestCase(
                $"{Name} (NonTrumpSameColor)",
                [
                    new(Rank.Queen, RelativeSuit.Trump),
                    new(Rank.Ten, RelativeSuit.Trump),
                    new(Rank.Queen, RelativeSuit.NonTrumpSameColor),
                    new(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1),
                    new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor1),
                    new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor1),
                ],
                decision => decision.Suit == RelativeSuit.NonTrumpSameColor),
            new DiscardCardTestCase(
                $"{Name} (NonTrumpOppositeColor1)",
                [
                    new(Rank.Queen, RelativeSuit.Trump),
                    new(Rank.Ten, RelativeSuit.Trump),
                    new(Rank.Queen, RelativeSuit.NonTrumpSameColor),
                    new(Rank.Ten, RelativeSuit.NonTrumpSameColor),
                    new(Rank.Nine, RelativeSuit.NonTrumpSameColor),
                    new(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1),
                ],
                decision => decision.Suit == RelativeSuit.NonTrumpOppositeColor1),
            new DiscardCardTestCase(
                $"{Name} (NonTrumpOppositeColor2)",
                [
                    new(Rank.Queen, RelativeSuit.Trump),
                    new(Rank.Ten, RelativeSuit.Trump),
                    new(Rank.Queen, RelativeSuit.NonTrumpSameColor),
                    new(Rank.Ten, RelativeSuit.NonTrumpSameColor),
                    new(Rank.Nine, RelativeSuit.NonTrumpSameColor),
                    new(Rank.Queen, RelativeSuit.NonTrumpOppositeColor2),
                ],
                decision => decision.Suit == RelativeSuit.NonTrumpOppositeColor2),
        ];
    }

    protected override bool IsExpectedChoice(RelativeCard chosenCard)
    {
        return chosenCard.Suit == RelativeSuit.NonTrumpOppositeColor2;
    }
}
