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
        return [
            new DiscardCardTestCase($"{Name} (Right Bower)", [
                new(Rank.RightBower, RelativeSuit.Trump),
                new(Rank.Ace, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Jack, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor1),
            ]),
            new DiscardCardTestCase($"{Name} (Left Bower)", [
                new(Rank.LeftBower, RelativeSuit.Trump),
                new(Rank.Ace, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Jack, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor1),
            ]),
            new DiscardCardTestCase($"{Name} (Ace)", [
                new(Rank.Ace, RelativeSuit.Trump),
                new(Rank.Ace, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Jack, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor1),
            ]),
            new DiscardCardTestCase($"{Name} (King)", [
                new(Rank.King, RelativeSuit.Trump),
                new(Rank.Ace, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Jack, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor1),
            ]),
            new DiscardCardTestCase($"{Name} (Queen)", [
                new(Rank.Queen, RelativeSuit.Trump),
                new(Rank.Ace, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Jack, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor1),
            ]),
            new DiscardCardTestCase($"{Name} (Ten)", [
                new(Rank.Ten, RelativeSuit.Trump),
                new(Rank.Ace, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Jack, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor1),
            ]),
            new DiscardCardTestCase($"{Name} (Nine)", [
                new(Rank.Nine, RelativeSuit.Trump),
                new(Rank.Ace, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Jack, RelativeSuit.NonTrumpOppositeColor1),
                new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor1),
            ]),
        ];
    }

    protected override bool IsExpectedChoice(RelativeCard chosenCard)
    {
        return chosenCard.Suit != RelativeSuit.Trump;
    }
}
