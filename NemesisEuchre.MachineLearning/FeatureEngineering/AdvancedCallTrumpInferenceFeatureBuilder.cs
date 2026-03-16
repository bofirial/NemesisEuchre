using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public interface IAdvancedCallTrumpInferenceFeatureBuilder
{
    AdvancedCallTrumpTrainingData BuildFeatures(
        Card[] cardsInHand,
        Card upCard,
        RelativePlayerPosition dealerPosition,
        short teamScore,
        short opponentScore,
        CallTrumpDecision chosenDecision,
        byte decisionNumber);
}

public class AdvancedCallTrumpInferenceFeatureBuilder : IAdvancedCallTrumpInferenceFeatureBuilder
{
    public AdvancedCallTrumpTrainingData BuildFeatures(
        Card[] cardsInHand,
        Card upCard,
        RelativePlayerPosition dealerPosition,
        short teamScore,
        short opponentScore,
        CallTrumpDecision chosenDecision,
        byte decisionNumber)
    {
        var full = CallTrumpFeatureBuilder.BuildFeatures(
            cardsInHand,
            upCard,
            dealerPosition,
            teamScore,
            opponentScore,
            decisionNumber,
            chosenDecision);

        return CallTrumpTrainingDataMapper.MapFrom<AdvancedCallTrumpTrainingData>(full);
    }
}
