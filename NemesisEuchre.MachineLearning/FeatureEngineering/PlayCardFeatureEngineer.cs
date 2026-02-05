using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public class PlayCardFeatureEngineer : IFeatureEngineer<PlayCardDecisionEntity, PlayCardTrainingData>
{
    public PlayCardTrainingData Transform(PlayCardDecisionEntity entity)
    {
        var context = PlayCardEntityDeserializer.Deserialize(entity);

        var chosenCardIndex = Array.FindIndex(context.CardsInHand, c =>
            c.Rank == context.ChosenCard.Rank && c.Suit == context.ChosenCard.Suit);

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
            entity.LeadPlayer,
            entity.LeadSuit,
            entity.CallingPlayer,
            entity.CallingPlayerGoingAlone,
            entity.DealerPosition,
            context.DealerPickedUpCard,
            context.KnownPlayerSuitVoids,
            context.CardsAccountedFor,
            entity.WinningTrickPlayer,
            entity.TrickNumber,
            context.ChosenCard);

        trainingData.ExpectedDealPoints = entity.RelativeDealPoints ?? throw new InvalidOperationException(
            "RelativeDealPoints is required for regression training");

        return trainingData;
    }
}
