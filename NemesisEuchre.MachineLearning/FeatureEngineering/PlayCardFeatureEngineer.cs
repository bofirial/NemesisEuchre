using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public class PlayCardFeatureEngineer : IFeatureEngineer<PlayCardDecisionEntity, PlayCardTrainingData>
{
    public PlayCardTrainingData Transform(PlayCardDecisionEntity entity)
    {
        var context = PlayCardFeatureContextBuilder.Build(entity);

        var chosenCardIndex = Array.FindIndex(context.CardsInHand, c => c == context.ChosenCard);

        if (chosenCardIndex == -1)
        {
            throw new InvalidOperationException(
                $"Chosen card {context.ChosenCard.Rank} of {context.ChosenCard.Suit} not found in hand");
        }

        var trainingData = PlayCardFeatureBuilder.BuildFeatures(
            context.CardsInHand,
            context.ValidCards,
            context.PlayedCards,
            entity.TeamScore,
            entity.OpponentScore,
            (RelativePlayerPosition)entity.LeadRelativePlayerPositionId,
            entity.LeadRelativeSuitId.HasValue ? (RelativeSuit)entity.LeadRelativeSuitId.Value : null,
            (RelativePlayerPosition)entity.CallingRelativePlayerPositionId,
            entity.CallingPlayerGoingAlone,
            (RelativePlayerPosition)entity.DealerRelativePlayerPositionId,
            context.DealerPickedUpCard,
            context.KnownPlayerSuitVoids,
            context.CardsAccountedFor,
            entity.WinningTrickRelativePlayerPositionId.HasValue ? (RelativePlayerPosition)entity.WinningTrickRelativePlayerPositionId.Value : null,
            entity.TrickNumber,
            context.ChosenCard);

        trainingData.ExpectedDealPoints = entity.RelativeDealPoints ?? throw new InvalidOperationException(
            "RelativeDealPoints is required for regression training");

        return trainingData;
    }
}
