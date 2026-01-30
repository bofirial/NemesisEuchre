using System.Text.Json;

using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public class PlayCardFeatureEngineer : IFeatureEngineer<PlayCardDecisionEntity, PlayCardTrainingData>
{
    public PlayCardTrainingData Transform(PlayCardDecisionEntity entity)
    {
        var cards = JsonSerializer.Deserialize<RelativeCard[]>(
            entity.CardsInHandJson,
            JsonSerializationOptions.Default)!;

        var playedCardsDict = JsonSerializer.Deserialize<Dictionary<RelativePlayerPosition, RelativeCard>>(
            entity.PlayedCardsJson,
            JsonSerializationOptions.Default)!;

        var playedCards = playedCardsDict
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => kvp.Value)
            .ToArray();

        var validCards = JsonSerializer.Deserialize<RelativeCard[]>(
            entity.ValidCardsToPlayJson,
            JsonSerializationOptions.Default)!;

        var validityArray = new float[5];
        foreach (var validCard in validCards)
        {
            var index = Array.FindIndex(cards, c =>
                c.Rank == validCard.Rank && c.Suit == validCard.Suit);
            if (index != -1)
            {
                validityArray[index] = 1.0f;
            }
        }

        var chosenCard = JsonSerializer.Deserialize<RelativeCard>(
            entity.ChosenCardJson,
            JsonSerializationOptions.Default)!;

        var chosenCardIndex = Array.FindIndex(cards, c =>
            c.Rank == chosenCard.Rank && c.Suit == chosenCard.Suit);

        if (chosenCardIndex == -1)
        {
            throw new InvalidOperationException(
                $"Chosen card {chosenCard.Rank} of {chosenCard.Suit} not found in hand");
        }

        return new PlayCardTrainingData
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
            TrickNumber = entity.Trick.TrickNumber,
            CardsPlayedInTrick = playedCards.Length,
            WinningTrickPlayer = entity.WinningTrickPlayer.HasValue ? (float)entity.WinningTrickPlayer.Value : -1.0f,
            Card1IsValid = validityArray[0],
            Card2IsValid = validityArray[1],
            Card3IsValid = validityArray[2],
            Card4IsValid = validityArray[3],
            Card5IsValid = validityArray[4],
            CallingPlayerPosition = (float)entity.CallingPlayer,
            CallingPlayerGoingAlone = entity.CallingPlayerGoingAlone ? 1.0f : 0.0f,
            ChosenCardIndex = (uint)chosenCardIndex,
        };
    }
}
