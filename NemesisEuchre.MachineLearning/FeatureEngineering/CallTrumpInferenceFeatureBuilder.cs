using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Pooling;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public interface ICallTrumpInferenceFeatureBuilder
{
    CallTrumpTrainingData BuildFeatures(
        Card[] cardsInHand,
        Card upCard,
        PlayerPosition dealerPosition,
        short teamScore,
        short opponentScore,
        CallTrumpDecision[] validDecisions,
        CallTrumpDecision chosenDecision);
}

public class CallTrumpInferenceFeatureBuilder : ICallTrumpInferenceFeatureBuilder
{
    public CallTrumpTrainingData BuildFeatures(
        Card[] cardsInHand,
        Card upCard,
        PlayerPosition dealerPosition,
        short teamScore,
        short opponentScore,
        CallTrumpDecision[] validDecisions,
        CallTrumpDecision chosenDecision)
    {
        var data = new CallTrumpTrainingData
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
            UpCardRank = (float)upCard.Rank,
            UpCardSuit = (float)upCard.Suit,
            DealerPosition = (float)dealerPosition,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            DecisionOrder = 0f,
        };

        var validityArray = GameEnginePoolManager.RentFloatArray(11);
        try
        {
            Array.Clear(validityArray, 0, 11);

            foreach (var decision in validDecisions)
            {
                validityArray[(int)decision] = 1.0f;
            }

            data.Decision0IsValid = validityArray[0];
            data.Decision1IsValid = validityArray[1];
            data.Decision2IsValid = validityArray[2];
            data.Decision3IsValid = validityArray[3];
            data.Decision4IsValid = validityArray[4];
            data.Decision5IsValid = validityArray[5];
            data.Decision6IsValid = validityArray[6];
            data.Decision7IsValid = validityArray[7];
            data.Decision8IsValid = validityArray[8];
            data.Decision9IsValid = validityArray[9];
            data.Decision10IsValid = validityArray[10];

            var chosenIndex = (int)chosenDecision;
            data.Decision0Chosen = chosenIndex == 0 ? 1.0f : 0.0f;
            data.Decision1Chosen = chosenIndex == 1 ? 1.0f : 0.0f;
            data.Decision2Chosen = chosenIndex == 2 ? 1.0f : 0.0f;
            data.Decision3Chosen = chosenIndex == 3 ? 1.0f : 0.0f;
            data.Decision4Chosen = chosenIndex == 4 ? 1.0f : 0.0f;
            data.Decision5Chosen = chosenIndex == 5 ? 1.0f : 0.0f;
            data.Decision6Chosen = chosenIndex == 6 ? 1.0f : 0.0f;
            data.Decision7Chosen = chosenIndex == 7 ? 1.0f : 0.0f;
            data.Decision8Chosen = chosenIndex == 8 ? 1.0f : 0.0f;
            data.Decision9Chosen = chosenIndex == 9 ? 1.0f : 0.0f;
            data.Decision10Chosen = chosenIndex == 10 ? 1.0f : 0.0f;

            return data;
        }
        finally
        {
            GameEnginePoolManager.ReturnFloatArray(validityArray);
        }
    }
}
