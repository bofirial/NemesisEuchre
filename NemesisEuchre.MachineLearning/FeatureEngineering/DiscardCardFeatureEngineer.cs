using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public class DiscardCardFeatureEngineer : IFeatureEngineer<DiscardCardDecisionEntity, DiscardCardTrainingData>
{
    private const int ExpectedCardsInHand = 6;

    public DiscardCardTrainingData Transform(DiscardCardDecisionEntity entity)
    {
        var context = DiscardCardFeatureContextBuilder.Build(entity);

        if (context.CardsInHand.Length != ExpectedCardsInHand)
        {
            throw new InvalidOperationException(
                $"Expected 6 cards in hand but found {context.CardsInHand.Length}");
        }

        var chosenCardIndex = Array.FindIndex(context.CardsInHand, c => c == context.ChosenCard);

        if (chosenCardIndex == -1)
        {
            throw new InvalidOperationException(
                $"Chosen card {context.ChosenCard.Rank} of {context.ChosenCard.Suit} not found in hand");
        }

        var result = DiscardCardFeatureBuilder.BuildFeatures(
            context.CardsInHand,
            (RelativePlayerPosition)entity.CallingRelativePlayerPositionId,
            entity.CallingPlayerGoingAlone,
            entity.TeamScore,
            entity.OpponentScore,
            context.ChosenCard);

        result.ExpectedDealPoints = entity.RelativeDealPoints ?? throw new InvalidOperationException(
            "RelativeDealPoints is required for regression training");

        return result;
    }
}
