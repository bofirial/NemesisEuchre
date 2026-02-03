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
        var data = new DiscardCardTrainingData
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
        };

        var chosenIndex = Array.IndexOf(cardsInHand, chosenCard);
        data.Card1Chosen = chosenIndex == 0 ? 1.0f : 0.0f;
        data.Card2Chosen = chosenIndex == 1 ? 1.0f : 0.0f;
        data.Card3Chosen = chosenIndex == 2 ? 1.0f : 0.0f;
        data.Card4Chosen = chosenIndex == 3 ? 1.0f : 0.0f;
        data.Card5Chosen = chosenIndex == 4 ? 1.0f : 0.0f;
        data.Card6Chosen = chosenIndex == 5 ? 1.0f : 0.0f;

        return data;
    }
}
