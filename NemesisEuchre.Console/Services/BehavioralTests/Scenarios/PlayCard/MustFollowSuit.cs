using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.PlayCard;

public class MustFollowSuit(
    IPlayCardInferenceFeatureBuilder featureBuilder)
    : PlayCardBehavioralTest(featureBuilder)
{
    public override string Name => "Must follow suit";

    public override string Description => "When holding a card of the lead suit, must follow suit";

    public override string AssertionDescription => "Must follow suit";

    protected override RelativePlayerPosition LeadPlayer => RelativePlayerPosition.LeftHandOpponent;

    protected override RelativeSuit? LeadSuit => RelativeSuit.NonTrumpOppositeColor1;

    protected override Dictionary<RelativePlayerPosition, RelativeCard> PlayedCardsInTrick =>
        new()
        {
            [RelativePlayerPosition.LeftHandOpponent] = new(Rank.Queen, RelativeSuit.NonTrumpOppositeColor1),
        };

    protected override RelativePlayerPosition? WinningTrickPlayer => RelativePlayerPosition.LeftHandOpponent;

    protected override RelativeCard[] GetCardsInHand()
    {
        return [
        new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
        new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor1),
        new(Rank.Ace, RelativeSuit.Trump),
        new(Rank.Queen, RelativeSuit.NonTrumpSameColor),
    ];
    }

    protected override RelativeCard[] GetValidCardsToPlay()
    {
        return [
        new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
        new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor1),
    ];
    }

    protected override bool IsExpectedChoice(RelativeCard chosenCard)
    {
        return chosenCard.Suit == RelativeSuit.NonTrumpOppositeColor1;
    }
}
