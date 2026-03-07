using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.Console.Services.BehavioralTests;

public interface IPlayCardBehavioralTestRunner
{
    DecisionType DecisionType { get; }

    float? TryScore(IPredictionEngineProvider engineProvider, string modelName, PlayCardFeatureBuilderContext context);
}

public class PlayCardBehavioralTestRunner : IPlayCardBehavioralTestRunner
{
    public DecisionType DecisionType => DecisionType.Play;

    public float? TryScore(IPredictionEngineProvider engineProvider, string modelName, PlayCardFeatureBuilderContext context)
    {
        var engine = engineProvider.TryGetEngine<PlayCardTrainingData, PlayCardRegressionPrediction>("PlayCard", modelName);
        if (engine == null)
        {
            return null;
        }

        var features = PlayCardFeatureBuilder.BuildFeatures(context);
        return engine.Predict(features).PredictedPoints;
    }
}
