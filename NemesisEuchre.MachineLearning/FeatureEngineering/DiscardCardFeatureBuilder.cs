using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public sealed class DiscardCardFeatureBuilder : FeatureBuilderBase<DiscardCardDecisionEntity, AllDiscardCardTrainingData>
{
    private const int ExpectedCardsInHand = 6;

    public static AllDiscardCardTrainingData BuildFeatures(
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

    protected override AllDiscardCardTrainingData BuildFeaturesCore(DiscardCardDecisionEntity entity)
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

    private static AllDiscardCardTrainingData BuildFeaturesFromContext(
        RelativeCard[] cards,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        short teamScore,
        short opponentScore,
        RelativeCard chosenCard)
    {
        RelativePlayerSuitVoid[] emptyVoids = [];

        var card1Threats = ThreatCalculator.CalculateThreats(cards[0], cards, emptyVoids);
        var card2Threats = ThreatCalculator.CalculateThreats(cards[1], cards, emptyVoids);
        var card3Threats = ThreatCalculator.CalculateThreats(cards[2], cards, emptyVoids);
        var card4Threats = ThreatCalculator.CalculateThreats(cards[3], cards, emptyVoids);
        var card5Threats = ThreatCalculator.CalculateThreats(cards[4], cards, emptyVoids);
        var card6Threats = ThreatCalculator.CalculateThreats(cards[5], cards, emptyVoids);
        var chosenCardThreats = ThreatCalculator.CalculateThreats(chosenCard, cards, emptyVoids);

        var numberOfNonTrumpSuits = CountNonTrumpSuits(cards);
        var numberOfNonTrumpSuitsAfterDiscard = CountNonTrumpSuitsExcluding(cards, chosenCard);

        return new AllDiscardCardTrainingData
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
            Card1Threats = card1Threats,
            Card2Threats = card2Threats,
            Card3Threats = card3Threats,
            Card4Threats = card4Threats,
            Card5Threats = card5Threats,
            Card6Threats = card6Threats,
            ChosenCardThreats = chosenCardThreats,
            NumberOfNonTrumpSuits = numberOfNonTrumpSuits,
            NumberOfNonTrumpSuitsAfterDiscard = numberOfNonTrumpSuitsAfterDiscard,
        };
    }

    private static float CountNonTrumpSuits(RelativeCard[] cards)
    {
        var hasSuit = new bool[4];
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i].Suit != RelativeSuit.Trump)
            {
                hasSuit[(int)cards[i].Suit] = true;
            }
        }

        float count = 0f;
        for (int i = 0; i < hasSuit.Length; i++)
        {
            if (hasSuit[i])
            {
                count++;
            }
        }

        return count;
    }

    private static float CountNonTrumpSuitsExcluding(RelativeCard[] cards, RelativeCard excluded)
    {
        var hasSuit = new bool[4];
        var excludedOnce = false;
        for (int i = 0; i < cards.Length; i++)
        {
            if (!excludedOnce && cards[i].Rank == excluded.Rank && cards[i].Suit == excluded.Suit)
            {
                excludedOnce = true;
                continue;
            }

            if (cards[i].Suit != RelativeSuit.Trump)
            {
                hasSuit[(int)cards[i].Suit] = true;
            }
        }

        float count = 0f;
        for (int i = 0; i < hasSuit.Length; i++)
        {
            if (hasSuit[i])
            {
                count++;
            }
        }

        return count;
    }
}
