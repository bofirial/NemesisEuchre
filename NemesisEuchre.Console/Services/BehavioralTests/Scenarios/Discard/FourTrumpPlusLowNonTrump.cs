using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.Discard;

public class FourTrumpPlusLowNonTrump(
    IDiscardCardInferenceFeatureBuilder featureBuilder)
    : DiscardCardBehavioralTest(featureBuilder)
{
    public override string Name => "Four trump plus low non-trump";

    public override string Description => "Holding 4 trump and 2 low non-trump, must discard a non-trump";

    public override string AssertionDescription => "Must discard non-trump card";

    protected override RelativeCard[] GetCardsInHand()
    {
        return [
        new(Rank.RightBower, RelativeSuit.Trump),
        new(Rank.LeftBower, RelativeSuit.Trump),
        new(Rank.Ace, RelativeSuit.Trump),
        new(Rank.King, RelativeSuit.Trump),
        new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor1),
        new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor2),
    ];
    }

    protected override bool IsExpectedChoice(RelativeCard chosenCard)
    {
        return chosenCard.Suit != RelativeSuit.Trump;
    }
}
