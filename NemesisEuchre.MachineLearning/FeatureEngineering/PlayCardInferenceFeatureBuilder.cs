using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public interface IPlayCardInferenceFeatureBuilder
{
    PlayCardTrainingData BuildFeatures(
        RelativeCard[] cardsInHand,
        RelativePlayerPosition leadPlayer,
        RelativeSuit? leadSuit,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCards,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativePlayerPosition dealer,
        RelativeCard? dealerPickedUpCard,
        RelativePlayerSuitVoid[] knownPlayerSuitVoids,
        RelativeCard[] cardsAccountedFor,
        RelativePlayerPosition? winningTrickPlayer,
        short trickNumber,
        short wonTricks,
        short opponentsWonTricks,
        RelativeCard chosenCard);
}

public class PlayCardInferenceFeatureBuilder : IPlayCardInferenceFeatureBuilder
{
    public PlayCardTrainingData BuildFeatures(
        RelativeCard[] cardsInHand,
        RelativePlayerPosition leadPlayer,
        RelativeSuit? leadSuit,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCards,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativePlayerPosition dealer,
        RelativeCard? dealerPickedUpCard,
        RelativePlayerSuitVoid[] knownPlayerSuitVoids,
        RelativeCard[] cardsAccountedFor,
        RelativePlayerPosition? winningTrickPlayer,
        short trickNumber,
        short wonTricks,
        short opponentsWonTricks,
        RelativeCard chosenCard)
    {
        var context = new PlayCardFeatureBuilderContext(
            cardsInHand,
            playedCards,
            teamScore,
            opponentScore,
            leadPlayer,
            leadSuit,
            callingPlayer,
            callingPlayerGoingAlone,
            dealer,
            dealerPickedUpCard,
            knownPlayerSuitVoids,
            cardsAccountedFor,
            winningTrickPlayer,
            trickNumber,
            wonTricks,
            opponentsWonTricks,
            chosenCard);

        return PlayCardFeatureBuilder.BuildFeatures(context);
    }
}
