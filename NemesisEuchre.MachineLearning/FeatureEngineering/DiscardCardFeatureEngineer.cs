using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public class DiscardCardFeatureEngineer : IFeatureEngineer<DiscardCardDecisionEntity, DiscardCardTrainingData>
{
    private const int ExpectedCardsInHand = 6;

    public DiscardCardTrainingData Transform(DiscardCardDecisionEntity entity)
    {
        var cards = JsonDeserializationHelper.DeserializeRelativeCards(entity.CardsInHandJson);

        if (cards.Length != ExpectedCardsInHand)
        {
            throw new InvalidOperationException(
                $"Expected 6 cards in hand but found {cards.Length}");
        }

        var chosenCard = JsonDeserializationHelper.DeserializeRelativeCard(entity.ChosenCardJson);

        var chosenCardIndex = Array.FindIndex(cards, c =>
            c.Rank == chosenCard.Rank && c.Suit == chosenCard.Suit);

        if (chosenCardIndex == -1)
        {
            throw new InvalidOperationException(
                $"Chosen card {chosenCard.Rank} of {chosenCard.Suit} not found in hand");
        }

        var result = DiscardCardFeatureBuilder.BuildFeatures(
            cards,
            entity.CallingPlayer,
            entity.CallingPlayerGoingAlone,
            entity.TeamScore,
            entity.OpponentScore,
            chosenCard);

        result.ExpectedDealPoints = entity.RelativeDealPoints ?? throw new InvalidOperationException(
            "RelativeDealPoints is required for regression training");

        return result;
    }
}
