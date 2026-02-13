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
        return [
            new DiscardCardTestCase($"{Name} (NonTrumpSameColor)", [
                new(Rank.RightBower, RelativeSuit.Trump),
                new(Rank.LeftBower, RelativeSuit.Trump),
                new(Rank.Ace, RelativeSuit.Trump),
                new(Rank.King, RelativeSuit.Trump),
                new(Rank.Queen, RelativeSuit.Trump),
                new(Rank.Nine, RelativeSuit.NonTrumpSameColor),
            ]),
            new DiscardCardTestCase($"{Name} (NonTrumpOppositeColor1)", [
                new(Rank.RightBower, RelativeSuit.Trump),
                new(Rank.LeftBower, RelativeSuit.Trump),
                new(Rank.Ace, RelativeSuit.Trump),
                new(Rank.King, RelativeSuit.Trump),
                new(Rank.Queen, RelativeSuit.Trump),
                new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor1),
            ]),
            new DiscardCardTestCase($"{Name} (NonTrumpOppositeColor2)", [
                new(Rank.RightBower, RelativeSuit.Trump),
                new(Rank.LeftBower, RelativeSuit.Trump),
                new(Rank.Ace, RelativeSuit.Trump),
                new(Rank.King, RelativeSuit.Trump),
                new(Rank.Queen, RelativeSuit.Trump),
                new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor2),
            ]),
        ];
    }

    protected override bool IsExpectedChoice(RelativeCard chosenCard)
    {
        return chosenCard.Suit != RelativeSuit.Trump;
    }
}
