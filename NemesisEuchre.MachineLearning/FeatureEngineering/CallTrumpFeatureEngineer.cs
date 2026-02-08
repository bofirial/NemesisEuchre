using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public class CallTrumpFeatureEngineer : IFeatureEngineer<CallTrumpDecisionEntity, CallTrumpTrainingData>
{
    public CallTrumpTrainingData Transform(CallTrumpDecisionEntity entity)
    {
        var context = CallTrumpEntityDeserializer.Deserialize(entity);

        if (!context.ValidDecisions.Contains(context.ChosenDecision))
        {
            throw new InvalidOperationException(
                $"Chosen decision {context.ChosenDecision} is not in the valid decisions array");
        }

        var result = CallTrumpFeatureBuilder.BuildFeatures(
            context.Cards,
            context.UpCard,
            entity.DealerPosition,
            entity.TeamScore,
            entity.OpponentScore,
            entity.DecisionOrder,
            context.ValidDecisions,
            context.ChosenDecision);

        result.ExpectedDealPoints = entity.RelativeDealPoints ?? throw new InvalidOperationException(
            "RelativeDealPoints is required for regression training");

        return result;
    }
}
