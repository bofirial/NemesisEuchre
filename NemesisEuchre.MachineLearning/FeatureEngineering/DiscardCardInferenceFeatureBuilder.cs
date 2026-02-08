using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public interface IDiscardCardInferenceFeatureBuilder
{
    DiscardCardTrainingData BuildFeatures(
        RelativeCard[] cardsInHand,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        short teamScore,
        short opponentScore,
        RelativeCard chosenCard);
}

public class DiscardCardInferenceFeatureBuilder : IDiscardCardInferenceFeatureBuilder
{
    public DiscardCardTrainingData BuildFeatures(
        RelativeCard[] cardsInHand,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        short teamScore,
        short opponentScore,
        RelativeCard chosenCard)
    {
        return DiscardCardFeatureBuilder.BuildFeatures(
            cardsInHand,
            callingPlayer,
            callingPlayerGoingAlone,
            teamScore,
            opponentScore,
            chosenCard);
    }
}
