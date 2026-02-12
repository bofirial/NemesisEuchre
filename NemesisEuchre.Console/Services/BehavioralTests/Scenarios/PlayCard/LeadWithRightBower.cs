using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.PlayCard;

public class LeadWithRightBower(
    IPlayCardInferenceFeatureBuilder featureBuilder)
    : PlayCardBehavioralTest(featureBuilder)
{
    public override string Name => "Lead with right bower";

    public override string Description => "When leading with right bower in hand, should lead with it";

    public override string AssertionDescription => "Should lead right bower";

    protected override RelativeCard[] GetCardsInHand()
    {
        return [
        new(Rank.RightBower, RelativeSuit.Trump),
        new(Rank.Nine, RelativeSuit.NonTrumpSameColor),
        new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor1),
    ];
    }

    protected override RelativeCard[] GetValidCardsToPlay()
    {
        return [
        new(Rank.RightBower, RelativeSuit.Trump),
        new(Rank.Nine, RelativeSuit.NonTrumpSameColor),
        new(Rank.Ten, RelativeSuit.NonTrumpOppositeColor1),
    ];
    }

    protected override bool IsExpectedChoice(RelativeCard chosenCard)
    {
        return chosenCard.Rank == Rank.RightBower;
    }
}
