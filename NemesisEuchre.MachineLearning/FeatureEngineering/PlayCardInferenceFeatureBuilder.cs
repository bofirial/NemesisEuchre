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
        RelativePlayerPosition? winningTrickPlayer,
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
        RelativePlayerPosition? winningTrickPlayer,
        RelativeCard[] validCardsToPlay,
        RelativeCard chosenCard)
    {
        var data = new PlayCardTrainingData
        {
            Card1Rank = cardsInHand.Length > 0 ? (float)cardsInHand[0].Rank : 0f,
            Card1Suit = cardsInHand.Length > 0 ? (float)cardsInHand[0].Suit : 0f,
            Card2Rank = cardsInHand.Length > 1 ? (float)cardsInHand[1].Rank : 0f,
            Card2Suit = cardsInHand.Length > 1 ? (float)cardsInHand[1].Suit : 0f,
            Card3Rank = cardsInHand.Length > 2 ? (float)cardsInHand[2].Rank : 0f,
            Card3Suit = cardsInHand.Length > 2 ? (float)cardsInHand[2].Suit : 0f,
            Card4Rank = cardsInHand.Length > 3 ? (float)cardsInHand[3].Rank : 0f,
            Card4Suit = cardsInHand.Length > 3 ? (float)cardsInHand[3].Suit : 0f,
            Card5Rank = cardsInHand.Length > 4 ? (float)cardsInHand[4].Rank : 0f,
            Card5Suit = cardsInHand.Length > 4 ? (float)cardsInHand[4].Suit : 0f,
            LeadPlayer = (float)leadPlayer,
            LeadSuit = leadSuit.HasValue ? (float)leadSuit.Value : -1.0f,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            TrickNumber = 0f,
            CardsPlayedInTrick = playedCards.Count,
            WinningTrickPlayer = winningTrickPlayer.HasValue ? (float)winningTrickPlayer.Value : -1.0f,
            CallingPlayerPosition = (float)callingPlayer,
            CallingPlayerGoingAlone = callingPlayerGoingAlone ? 1.0f : 0.0f,
        };

        playedCards.TryGetValue(RelativePlayerPosition.Self, out var card0);
        playedCards.TryGetValue(RelativePlayerPosition.LeftHandOpponent, out var card1);
        playedCards.TryGetValue(RelativePlayerPosition.Partner, out var card2);

        data.PlayedCard1Rank = card0 != null ? (float)card0.Rank : 0f;
        data.PlayedCard1Suit = card0 != null ? (float)card0.Suit : 0f;
        data.PlayedCard2Rank = card1 != null ? (float)card1.Rank : 0f;
        data.PlayedCard2Suit = card1 != null ? (float)card1.Suit : 0f;
        data.PlayedCard3Rank = card2 != null ? (float)card2.Rank : 0f;
        data.PlayedCard3Suit = card2 != null ? (float)card2.Suit : 0f;

        var validityArray = new float[5];
        foreach (var card in validCardsToPlay)
        {
            var index = Array.IndexOf(cardsInHand, card);
            if (index is >= 0 and < 5)
            {
                validityArray[index] = 1.0f;
            }
        }

        data.Card1IsValid = validityArray[0];
        data.Card2IsValid = validityArray[1];
        data.Card3IsValid = validityArray[2];
        data.Card4IsValid = validityArray[3];
        data.Card5IsValid = validityArray[4];

        var chosenIndex = Array.IndexOf(cardsInHand, chosenCard);
        data.Card1Chosen = chosenIndex == 0 ? 1.0f : 0.0f;
        data.Card2Chosen = chosenIndex == 1 ? 1.0f : 0.0f;
        data.Card3Chosen = chosenIndex == 2 ? 1.0f : 0.0f;
        data.Card4Chosen = chosenIndex == 3 ? 1.0f : 0.0f;
        data.Card5Chosen = chosenIndex == 4 ? 1.0f : 0.0f;

        return data;
    }
}
