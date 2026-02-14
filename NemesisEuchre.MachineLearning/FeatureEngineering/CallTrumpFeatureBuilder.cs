using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Pooling;
using NemesisEuchre.MachineLearning.Constants;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public static class CallTrumpFeatureBuilder
{
    private const int NumberOfDecisionClasses = FeatureEngineeringConstants.CallTrumpDecisionCount;

    public static CallTrumpTrainingData BuildFeatures(
        Card[] cards,
        Card upCard,
        RelativePlayerPosition dealerPosition,
        short teamScore,
        short opponentScore,
        float decisionOrder,
        CallTrumpDecision[] validDecisions,
        CallTrumpDecision chosenDecision)
    {
        var validityArray = GameEnginePoolManager.RentFloatArray(NumberOfDecisionClasses);
        try
        {
            Array.Clear(validityArray, 0, NumberOfDecisionClasses);

            foreach (var decision in validDecisions)
            {
                validityArray[(int)decision] = 1.0f;
            }

            var chosenIndex = (int)chosenDecision;

            return new CallTrumpTrainingData
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
                UpCardRank = (float)upCard.Rank,
                UpCardSuit = (float)upCard.Suit,
                DealerPosition = (float)dealerPosition,
                TeamScore = teamScore,
                OpponentScore = opponentScore,
                DecisionOrder = decisionOrder,
                Decision0IsValid = validityArray[0],
                Decision1IsValid = validityArray[1],
                Decision2IsValid = validityArray[2],
                Decision3IsValid = validityArray[3],
                Decision4IsValid = validityArray[4],
                Decision5IsValid = validityArray[5],
                Decision6IsValid = validityArray[6],
                Decision7IsValid = validityArray[7],
                Decision8IsValid = validityArray[8],
                Decision9IsValid = validityArray[9],
                Decision10IsValid = validityArray[10],
                Decision0Chosen = chosenIndex == 0 ? 1.0f : 0.0f,
                Decision1Chosen = chosenIndex == 1 ? 1.0f : 0.0f,
                Decision2Chosen = chosenIndex == 2 ? 1.0f : 0.0f,
                Decision3Chosen = chosenIndex == 3 ? 1.0f : 0.0f,
                Decision4Chosen = chosenIndex == 4 ? 1.0f : 0.0f,
                Decision5Chosen = chosenIndex == 5 ? 1.0f : 0.0f,
                Decision6Chosen = chosenIndex == 6 ? 1.0f : 0.0f,
                Decision7Chosen = chosenIndex == 7 ? 1.0f : 0.0f,
                Decision8Chosen = chosenIndex == 8 ? 1.0f : 0.0f,
                Decision9Chosen = chosenIndex == 9 ? 1.0f : 0.0f,
                Decision10Chosen = chosenIndex == 10 ? 1.0f : 0.0f,
            };
        }
        finally
        {
            GameEnginePoolManager.ReturnFloatArray(validityArray);
        }
    }
}
