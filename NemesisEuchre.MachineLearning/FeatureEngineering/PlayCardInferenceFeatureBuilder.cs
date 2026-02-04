using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public interface IPlayCardInferenceFeatureBuilder
{
    PlayCardTrainingData BuildFeatures(
        RelativeCard[] cardsInHand,
        RelativePlayerPosition leadPlayer,
        RelativeSuit? leadSuit,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCards,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        (RelativePlayerPosition PlayerPosition, RelativeSuit Suit)[] knownPlayerSuitVoids,
        RelativeCard[] cardsAccountedFor,
        RelativePlayerPosition? winningTrickPlayer,
        short trickNumber,
        RelativeCard[] validCardsToPlay,
        RelativeCard chosenCard);
}

public class PlayCardInferenceFeatureBuilder : IPlayCardInferenceFeatureBuilder
{
    public PlayCardTrainingData BuildFeatures(
        RelativeCard[] cardsInHand,
        RelativePlayerPosition leadPlayer,
        RelativeSuit? leadSuit,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCards,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        (RelativePlayerPosition PlayerPosition, RelativeSuit Suit)[] knownPlayerSuitVoids,
        RelativeCard[] cardsAccountedFor,
        RelativePlayerPosition? winningTrickPlayer,
        short trickNumber,
        RelativeCard[] validCardsToPlay,
        RelativeCard chosenCard)
    {
        var playedCardsArray = playedCards
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => kvp.Value)
            .ToArray();

        var validityArray = new float[5];
        foreach (var card in validCardsToPlay)
        {
            var index = Array.IndexOf(cardsInHand, card);
            if (index is >= 0 and < 5)
            {
                validityArray[index] = 1.0f;
            }
        }

        var chosenIndex = Array.IndexOf(cardsInHand, chosenCard);

        return new PlayCardTrainingData
        {
            Card1Rank = cardsInHand.Length > 0 ? (float)cardsInHand[0].Rank : -1.0f,
            Card1Suit = cardsInHand.Length > 0 ? (float)cardsInHand[0].Suit : -1.0f,
            Card2Rank = cardsInHand.Length > 1 ? (float)cardsInHand[1].Rank : -1.0f,
            Card2Suit = cardsInHand.Length > 1 ? (float)cardsInHand[1].Suit : -1.0f,
            Card3Rank = cardsInHand.Length > 2 ? (float)cardsInHand[2].Rank : -1.0f,
            Card3Suit = cardsInHand.Length > 2 ? (float)cardsInHand[2].Suit : -1.0f,
            Card4Rank = cardsInHand.Length > 3 ? (float)cardsInHand[3].Rank : -1.0f,
            Card4Suit = cardsInHand.Length > 3 ? (float)cardsInHand[3].Suit : -1.0f,
            Card5Rank = cardsInHand.Length > 4 ? (float)cardsInHand[4].Rank : -1.0f,
            Card5Suit = cardsInHand.Length > 4 ? (float)cardsInHand[4].Suit : -1.0f,
            LeadPlayer = (float)leadPlayer,
            LeadSuit = leadSuit.HasValue ? (float)leadSuit.Value : -1.0f,
            PlayedCard1Rank = playedCardsArray.Length > 0 ? (float)playedCardsArray[0].Rank : -1.0f,
            PlayedCard1Suit = playedCardsArray.Length > 0 ? (float)playedCardsArray[0].Suit : -1.0f,
            PlayedCard2Rank = playedCardsArray.Length > 1 ? (float)playedCardsArray[1].Rank : -1.0f,
            PlayedCard2Suit = playedCardsArray.Length > 1 ? (float)playedCardsArray[1].Suit : -1.0f,
            PlayedCard3Rank = playedCardsArray.Length > 2 ? (float)playedCardsArray[2].Rank : -1.0f,
            PlayedCard3Suit = playedCardsArray.Length > 2 ? (float)playedCardsArray[2].Suit : -1.0f,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            TrickNumber = trickNumber,
            CardsPlayedInTrick = playedCardsArray.Length,
            WinningTrickPlayer = winningTrickPlayer.HasValue ? (float)winningTrickPlayer.Value : -1.0f,
            Card1IsValid = validityArray[0],
            Card2IsValid = validityArray[1],
            Card3IsValid = validityArray[2],
            Card4IsValid = validityArray[3],
            Card5IsValid = validityArray[4],
            CallingPlayerPosition = (float)callingPlayer,
            CallingPlayerGoingAlone = callingPlayerGoingAlone ? 1.0f : 0.0f,
            Card1Chosen = chosenIndex == 0 ? 1.0f : 0.0f,
            Card2Chosen = chosenIndex == 1 ? 1.0f : 0.0f,
            Card3Chosen = chosenIndex == 2 ? 1.0f : 0.0f,
            Card4Chosen = chosenIndex == 3 ? 1.0f : 0.0f,
            Card5Chosen = chosenIndex == 4 ? 1.0f : 0.0f,
        };
    }
}
