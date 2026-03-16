using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.Console.Services.BehavioralTests;

public interface IDiscardCardBehavioralTestRunner
{
    DecisionType DecisionType { get; }

    float? TryScore(
        IPredictionEngineProvider engineProvider,
        string modelName,
        RelativeCard[] cardsInHand,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        short teamScore,
        short opponentScore,
        RelativeCard chosenCard);
}

public class DiscardCardBehavioralTestRunner(
    IDiscardCardInferenceFeatureBuilder featureBuilder) : IDiscardCardBehavioralTestRunner
{
    public DecisionType DecisionType => DecisionType.Discard;

    public float? TryScore(
        IPredictionEngineProvider engineProvider,
        string modelName,
        RelativeCard[] cardsInHand,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        short teamScore,
        short opponentScore,
        RelativeCard chosenCard)
    {
        var engine = engineProvider.TryGetEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>(
            "DiscardCard", modelName);
        if (engine == null)
        {
            return null;
        }

        var features = featureBuilder.BuildFeatures(
            cardsInHand,
            callingPlayer,
            callingPlayerGoingAlone,
            teamScore,
            opponentScore,
            chosenCard);
        return engine.Predict(features).PredictedPoints;
    }
}

public class AdvancedDiscardCardBehavioralTestRunner(
    IAdvancedDiscardCardInferenceFeatureBuilder featureBuilder) : IDiscardCardBehavioralTestRunner
{
    public DecisionType DecisionType => DecisionType.AdvancedDiscard;

    public float? TryScore(
        IPredictionEngineProvider engineProvider,
        string modelName,
        RelativeCard[] cardsInHand,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        short teamScore,
        short opponentScore,
        RelativeCard chosenCard)
    {
        var engine = engineProvider.TryGetEngine<AdvancedDiscardCardTrainingData, DiscardCardRegressionPrediction>(
            "AdvancedDiscardCard", modelName);
        if (engine == null)
        {
            return null;
        }

        var features = featureBuilder.BuildFeatures(
            cardsInHand,
            callingPlayer,
            callingPlayerGoingAlone,
            teamScore,
            opponentScore,
            chosenCard);
        return engine.Predict(features).PredictedPoints;
    }
}
