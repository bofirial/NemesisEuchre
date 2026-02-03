using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public class PlayCardFeatureEngineer : IFeatureEngineer<PlayCardDecisionEntity, PlayCardTrainingData>
{
    private const int MaxCardsInHand = 5;

    public PlayCardTrainingData Transform(PlayCardDecisionEntity entity)
    {
        var cards = JsonDeserializationHelper.DeserializeRelativeCards(entity.CardsInHandJson);
        var playedCardsDict = JsonDeserializationHelper.DeserializePlayedCards(entity.PlayedCardsJson);

        var playedCards = playedCardsDict
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => kvp.Value)
            .ToArray();

        var validCards = JsonDeserializationHelper.DeserializeRelativeCards(entity.ValidCardsToPlayJson);

        var validityArray = new float[MaxCardsInHand];
        foreach (var validCard in validCards)
        {
            var index = Array.FindIndex(cards, c =>
                c.Rank == validCard.Rank && c.Suit == validCard.Suit);
            if (index != -1)
            {
                validityArray[index] = 1.0f;
            }
        }

        var chosenCard = JsonDeserializationHelper.DeserializeRelativeCard(entity.ChosenCardJson);

        var chosenCardIndex = Array.FindIndex(cards, c =>
            c.Rank == chosenCard.Rank && c.Suit == chosenCard.Suit);

        if (chosenCardIndex == -1)
        {
            throw new InvalidOperationException(
                $"Chosen card {chosenCard.Rank} of {chosenCard.Suit} not found in hand");
        }

        return new PlayCardTrainingData
        {
            Card1Rank = cards.Length > 0 ? (float)cards[0].Rank : -1.0f,
            Card1Suit = cards.Length > 0 ? (float)cards[0].Suit : -1.0f,
            Card2Rank = cards.Length > 1 ? (float)cards[1].Rank : -1.0f,
            Card2Suit = cards.Length > 1 ? (float)cards[1].Suit : -1.0f,
            Card3Rank = cards.Length > 2 ? (float)cards[2].Rank : -1.0f,
            Card3Suit = cards.Length > 2 ? (float)cards[2].Suit : -1.0f,
            Card4Rank = cards.Length > 3 ? (float)cards[3].Rank : -1.0f,
            Card4Suit = cards.Length > 3 ? (float)cards[3].Suit : -1.0f,
            Card5Rank = cards.Length > 4 ? (float)cards[4].Rank : -1.0f,
            Card5Suit = cards.Length > 4 ? (float)cards[4].Suit : -1.0f,
            LeadPlayer = (float)entity.LeadPlayer,
            LeadSuit = entity.LeadSuit.HasValue ? (float)entity.LeadSuit.Value : -1.0f,
            PlayedCard1Rank = playedCards.Length > 0 ? (float)playedCards[0].Rank : 0.0f,
            PlayedCard1Suit = playedCards.Length > 0 ? (float)playedCards[0].Suit : 0.0f,
            PlayedCard2Rank = playedCards.Length > 1 ? (float)playedCards[1].Rank : 0.0f,
            PlayedCard2Suit = playedCards.Length > 1 ? (float)playedCards[1].Suit : 0.0f,
            PlayedCard3Rank = playedCards.Length > 2 ? (float)playedCards[2].Rank : 0.0f,
            PlayedCard3Suit = playedCards.Length > 2 ? (float)playedCards[2].Suit : 0.0f,
            TeamScore = entity.TeamScore,
            OpponentScore = entity.OpponentScore,
            TrickNumber = entity.TrickNumber,
            CardsPlayedInTrick = playedCards.Length,
            WinningTrickPlayer = entity.WinningTrickPlayer.HasValue ? (float)entity.WinningTrickPlayer.Value : -1.0f,
            Card1IsValid = validityArray[0],
            Card2IsValid = validityArray[1],
            Card3IsValid = validityArray[2],
            Card4IsValid = validityArray[3],
            Card5IsValid = validityArray[4],
            CallingPlayerPosition = (float)entity.CallingPlayer,
            CallingPlayerGoingAlone = entity.CallingPlayerGoingAlone ? 1.0f : 0.0f,
            Card1Chosen = chosenCardIndex == 0 ? 1.0f : 0.0f,
            Card2Chosen = chosenCardIndex == 1 ? 1.0f : 0.0f,
            Card3Chosen = chosenCardIndex == 2 ? 1.0f : 0.0f,
            Card4Chosen = chosenCardIndex == 3 ? 1.0f : 0.0f,
            Card5Chosen = chosenCardIndex == 4 ? 1.0f : 0.0f,
            ExpectedDealPoints = entity.RelativeDealPoints ?? throw new InvalidOperationException(
                "RelativeDealPoints is required for regression training"),
        };
    }
}
