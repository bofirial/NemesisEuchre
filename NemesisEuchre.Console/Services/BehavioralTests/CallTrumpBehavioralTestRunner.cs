using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.Console.Services.BehavioralTests;

public interface ICallTrumpBehavioralTestRunner
{
    DecisionType ReportingDecisionType { get; }

    float? TryScore(
        IPredictionEngineProvider engineProvider,
        string modelName,
        Card[] cardsInHand,
        Card upCard,
        RelativePlayerPosition dealerPosition,
        short teamScore,
        short opponentScore,
        CallTrumpDecision decision,
        byte decisionNumber);
}

public class CallTrumpBehavioralTestRunner(
    ICallTrumpInferenceFeatureBuilder featureBuilder) : ICallTrumpBehavioralTestRunner
{
    public DecisionType ReportingDecisionType => DecisionType.CallTrump;

    public float? TryScore(
        IPredictionEngineProvider engineProvider,
        string modelName,
        Card[] cardsInHand,
        Card upCard,
        RelativePlayerPosition dealerPosition,
        short teamScore,
        short opponentScore,
        CallTrumpDecision decision,
        byte decisionNumber)
    {
        var engine = engineProvider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
            "CallTrump", modelName);
        if (engine == null)
        {
            return null;
        }

        var features = featureBuilder.BuildFeatures(
            cardsInHand,
            upCard,
            dealerPosition,
            teamScore,
            opponentScore,
            decision,
            decisionNumber);
        return engine.Predict(features).PredictedPoints;
    }
}

public class AdvancedCallTrumpBehavioralTestRunner(
    IAdvancedCallTrumpInferenceFeatureBuilder featureBuilder) : ICallTrumpBehavioralTestRunner
{
    public DecisionType ReportingDecisionType => DecisionType.CallTrump;

    public float? TryScore(
        IPredictionEngineProvider engineProvider,
        string modelName,
        Card[] cardsInHand,
        Card upCard,
        RelativePlayerPosition dealerPosition,
        short teamScore,
        short opponentScore,
        CallTrumpDecision decision,
        byte decisionNumber)
    {
        var engine = engineProvider.TryGetEngine<AdvancedCallTrumpTrainingData, CallTrumpRegressionPrediction>(
            "AdvancedCallTrump", modelName);
        if (engine == null)
        {
            return null;
        }

        var features = featureBuilder.BuildFeatures(
            cardsInHand,
            upCard,
            dealerPosition,
            teamScore,
            opponentScore,
            decision,
            decisionNumber);
        return engine.Predict(features).PredictedPoints;
    }
}
