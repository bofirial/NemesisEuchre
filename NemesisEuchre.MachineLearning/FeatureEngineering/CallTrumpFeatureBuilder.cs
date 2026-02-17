using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public sealed class CallTrumpFeatureBuilder : FeatureBuilderBase<CallTrumpDecisionEntity, CallTrumpTrainingData>
{
    public static CallTrumpTrainingData BuildFeatures(
        Card[] cards,
        Card upCard,
        RelativePlayerPosition dealerPosition,
        short teamScore,
        short opponentScore,
        float decisionOrder,
        CallTrumpDecision chosenDecision)
    {
        return BuildFeaturesFromContext(
            cards,
            upCard,
            dealerPosition,
            teamScore,
            opponentScore,
            decisionOrder,
            chosenDecision);
    }

    protected override CallTrumpTrainingData BuildFeaturesCore(CallTrumpDecisionEntity entity)
    {
        var context = CallTrumpFeatureContextBuilder.Build(entity);

        var trainingData = BuildFeaturesFromContext(
            context.Cards,
            context.UpCard,
            (RelativePlayerPosition)entity.DealerRelativePositionId,
            entity.TeamScore,
            entity.OpponentScore,
            entity.DecisionOrder,
            context.ChosenDecision);

        trainingData.ExpectedDealPoints = entity.RelativeDealPoints ?? throw new InvalidOperationException(
            "RelativeDealPoints is required for regression training");

        return trainingData;
    }

    protected override void ValidateEntity(CallTrumpDecisionEntity entity)
    {
        if (entity.RelativeDealPoints == null)
        {
            throw new InvalidOperationException("RelativeDealPoints cannot be null");
        }
    }

    private static CallTrumpTrainingData BuildFeaturesFromContext(
        Card[] cards,
        Card upCard,
        RelativePlayerPosition dealerPosition,
        short teamScore,
        short opponentScore,
        float decisionOrder,
        CallTrumpDecision chosenDecision)
    {
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
            ChosenDecision = (float)chosenDecision,
        };
    }
}
