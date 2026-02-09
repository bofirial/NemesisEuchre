using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public interface ICallTrumpInferenceFeatureBuilder
{
    CallTrumpTrainingData BuildFeatures(
        Card[] cardsInHand,
        Card upCard,
        RelativePlayerPosition dealerPosition,
        short teamScore,
        short opponentScore,
        CallTrumpDecision[] validDecisions,
        CallTrumpDecision chosenDecision);
}

public class CallTrumpInferenceFeatureBuilder : ICallTrumpInferenceFeatureBuilder
{
    public CallTrumpTrainingData BuildFeatures(
        Card[] cardsInHand,
        Card upCard,
        RelativePlayerPosition dealerPosition,
        short teamScore,
        short opponentScore,
        CallTrumpDecision[] validDecisions,
        CallTrumpDecision chosenDecision)
    {
        return CallTrumpFeatureBuilder.BuildFeatures(
            cardsInHand,
            upCard,
            dealerPosition,
            teamScore,
            opponentScore,
            0f,
            validDecisions,
            chosenDecision);
    }
}
