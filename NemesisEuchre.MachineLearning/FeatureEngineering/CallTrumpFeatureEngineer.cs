using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public class CallTrumpFeatureEngineer : IFeatureEngineer<CallTrumpDecisionEntity, CallTrumpTrainingData>
{
    public CallTrumpTrainingData Transform(CallTrumpDecisionEntity entity)
    {
        var cards = JsonDeserializationHelper.DeserializeCards(entity.CardsInHandJson);
        var upCard = JsonDeserializationHelper.DeserializeCard(entity.UpCardJson);
        var validDecisions = JsonDeserializationHelper.DeserializeCallTrumpDecisions(entity.ValidDecisionsJson);
        var chosenDecision = JsonDeserializationHelper.DeserializeCallTrumpDecision(entity.ChosenDecisionJson);

        if (!validDecisions.Contains(chosenDecision))
        {
            throw new InvalidOperationException(
                $"Chosen decision {chosenDecision} is not in the valid decisions array");
        }

        var result = CallTrumpFeatureBuilder.BuildFeatures(
            cards,
            upCard,
            entity.DealerPosition,
            entity.TeamScore,
            entity.OpponentScore,
            entity.DecisionOrder,
            validDecisions,
            chosenDecision);

        result.ExpectedDealPoints = entity.RelativeDealPoints ?? throw new InvalidOperationException(
            "RelativeDealPoints is required for regression training");

        return result;
    }
}
