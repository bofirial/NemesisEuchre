using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.PlayCard;

public class DontTrumpOverPartner(
    IPlayCardInferenceFeatureBuilder featureBuilder)
    : PlayCardBehavioralTest(featureBuilder)
{
    public override string Name => "Don't trump over partner";

    public override string Description => "Partner winning with ace, should not play trump";

    public override string AssertionDescription => "Should not play trump";

    protected override RelativePlayerPosition LeadPlayer => RelativePlayerPosition.LeftHandOpponent;

    protected override RelativeSuit? LeadSuit => RelativeSuit.NonTrumpSameColor;

    protected override Dictionary<RelativePlayerPosition, RelativeCard> PlayedCardsInTrick =>
        new()
        {
            [RelativePlayerPosition.LeftHandOpponent] = new(Rank.Ten, RelativeSuit.NonTrumpSameColor),
            [RelativePlayerPosition.Partner] = new(Rank.Ace, RelativeSuit.NonTrumpSameColor),
        };

    protected override RelativePlayerPosition? WinningTrickPlayer => RelativePlayerPosition.Partner;

    protected override RelativeCard[] GetCardsInHand()
    {
        return [
        new(Rank.Queen, RelativeSuit.Trump),
        new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
        new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor2),
    ];
    }

    protected override RelativeCard[] GetValidCardsToPlay()
    {
        return [
        new(Rank.Queen, RelativeSuit.Trump),
        new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
        new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor2),
    ];
    }

    protected override bool IsExpectedChoice(RelativeCard chosenCard)
    {
        return chosenCard.Suit != RelativeSuit.Trump;
    }
}
