using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public interface IDiscardCardInferenceFeatureBuilder
{
    DiscardCardTrainingData BuildFeatures(
        RelativeCard[] cardsInHand,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        short teamScore,
        short opponentScore,
        RelativeCard chosenCard);
}

public class DiscardCardInferenceFeatureBuilder : IDiscardCardInferenceFeatureBuilder
{
    public DiscardCardTrainingData BuildFeatures(
        RelativeCard[] cardsInHand,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        short teamScore,
        short opponentScore,
        RelativeCard chosenCard)
    {
        return new DiscardCardTrainingData
        {
            Card1Rank = (float)cardsInHand[0].Rank,
            Card1Suit = (float)cardsInHand[0].Suit,
            Card2Rank = (float)cardsInHand[1].Rank,
            Card2Suit = (float)cardsInHand[1].Suit,
            Card3Rank = (float)cardsInHand[2].Rank,
            Card3Suit = (float)cardsInHand[2].Suit,
            Card4Rank = (float)cardsInHand[3].Rank,
            Card4Suit = (float)cardsInHand[3].Suit,
            Card5Rank = (float)cardsInHand[4].Rank,
            Card5Suit = (float)cardsInHand[4].Suit,
            Card6Rank = (float)cardsInHand[5].Rank,
            Card6Suit = (float)cardsInHand[5].Suit,
            CallingPlayerPosition = (float)callingPlayer,
            CallingPlayerGoingAlone = callingPlayerGoingAlone ? 1.0f : 0.0f,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            Card1Chosen = cardsInHand[0].Rank == chosenCard.Rank && cardsInHand[0].Suit == chosenCard.Suit ? 1.0f : 0.0f,
            Card2Chosen = cardsInHand[1].Rank == chosenCard.Rank && cardsInHand[1].Suit == chosenCard.Suit ? 1.0f : 0.0f,
            Card3Chosen = cardsInHand[2].Rank == chosenCard.Rank && cardsInHand[2].Suit == chosenCard.Suit ? 1.0f : 0.0f,
            Card4Chosen = cardsInHand[3].Rank == chosenCard.Rank && cardsInHand[3].Suit == chosenCard.Suit ? 1.0f : 0.0f,
            Card5Chosen = cardsInHand[4].Rank == chosenCard.Rank && cardsInHand[4].Suit == chosenCard.Suit ? 1.0f : 0.0f,
            Card6Chosen = cardsInHand[5].Rank == chosenCard.Rank && cardsInHand[5].Suit == chosenCard.Suit ? 1.0f : 0.0f,
        };
    }
}
