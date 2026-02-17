using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public sealed class PlayCardFeatureBuilder : FeatureBuilderBase<PlayCardDecisionEntity, PlayCardTrainingData>
{
    public static PlayCardTrainingData BuildFeatures(PlayCardFeatureBuilderContext context)
    {
        return BuildFeaturesFromContext(context);
    }

    protected override PlayCardTrainingData BuildFeaturesCore(PlayCardDecisionEntity entity)
    {
        var featureContext = PlayCardFeatureContextBuilder.Build(entity);

        var builderContext = new PlayCardFeatureBuilderContext(
            featureContext.CardsInHand,
            featureContext.PlayedCards,
            entity.TeamScore,
            entity.OpponentScore,
            (RelativePlayerPosition)entity.LeadRelativePlayerPositionId,
            entity.LeadRelativeSuitId.HasValue ? (RelativeSuit)entity.LeadRelativeSuitId.Value : null,
            (RelativePlayerPosition)entity.CallingRelativePlayerPositionId,
            entity.CallingPlayerGoingAlone,
            (RelativePlayerPosition)entity.DealerRelativePlayerPositionId,
            featureContext.DealerPickedUpCard,
            featureContext.KnownPlayerSuitVoids,
            featureContext.CardsAccountedFor,
            entity.WinningTrickRelativePlayerPositionId.HasValue ? (RelativePlayerPosition)entity.WinningTrickRelativePlayerPositionId.Value : null,
            entity.TrickNumber,
            entity.WonTricks,
            entity.OpponentsWonTricks,
            featureContext.ChosenCard);

        var trainingData = BuildFeaturesFromContext(builderContext);

        trainingData.ExpectedDealPoints = entity.RelativeDealPoints ?? throw new InvalidOperationException(
            "RelativeDealPoints is required for regression training");

        return trainingData;
    }

    protected override void ValidateEntity(PlayCardDecisionEntity entity)
    {
        if (entity.RelativeDealPoints == null)
        {
            throw new InvalidOperationException("RelativeDealPoints cannot be null");
        }
    }

    private static PlayCardTrainingData BuildFeaturesFromContext(PlayCardFeatureBuilderContext context)
    {
        var cardsInHand = context.CardsInHand;
        var playedCards = context.PlayedCards;
        var teamScore = context.TeamScore;
        var opponentScore = context.OpponentScore;
        var leadPlayer = context.LeadPlayer;
        var leadSuit = context.LeadSuit;
        var callingPlayer = context.CallingPlayer;
        var callingPlayerGoingAlone = context.CallingPlayerGoingAlone;
        var dealer = context.Dealer;
        var dealerPickedUpCard = context.DealerPickedUpCard;
        var knownPlayerSuitVoids = context.KnownPlayerSuitVoids;
        var cardsAccountedFor = context.CardsAccountedFor;
        var winningTrickPlayer = context.WinningTrickPlayer;
        var trickNumber = context.TrickNumber;
        var wonTricks = context.WonTricks;
        var opponentsWonTricks = context.OpponentsWonTricks;
        var chosenCard = context.ChosenCard;

        playedCards.TryGetValue(RelativePlayerPosition.LeftHandOpponent, out RelativeCard? leftHandOpponentPlayedCard);
        playedCards.TryGetValue(RelativePlayerPosition.Partner, out RelativeCard? partnerPlayedCard);
        playedCards.TryGetValue(RelativePlayerPosition.RightHandOpponent, out RelativeCard? rightHandOpponentPlayedCard);

        var (card1Rank, card1Suit) = GetCardFeatures(cardsInHand, 0);
        var (card2Rank, card2Suit) = GetCardFeatures(cardsInHand, 1);
        var (card3Rank, card3Suit) = GetCardFeatures(cardsInHand, 2);
        var (card4Rank, card4Suit) = GetCardFeatures(cardsInHand, 3);
        var (card5Rank, card5Suit) = GetCardFeatures(cardsInHand, 4);

        return new PlayCardTrainingData
        {
            Card1Rank = card1Rank,
            Card1Suit = card1Suit,
            Card2Rank = card2Rank,
            Card2Suit = card2Suit,
            Card3Rank = card3Rank,
            Card3Suit = card3Suit,
            Card4Rank = card4Rank,
            Card4Suit = card4Suit,
            Card5Rank = card5Rank,
            Card5Suit = card5Suit,
            LeadPlayer = (float)leadPlayer,
            LeadSuit = leadSuit.HasValue ? (float)leadSuit.Value : -1.0f,
            LeftHandOpponentPlayedCardRank = (float?)leftHandOpponentPlayedCard?.Rank ?? -1.0f,
            LeftHandOpponentPlayedCardSuit = (float?)leftHandOpponentPlayedCard?.Suit ?? -1.0f,
            PartnerPlayedCardRank = (float?)partnerPlayedCard?.Rank ?? -1.0f,
            PartnerPlayedCardSuit = (float?)partnerPlayedCard?.Suit ?? -1.0f,
            RightHandOpponentPlayedCardRank = (float?)rightHandOpponentPlayedCard?.Rank ?? -1.0f,
            RightHandOpponentPlayedCardSuit = (float?)rightHandOpponentPlayedCard?.Suit ?? -1.0f,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            TrickNumber = trickNumber,
            CardsPlayedInTrick = playedCards.Count,
            WonTricks = wonTricks,
            OpponentsWonTricks = opponentsWonTricks,
            WinningTrickPlayer = winningTrickPlayer.HasValue ? (float)winningTrickPlayer.Value : -1.0f,
            ChosenCardRank = (float)chosenCard.Rank,
            ChosenCardRelativeSuit = (float)chosenCard.Suit,
            CallingPlayerPosition = (float)callingPlayer,
            CallingPlayerGoingAlone = callingPlayerGoingAlone ? 1.0f : 0.0f,
            DealerPlayerPosition = (float)dealer,
            DealerPickedUpCardRank = dealerPickedUpCard != null ? (float)dealerPickedUpCard.Rank : -1.0f,
            DealerPickedUpCardSuit = dealerPickedUpCard != null ? (float)dealerPickedUpCard.Suit : -1.0f,
            LeftHandOpponentMayHaveTrump = HasSuitVoid(knownPlayerSuitVoids, RelativePlayerPosition.LeftHandOpponent, RelativeSuit.Trump),
            LeftHandOpponentMayHaveNonTrumpSameColor = HasSuitVoid(knownPlayerSuitVoids, RelativePlayerPosition.LeftHandOpponent, RelativeSuit.NonTrumpSameColor),
            LeftHandOpponentMayHaveNonTrumpOppositeColor1 = HasSuitVoid(knownPlayerSuitVoids, RelativePlayerPosition.LeftHandOpponent, RelativeSuit.NonTrumpOppositeColor1),
            LeftHandOpponentMayHaveNonTrumpOppositeColor2 = HasSuitVoid(knownPlayerSuitVoids, RelativePlayerPosition.LeftHandOpponent, RelativeSuit.NonTrumpOppositeColor2),
            PartnerMayHaveTrump = HasSuitVoid(knownPlayerSuitVoids, RelativePlayerPosition.Partner, RelativeSuit.Trump),
            PartnerMayHaveNonTrumpSameColor = HasSuitVoid(knownPlayerSuitVoids, RelativePlayerPosition.Partner, RelativeSuit.NonTrumpSameColor),
            PartnerMayHaveNonTrumpOppositeColor1 = HasSuitVoid(knownPlayerSuitVoids, RelativePlayerPosition.Partner, RelativeSuit.NonTrumpOppositeColor1),
            PartnerMayHaveNonTrumpOppositeColor2 = HasSuitVoid(knownPlayerSuitVoids, RelativePlayerPosition.Partner, RelativeSuit.NonTrumpOppositeColor2),
            RightHandOpponentMayHaveTrump = HasSuitVoid(knownPlayerSuitVoids, RelativePlayerPosition.RightHandOpponent, RelativeSuit.Trump),
            RightHandOpponentMayHaveNonTrumpSameColor = HasSuitVoid(knownPlayerSuitVoids, RelativePlayerPosition.RightHandOpponent, RelativeSuit.NonTrumpSameColor),
            RightHandOpponentMayHaveNonTrumpOppositeColor1 = HasSuitVoid(knownPlayerSuitVoids, RelativePlayerPosition.RightHandOpponent, RelativeSuit.NonTrumpOppositeColor1),
            RightHandOpponentMayHaveNonTrumpOppositeColor2 = HasSuitVoid(knownPlayerSuitVoids, RelativePlayerPosition.RightHandOpponent, RelativeSuit.NonTrumpOppositeColor2),
            RightBowerOfTrumpHasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.RightBower, RelativeSuit.Trump),
            LeftBowerOfTrumpHasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.LeftBower, RelativeSuit.Trump),
            AceOfTrumpHasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Ace, RelativeSuit.Trump),
            KingOfTrumpHasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.King, RelativeSuit.Trump),
            QueenOfTrumpHasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Queen, RelativeSuit.Trump),
            TenOfTrumpHasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Ten, RelativeSuit.Trump),
            NineOfTrumpHasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Nine, RelativeSuit.Trump),
            AceOfNonTrumpSameColorHasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Ace, RelativeSuit.NonTrumpSameColor),
            KingOfNonTrumpSameColorHasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.King, RelativeSuit.NonTrumpSameColor),
            QueenOfNonTrumpSameColorHasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Queen, RelativeSuit.NonTrumpSameColor),
            TenOfNonTrumpSameColorHasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Ten, RelativeSuit.NonTrumpSameColor),
            NineOfNonTrumpSameColorHasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Nine, RelativeSuit.NonTrumpSameColor),
            AceOfNonTrumpOppositeColor1HasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Ace, RelativeSuit.NonTrumpOppositeColor1),
            KingOfNonTrumpOppositeColor1HasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.King, RelativeSuit.NonTrumpOppositeColor1),
            QueenOfNonTrumpOppositeColor1HasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Queen, RelativeSuit.NonTrumpOppositeColor1),
            JackOfNonTrumpOppositeColor1HasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Jack, RelativeSuit.NonTrumpOppositeColor1),
            TenOfNonTrumpOppositeColor1HasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Ten, RelativeSuit.NonTrumpOppositeColor1),
            NineOfNonTrumpOppositeColor1HasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Nine, RelativeSuit.NonTrumpOppositeColor1),
            AceOfNonTrumpOppositeColor2HasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Ace, RelativeSuit.NonTrumpOppositeColor2),
            KingOfNonTrumpOppositeColor2HasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.King, RelativeSuit.NonTrumpOppositeColor2),
            QueenOfNonTrumpOppositeColor2HasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Queen, RelativeSuit.NonTrumpOppositeColor2),
            JackOfNonTrumpOppositeColor2HasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Jack, RelativeSuit.NonTrumpOppositeColor2),
            TenOfNonTrumpOppositeColor2HasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Ten, RelativeSuit.NonTrumpOppositeColor2),
            NineOfNonTrumpOppositeColor2HasBeenAccountedFor = IsCardAccountedFor(cardsAccountedFor, Rank.Nine, RelativeSuit.NonTrumpOppositeColor2),
        };
    }

    private static (float rank, float suit) GetCardFeatures(RelativeCard[] cards, int index)
    {
        return cards.Length > index
            ? ((float)cards[index].Rank, (float)cards[index].Suit)
            : (-1.0f, -1.0f);
    }

    private static float HasSuitVoid(
        RelativePlayerSuitVoid[] voids,
        RelativePlayerPosition player,
        RelativeSuit suit)
    {
        return voids.Any(x => x.PlayerPosition == player && x.Suit == suit) ? 0.0f : 1.0f;
    }

    private static float IsCardAccountedFor(
        RelativeCard[] accountedCards,
        Rank rank,
        RelativeSuit suit)
    {
        return accountedCards.Any(card => card.Rank == rank && card.Suit == suit) ? 1.0f : 0.0f;
    }
}
