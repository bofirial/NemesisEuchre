using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public interface IAdvancedDiscardCardInferenceFeatureBuilder
{
    AdvancedDiscardCardTrainingData BuildFeatures(
        RelativeCard[] cardsInHand,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        short teamScore,
        short opponentScore,
        RelativeCard chosenCard);
}

public class AdvancedDiscardCardInferenceFeatureBuilder : IAdvancedDiscardCardInferenceFeatureBuilder
{
    public AdvancedDiscardCardTrainingData BuildFeatures(
        RelativeCard[] cardsInHand,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        short teamScore,
        short opponentScore,
        RelativeCard chosenCard)
    {
        var full = DiscardCardFeatureBuilder.BuildFeatures(
            cardsInHand,
            callingPlayer,
            callingPlayerGoingAlone,
            teamScore,
            opponentScore,
            chosenCard);

        return DiscardCardTrainingDataMapper.MapFrom<AdvancedDiscardCardTrainingData>(full);
    }
}
