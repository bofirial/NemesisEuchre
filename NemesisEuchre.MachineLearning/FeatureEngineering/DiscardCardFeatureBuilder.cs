using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public static class DiscardCardFeatureBuilder
{
    public static DiscardCardTrainingData BuildFeatures(
        RelativeCard[] cards,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        short teamScore,
        short opponentScore,
        RelativeCard chosenCard)
    {
        var chosenCardIndex = Array.FindIndex(cards, c => c == chosenCard);

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
            CallingPlayerPosition = (float)callingPlayer,
            CallingPlayerGoingAlone = callingPlayerGoingAlone ? 1.0f : 0.0f,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            Card1Chosen = chosenCardIndex == 0 ? 1.0f : 0.0f,
            Card2Chosen = chosenCardIndex == 1 ? 1.0f : 0.0f,
            Card3Chosen = chosenCardIndex == 2 ? 1.0f : 0.0f,
            Card4Chosen = chosenCardIndex == 3 ? 1.0f : 0.0f,
            Card5Chosen = chosenCardIndex == 4 ? 1.0f : 0.0f,
            Card6Chosen = chosenCardIndex == 5 ? 1.0f : 0.0f,
        };
    }
}
