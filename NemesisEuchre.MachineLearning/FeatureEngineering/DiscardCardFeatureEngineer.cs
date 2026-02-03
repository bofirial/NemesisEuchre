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

        return new DiscardCardTrainingData
        {
            Card1Rank = (float)cards[0].Rank,
            Card1Suit = (float)cards[0].Suit,
            Card2Rank = (float)cards[1].Rank,
            Card2Suit = (float)cards[1].Suit,
            Card3Rank = (float)cards[2].Rank,
            Card3Suit = (float)cards[2].Suit,
            Card4Rank = (float)cards[3].Rank,
            Card4Suit = (float)cards[3].Suit,
            Card5Rank = (float)cards[4].Rank,
            Card5Suit = (float)cards[4].Suit,
            Card6Rank = (float)cards[5].Rank,
            Card6Suit = (float)cards[5].Suit,
            CallingPlayerPosition = (float)entity.CallingPlayer,
            CallingPlayerGoingAlone = entity.CallingPlayerGoingAlone ? 1.0f : 0.0f,
            TeamScore = entity.TeamScore,
            OpponentScore = entity.OpponentScore,
            Card1Chosen = chosenCardIndex == 0 ? 1.0f : 0.0f,
            Card2Chosen = chosenCardIndex == 1 ? 1.0f : 0.0f,
            Card3Chosen = chosenCardIndex == 2 ? 1.0f : 0.0f,
            Card4Chosen = chosenCardIndex == 3 ? 1.0f : 0.0f,
            Card5Chosen = chosenCardIndex == 4 ? 1.0f : 0.0f,
            Card6Chosen = chosenCardIndex == 5 ? 1.0f : 0.0f,
            ExpectedDealPoints = entity.RelativeDealPoints ?? throw new InvalidOperationException(
                "RelativeDealPoints is required for regression training"),
        };
    }
}
