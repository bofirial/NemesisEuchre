using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public sealed class CallTrumpFeatureBuilder : FeatureBuilderBase<CallTrumpDecisionEntity, AllCallTrumpTrainingData>
{
    public static AllCallTrumpTrainingData BuildFeatures(
        Card[] cards,
        Card upCard,
        RelativePlayerPosition dealerPosition,
        short teamScore,
        short opponentScore,
        byte decisionNumber,
        CallTrumpDecision chosenDecision)
    {
        return BuildFeaturesFromContext(
            cards,
            upCard,
            dealerPosition,
            teamScore,
            opponentScore,
            decisionNumber,
            chosenDecision);
    }

    internal static Suit? CallTrumpDecisionToSuit(CallTrumpDecision decision, Card upCard)
    {
        return decision switch
        {
            CallTrumpDecision.Pass => null,
            CallTrumpDecision.OrderItUp or CallTrumpDecision.OrderItUpAndGoAlone => upCard.Suit,
            CallTrumpDecision.CallSpades or CallTrumpDecision.CallSpadesAndGoAlone => Suit.Spades,
            CallTrumpDecision.CallHearts or CallTrumpDecision.CallHeartsAndGoAlone => Suit.Hearts,
            CallTrumpDecision.CallClubs or CallTrumpDecision.CallClubsAndGoAlone => Suit.Clubs,
            CallTrumpDecision.CallDiamonds or CallTrumpDecision.CallDiamondsAndGoAlone => Suit.Diamonds,
            _ => null,
        };
    }

    protected override AllCallTrumpTrainingData BuildFeaturesCore(CallTrumpDecisionEntity entity)
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

    private static AllCallTrumpTrainingData BuildFeaturesFromContext(
        Card[] cards,
        Card upCard,
        RelativePlayerPosition dealerPosition,
        short teamScore,
        short opponentScore,
        byte decisionNumber,
        CallTrumpDecision chosenDecision)
    {
        var trumpSuit = CallTrumpDecisionToSuit(chosenDecision, upCard);
        var isPass = trumpSuit == null;

        float card1Threats = -1f;
        float card2Threats = -1f;
        float card3Threats = -1f;
        float card4Threats = -1f;
        float card5Threats = -1f;
        float upCardThreats = -1f;

        if (!isPass)
        {
            var suit = trumpSuit!.Value;
            var relativeCards = new RelativeCard[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                relativeCards[i] = cards[i].ToRelative(suit);
            }

            var relativeUpCard = upCard.ToRelative(suit);

            var isUpCardAccountedFor = decisionNumber >= 5 || dealerPosition == RelativePlayerPosition.Self;
            var accountedFor = isUpCardAccountedFor
                ? [.. relativeCards, relativeUpCard]
                : relativeCards;

            RelativePlayerSuitVoid[] emptyVoids = [];

            card1Threats = ThreatCalculator.CalculateThreats(relativeCards[0], accountedFor, emptyVoids);
            card2Threats = ThreatCalculator.CalculateThreats(relativeCards[1], accountedFor, emptyVoids);
            card3Threats = ThreatCalculator.CalculateThreats(relativeCards[2], accountedFor, emptyVoids);
            card4Threats = ThreatCalculator.CalculateThreats(relativeCards[3], accountedFor, emptyVoids);
            card5Threats = ThreatCalculator.CalculateThreats(relativeCards[4], accountedFor, emptyVoids);

            if (dealerPosition == RelativePlayerPosition.Self && decisionNumber <= 4)
            {
                upCardThreats = ThreatCalculator.CalculateThreats(relativeUpCard, accountedFor, emptyVoids);
            }
        }

        return new AllCallTrumpTrainingData
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
            DecisionNumber = decisionNumber,
            ChosenDecision = (float)chosenDecision,
            Card1Threats = card1Threats,
            Card2Threats = card2Threats,
            Card3Threats = card3Threats,
            Card4Threats = card4Threats,
            Card5Threats = card5Threats,
            UpCardThreats = upCardThreats,
            ScoreDifferential = teamScore - opponentScore,
        };
    }
}
