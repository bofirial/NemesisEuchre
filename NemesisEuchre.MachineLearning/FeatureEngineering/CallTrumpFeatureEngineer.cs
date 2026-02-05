using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Pooling;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public class CallTrumpFeatureEngineer : IFeatureEngineer<CallTrumpDecisionEntity, CallTrumpTrainingData>
{
    private const int NumberOfDecisionClasses = 11;

    public CallTrumpTrainingData Transform(CallTrumpDecisionEntity entity)
    {
        var cards = JsonDeserializationHelper.DeserializeCards(entity.CardsInHandJson);
        var upCard = JsonDeserializationHelper.DeserializeCard(entity.UpCardJson);
        var validDecisions = JsonDeserializationHelper.DeserializeCallTrumpDecisions(entity.ValidDecisionsJson);
        var chosenDecision = JsonDeserializationHelper.DeserializeCallTrumpDecision(entity.ChosenDecisionJson);

        var validityArray = GameEnginePoolManager.RentFloatArray(NumberOfDecisionClasses);
        try
        {
            Array.Clear(validityArray, 0, NumberOfDecisionClasses);

            foreach (var decision in validDecisions)
            {
                validityArray[(int)decision] = 1.0f;
            }

            if (!validDecisions.Contains(chosenDecision))
            {
                throw new InvalidOperationException(
                    $"Chosen decision {chosenDecision} is not in the valid decisions array");
            }

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
                DealerPosition = (float)entity.DealerPosition,
                TeamScore = entity.TeamScore,
                OpponentScore = entity.OpponentScore,
                DecisionOrder = entity.DecisionOrder,
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
                Decision0Chosen = chosenDecision == CallTrumpDecision.Pass ? 1.0f : 0.0f,
                Decision1Chosen = chosenDecision == CallTrumpDecision.CallSpades ? 1.0f : 0.0f,
                Decision2Chosen = chosenDecision == CallTrumpDecision.CallHearts ? 1.0f : 0.0f,
                Decision3Chosen = chosenDecision == CallTrumpDecision.CallClubs ? 1.0f : 0.0f,
                Decision4Chosen = chosenDecision == CallTrumpDecision.CallDiamonds ? 1.0f : 0.0f,
                Decision5Chosen = chosenDecision == CallTrumpDecision.CallSpadesAndGoAlone ? 1.0f : 0.0f,
                Decision6Chosen = chosenDecision == CallTrumpDecision.CallHeartsAndGoAlone ? 1.0f : 0.0f,
                Decision7Chosen = chosenDecision == CallTrumpDecision.CallClubsAndGoAlone ? 1.0f : 0.0f,
                Decision8Chosen = chosenDecision == CallTrumpDecision.CallDiamondsAndGoAlone ? 1.0f : 0.0f,
                Decision9Chosen = chosenDecision == CallTrumpDecision.OrderItUp ? 1.0f : 0.0f,
                Decision10Chosen = chosenDecision == CallTrumpDecision.OrderItUpAndGoAlone ? 1.0f : 0.0f,
                ExpectedDealPoints = entity.RelativeDealPoints ?? throw new InvalidOperationException(
                    "RelativeDealPoints is required for regression training"),
            };
        }
        finally
        {
            GameEnginePoolManager.ReturnFloatArray(validityArray);
        }
    }
}
