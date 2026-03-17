using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.Console.Services.BehavioralTests;

public class SimplePlayCardBehavioralTestRunner(ISimplePlayCardInferenceFeatureBuilder featureBuilder) : IPlayCardBehavioralTestRunner
{
    public DecisionType ReportingDecisionType => DecisionType.Play;

    public float? TryScore(IPredictionEngineProvider engineProvider, string modelName, PlayCardFeatureBuilderContext context)
    {
        var engine = engineProvider.TryGetEngine<SimplePlayCardTrainingData, PlayCardRegressionPrediction>("SimplePlayCard", modelName);
        if (engine == null)
        {
            return null;
        }

        var features = featureBuilder.BuildFeatures(
            context.CardsInHand,
            context.LeadPlayer,
            context.LeadSuit,
            context.PlayedCards,
            context.TeamScore,
            context.OpponentScore,
            context.CallingPlayer,
            context.CallingPlayerGoingAlone,
            context.Dealer,
            context.DealerPickedUpCard,
            context.KnownPlayerSuitVoids,
            context.CardsAccountedFor,
            context.WinningTrickPlayer,
            context.TrickNumber,
            context.WonTricks,
            context.OpponentsWonTricks,
            context.ChosenCard);
        return engine.Predict(features).PredictedPoints;
    }
}
