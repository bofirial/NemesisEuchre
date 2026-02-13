using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.PlayCard;

public class OpponentWinningTrickShouldPlayTrump(
    IPlayCardInferenceFeatureBuilder featureBuilder)
    : PlayCardBehavioralTest(featureBuilder)
{
    public override string Name => "Opponent winning trick, should play trump";

    public override string Description => "Opponent winning with ace, should play trump";

    public override string AssertionDescription => "Should play trump";

    protected override RelativePlayerPosition LeadPlayer => RelativePlayerPosition.LeftHandOpponent;

    protected override RelativeSuit? LeadSuit => RelativeSuit.NonTrumpSameColor;

    protected override Dictionary<RelativePlayerPosition, RelativeCard> PlayedCardsInTrick =>
        new()
        {
            [RelativePlayerPosition.LeftHandOpponent] = new(Rank.Ace, RelativeSuit.NonTrumpSameColor),
            [RelativePlayerPosition.Partner] = new(Rank.Ten, RelativeSuit.NonTrumpSameColor),
            [RelativePlayerPosition.RightHandOpponent] = new(Rank.Queen, RelativeSuit.NonTrumpSameColor),
        };

    protected override RelativePlayerPosition? WinningTrickPlayer => RelativePlayerPosition.LeftHandOpponent;

    protected override RelativeCard[] GetCardsInHand()
    {
        return [
            new(Rank.RightBower, RelativeSuit.Trump),
            new(Rank.Queen, RelativeSuit.Trump),
            new(Rank.Nine, RelativeSuit.Trump),
            new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
            new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor2),
        ];
    }

    protected override RelativeCard[] GetValidCardsToPlay()
    {
        return [
            new(Rank.RightBower, RelativeSuit.Trump),
            new(Rank.Queen, RelativeSuit.Trump),
            new(Rank.Nine, RelativeSuit.Trump),
            new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
            new(Rank.Nine, RelativeSuit.NonTrumpOppositeColor2),
        ];
    }

    protected override bool IsExpectedChoice(RelativeCard chosenCard)
    {
        return chosenCard.Suit == RelativeSuit.Trump;
    }
}
