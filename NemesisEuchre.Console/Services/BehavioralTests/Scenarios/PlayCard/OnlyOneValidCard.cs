using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.Console.Services.BehavioralTests.Scenarios.PlayCard;

public class OnlyOneValidCard(
    IPlayCardInferenceFeatureBuilder featureBuilder)
    : PlayCardBehavioralTest(featureBuilder)
{
    public override string Name => "Only one valid card";

    public override string Description => "With only one valid card to play, must play it";

    public override string AssertionDescription => "Must play only valid card";

    protected override RelativePlayerPosition LeadPlayer => RelativePlayerPosition.LeftHandOpponent;

    protected override RelativeSuit? LeadSuit => RelativeSuit.Trump;

    protected override Dictionary<RelativePlayerPosition, RelativeCard> PlayedCardsInTrick =>
        new()
        {
            [RelativePlayerPosition.LeftHandOpponent] = new(Rank.King, RelativeSuit.Trump),
        };

    protected override RelativePlayerPosition? WinningTrickPlayer => RelativePlayerPosition.LeftHandOpponent;

    protected override short TrickNumber => 3;

    protected override RelativeCard[] GetCardsInHand()
    {
        return [
        new(Rank.Nine, RelativeSuit.Trump),
        new(Rank.Ace, RelativeSuit.NonTrumpSameColor),
        new(Rank.King, RelativeSuit.NonTrumpOppositeColor1),
    ];
    }

    protected override RelativeCard[] GetValidCardsToPlay()
    {
        return [
        new(Rank.Nine, RelativeSuit.Trump),
    ];
    }

    protected override bool IsExpectedChoice(RelativeCard chosenCard)
    {
        return chosenCard.Rank == Rank.Nine && chosenCard.Suit == RelativeSuit.Trump;
    }
}
