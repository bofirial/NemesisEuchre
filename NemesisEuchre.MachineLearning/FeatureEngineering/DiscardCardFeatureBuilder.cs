using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public sealed class DiscardCardFeatureBuilder : FeatureBuilderBase<DiscardCardDecisionEntity, DiscardCardTrainingData>
{
    private const int ExpectedCardsInHand = 6;

    public static DiscardCardTrainingData BuildFeatures(
        RelativeCard[] cards,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        short teamScore,
        short opponentScore,
        RelativeCard chosenCard)
    {
        return BuildFeaturesFromContext(
            cards,
            callingPlayer,
            callingPlayerGoingAlone,
            teamScore,
            opponentScore,
            chosenCard);
    }

    protected override DiscardCardTrainingData BuildFeaturesCore(DiscardCardDecisionEntity entity)
    {
        var context = DiscardCardFeatureContextBuilder.Build(entity);

        if (context.CardsInHand.Length != ExpectedCardsInHand)
        {
            throw new InvalidOperationException(
                $"Expected 6 cards in hand but found {context.CardsInHand.Length}");
        }

        var chosenCardIndex = Array.FindIndex(context.CardsInHand, c => c == context.ChosenCard);

        if (chosenCardIndex == -1)
        {
            throw new InvalidOperationException(
                $"Chosen card {context.ChosenCard.Rank} of {context.ChosenCard.Suit} not found in hand");
        }

        var trainingData = BuildFeaturesFromContext(
            context.CardsInHand,
            (RelativePlayerPosition)entity.CallingRelativePlayerPositionId,
            entity.CallingPlayerGoingAlone,
            entity.TeamScore,
            entity.OpponentScore,
            context.ChosenCard);

        trainingData.ExpectedDealPoints = entity.RelativeDealPoints ?? throw new InvalidOperationException(
            "RelativeDealPoints is required for regression training");

        return trainingData;
    }

    protected override void ValidateEntity(DiscardCardDecisionEntity entity)
    {
        if (entity.RelativeDealPoints == null)
        {
            throw new InvalidOperationException("RelativeDealPoints cannot be null");
        }
    }

    private static DiscardCardTrainingData BuildFeaturesFromContext(
        RelativeCard[] cards,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        short teamScore,
        short opponentScore,
        RelativeCard chosenCard)
    {
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
            ChosenCardRank = (float)chosenCard.Rank,
            ChosenCardRelativeSuit = (float)chosenCard.Suit,
        };
    }
}
