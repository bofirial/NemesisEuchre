using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public sealed class PlayCardFeatureBuilder : FeatureBuilderBase<PlayCardDecisionEntity, AllPlayCardTrainingData>
{
    private static readonly Rank[] TrumpRanks =
        [Rank.Nine, Rank.Ten, Rank.Queen, Rank.King, Rank.Ace, Rank.LeftBower, Rank.RightBower];

    private static readonly Rank[] NonTrumpSameColorRanks =
        [Rank.Nine, Rank.Ten, Rank.Queen, Rank.King, Rank.Ace];

    private static readonly Rank[] NonTrumpOppositeColorRanks =
        [Rank.Nine, Rank.Ten, Rank.Jack, Rank.Queen, Rank.King, Rank.Ace];

    public static AllPlayCardTrainingData BuildFeatures(PlayCardFeatureBuilderContext context)
    {
        return BuildFeaturesFromContext(context);
    }

    protected override AllPlayCardTrainingData BuildFeaturesCore(PlayCardDecisionEntity entity)
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

    private static AllPlayCardTrainingData BuildFeaturesFromContext(PlayCardFeatureBuilderContext context)
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

        var effectiveAccountedFor = BuildEffectiveAccountedFor(cardsAccountedFor, dealer, dealerPickedUpCard);

        playedCards.TryGetValue(RelativePlayerPosition.LeftHandOpponent, out RelativeCard? leftHandOpponentPlayedCard);
        playedCards.TryGetValue(RelativePlayerPosition.Partner, out RelativeCard? partnerPlayedCard);
        playedCards.TryGetValue(RelativePlayerPosition.RightHandOpponent, out RelativeCard? rightHandOpponentPlayedCard);

        var (card1Rank, card1Suit) = GetCardFeatures(cardsInHand, 0);
        var (card2Rank, card2Suit) = GetCardFeatures(cardsInHand, 1);
        var (card3Rank, card3Suit) = GetCardFeatures(cardsInHand, 2);
        var (card4Rank, card4Suit) = GetCardFeatures(cardsInHand, 3);
        var (card5Rank, card5Suit) = GetCardFeatures(cardsInHand, 4);

        return new AllPlayCardTrainingData
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
            Card1Threats = GetCardThreats(cardsInHand, 0, effectiveAccountedFor, knownPlayerSuitVoids),
            Card2Threats = GetCardThreats(cardsInHand, 1, effectiveAccountedFor, knownPlayerSuitVoids),
            Card3Threats = GetCardThreats(cardsInHand, 2, effectiveAccountedFor, knownPlayerSuitVoids),
            Card4Threats = GetCardThreats(cardsInHand, 3, effectiveAccountedFor, knownPlayerSuitVoids),
            Card5Threats = GetCardThreats(cardsInHand, 4, effectiveAccountedFor, knownPlayerSuitVoids),
            Card1ThreatsThisTrick = GetCardThreatsThisTrick(cardsInHand, 0, effectiveAccountedFor, knownPlayerSuitVoids, playedCards, leadPlayer, leadSuit, winningTrickPlayer, callingPlayer, callingPlayerGoingAlone),
            Card2ThreatsThisTrick = GetCardThreatsThisTrick(cardsInHand, 1, effectiveAccountedFor, knownPlayerSuitVoids, playedCards, leadPlayer, leadSuit, winningTrickPlayer, callingPlayer, callingPlayerGoingAlone),
            Card3ThreatsThisTrick = GetCardThreatsThisTrick(cardsInHand, 2, effectiveAccountedFor, knownPlayerSuitVoids, playedCards, leadPlayer, leadSuit, winningTrickPlayer, callingPlayer, callingPlayerGoingAlone),
            Card4ThreatsThisTrick = GetCardThreatsThisTrick(cardsInHand, 3, effectiveAccountedFor, knownPlayerSuitVoids, playedCards, leadPlayer, leadSuit, winningTrickPlayer, callingPlayer, callingPlayerGoingAlone),
            Card5ThreatsThisTrick = GetCardThreatsThisTrick(cardsInHand, 4, effectiveAccountedFor, knownPlayerSuitVoids, playedCards, leadPlayer, leadSuit, winningTrickPlayer, callingPlayer, callingPlayerGoingAlone),
            ChosenCardThreats = CalculateThreats(chosenCard, effectiveAccountedFor, knownPlayerSuitVoids),
            ChosenCardThreatsThisTrick = CalculateThreatsThisTrick(chosenCard, effectiveAccountedFor, knownPlayerSuitVoids, playedCards, leadPlayer, leadSuit, winningTrickPlayer, callingPlayer, callingPlayerGoingAlone),
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

    private static RelativeCard[] BuildEffectiveAccountedFor(
        RelativeCard[] cardsAccountedFor,
        RelativePlayerPosition dealer,
        RelativeCard? dealerPickedUpCard)
    {
        if (dealerPickedUpCard == null)
        {
            return cardsAccountedFor;
        }

        if (dealer is RelativePlayerPosition.LeftHandOpponent or RelativePlayerPosition.RightHandOpponent)
        {
            return [.. cardsAccountedFor
                .Where(c => c.Rank != dealerPickedUpCard.Rank || c.Suit != dealerPickedUpCard.Suit)];
        }

        return cardsAccountedFor;
    }

    private static float GetCardThreats(
        RelativeCard[] cardsInHand,
        int index,
        RelativeCard[] effectiveAccountedFor,
        RelativePlayerSuitVoid[] knownPlayerSuitVoids)
    {
        if (index >= cardsInHand.Length)
        {
            return -1f;
        }

        return CalculateThreats(cardsInHand[index], effectiveAccountedFor, knownPlayerSuitVoids);
    }

    private static float CalculateThreats(
        RelativeCard card,
        RelativeCard[] effectiveAccountedFor,
        RelativePlayerSuitVoid[] knownPlayerSuitVoids)
    {
        if (card.Suit == RelativeSuit.Trump)
        {
            if (BothOpponentsVoidIn(knownPlayerSuitVoids, RelativeSuit.Trump))
            {
                return 0f;
            }

            return CountUnaccountedHigherCards(card.Rank, RelativeSuit.Trump, TrumpRanks, effectiveAccountedFor);
        }

        float threats = 0f;

        if (!BothOpponentsVoidIn(knownPlayerSuitVoids, RelativeSuit.Trump))
        {
            threats += CountAllUnaccountedTrumpCards(effectiveAccountedFor);
        }

        if (!BothOpponentsVoidIn(knownPlayerSuitVoids, card.Suit))
        {
            Rank[] ranksForSuit = card.Suit == RelativeSuit.NonTrumpSameColor
                ? NonTrumpSameColorRanks
                : NonTrumpOppositeColorRanks;

            threats += CountUnaccountedHigherCards(card.Rank, card.Suit, ranksForSuit, effectiveAccountedFor);
        }

        return threats;
    }

    private static bool BothOpponentsVoidIn(RelativePlayerSuitVoid[] voids, RelativeSuit suit)
    {
        bool lhoVoid = false;
        bool rhoVoid = false;

        for (int i = 0; i < voids.Length; i++)
        {
            if (voids[i].Suit == suit)
            {
                if (voids[i].PlayerPosition == RelativePlayerPosition.LeftHandOpponent)
                {
                    lhoVoid = true;
                }
                else if (voids[i].PlayerPosition == RelativePlayerPosition.RightHandOpponent)
                {
                    rhoVoid = true;
                }
            }
        }

        return lhoVoid && rhoVoid;
    }

    private static float CountUnaccountedHigherCards(
        Rank rank,
        RelativeSuit suit,
        Rank[] validRanks,
        RelativeCard[] effectiveAccountedFor)
    {
        float count = 0f;

        for (int i = 0; i < validRanks.Length; i++)
        {
            if (validRanks[i] > rank && !IsAccountedFor(validRanks[i], suit, effectiveAccountedFor))
            {
                count++;
            }
        }

        return count;
    }

    private static float CountAllUnaccountedTrumpCards(RelativeCard[] effectiveAccountedFor)
    {
        float count = 0f;

        for (int i = 0; i < TrumpRanks.Length; i++)
        {
            if (!IsAccountedFor(TrumpRanks[i], RelativeSuit.Trump, effectiveAccountedFor))
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsAccountedFor(Rank rank, RelativeSuit suit, RelativeCard[] effectiveAccountedFor)
    {
        for (int i = 0; i < effectiveAccountedFor.Length; i++)
        {
            if (effectiveAccountedFor[i].Rank == rank && effectiveAccountedFor[i].Suit == suit)
            {
                return true;
            }
        }

        return false;
    }

    private static float GetCardThreatsThisTrick(
        RelativeCard[] cardsInHand,
        int index,
        RelativeCard[] effectiveAccountedFor,
        RelativePlayerSuitVoid[] knownPlayerSuitVoids,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCards,
        RelativePlayerPosition leadPlayer,
        RelativeSuit? leadSuit,
        RelativePlayerPosition? winningTrickPlayer,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone)
    {
        if (index >= cardsInHand.Length)
        {
            return -1f;
        }

        return CalculateThreatsThisTrick(
            cardsInHand[index],
            effectiveAccountedFor,
            knownPlayerSuitVoids,
            playedCards,
            leadPlayer,
            leadSuit,
            winningTrickPlayer,
            callingPlayer,
            callingPlayerGoingAlone);
    }

    private static float CalculateThreatsThisTrick(
        RelativeCard card,
        RelativeCard[] effectiveAccountedFor,
        RelativePlayerSuitVoid[] knownPlayerSuitVoids,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCards,
        RelativePlayerPosition leadPlayer,
        RelativeSuit? leadSuit,
        RelativePlayerPosition? winningTrickPlayer,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone)
    {
        if (playedCards.Count == 0 || !leadSuit.HasValue || !winningTrickPlayer.HasValue)
        {
            return CalculateThreats(card, effectiveAccountedFor, knownPlayerSuitVoids);
        }

        var resolvedLeadSuit = leadSuit.Value;
        var resolvedWinner = winningTrickPlayer.Value;
        var opponentsAfterMe = GetOpponentsAfterMe(leadPlayer, callingPlayer, callingPlayerGoingAlone);

        if (opponentsAfterMe.Length == 0)
        {
            return CalculateLastPlayerThreats(card, effectiveAccountedFor, knownPlayerSuitVoids, playedCards, resolvedWinner);
        }

        bool opponentWinning = resolvedWinner is RelativePlayerPosition.LeftHandOpponent or RelativePlayerPosition.RightHandOpponent;

        if (opponentWinning)
        {
            if (!playedCards.TryGetValue(resolvedWinner, out var winningCardOpp))
            {
                return CalculateThreats(card, effectiveAccountedFor, knownPlayerSuitVoids);
            }

            if (!CardBeatsInTrick(card, winningCardOpp))
            {
                return 25f;
            }

            return CountThreatsFromOpponents(card, resolvedLeadSuit, opponentsAfterMe, effectiveAccountedFor, knownPlayerSuitVoids);
        }

        // Partner winning — count threats against the best card our team can field
        if (!playedCards.TryGetValue(RelativePlayerPosition.Partner, out var partnerCard))
        {
            return CountThreatsFromOpponents(card, resolvedLeadSuit, opponentsAfterMe, effectiveAccountedFor, knownPlayerSuitVoids);
        }

        var teamBestCard = CardBeatsInTrick(card, partnerCard) ? card : partnerCard;
        return CountThreatsFromOpponents(teamBestCard, resolvedLeadSuit, opponentsAfterMe, effectiveAccountedFor, knownPlayerSuitVoids);
    }

    private static float CalculateLastPlayerThreats(
        RelativeCard card,
        RelativeCard[] effectiveAccountedFor,
        RelativePlayerSuitVoid[] knownPlayerSuitVoids,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCards,
        RelativePlayerPosition resolvedWinner)
    {
        bool myTeamWinning = resolvedWinner is RelativePlayerPosition.Self or RelativePlayerPosition.Partner;
        if (myTeamWinning)
        {
            return 0f;
        }

        if (!playedCards.TryGetValue(resolvedWinner, out var winningCard))
        {
            return CalculateThreats(card, effectiveAccountedFor, knownPlayerSuitVoids);
        }

        return CardBeatsInTrick(card, winningCard) ? 0f : 25f;
    }

    private static RelativePlayerPosition[] GetOpponentsAfterMe(
        RelativePlayerPosition leadPlayer,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone)
    {
        RelativePlayerPosition[] opponents = leadPlayer switch
        {
            RelativePlayerPosition.Self => [RelativePlayerPosition.LeftHandOpponent, RelativePlayerPosition.RightHandOpponent],
            RelativePlayerPosition.LeftHandOpponent => [],
            RelativePlayerPosition.Partner => [RelativePlayerPosition.LeftHandOpponent],
            RelativePlayerPosition.RightHandOpponent => [RelativePlayerPosition.LeftHandOpponent],
            _ => [],
        };

        if (!callingPlayerGoingAlone || opponents.Length == 0)
        {
            return opponents;
        }

        var sittingOut = callingPlayer switch
        {
            RelativePlayerPosition.Self => RelativePlayerPosition.Partner,
            RelativePlayerPosition.LeftHandOpponent => RelativePlayerPosition.RightHandOpponent,
            RelativePlayerPosition.Partner => (RelativePlayerPosition?)null,
            RelativePlayerPosition.RightHandOpponent => RelativePlayerPosition.LeftHandOpponent,
            _ => null,
        };

        if (sittingOut == null)
        {
            return opponents;
        }

        return [.. opponents.Where(o => o != sittingOut.Value)];
    }

    private static bool CardBeatsInTrick(RelativeCard challenger, RelativeCard current)
    {
        if (challenger.Suit == RelativeSuit.Trump && current.Suit != RelativeSuit.Trump)
        {
            return true;
        }

        if (challenger.Suit != RelativeSuit.Trump && current.Suit == RelativeSuit.Trump)
        {
            return false;
        }

        if (challenger.Suit == current.Suit)
        {
            return challenger.Rank > current.Rank;
        }

        // Challenger is off-suit (not trump, not same suit as current winner)
        return false;
    }

    private static float CountThreatsFromOpponents(
        RelativeCard card,
        RelativeSuit leadSuit,
        RelativePlayerPosition[] opponentsAfterMe,
        RelativeCard[] effectiveAccountedFor,
        RelativePlayerSuitVoid[] knownPlayerSuitVoids)
    {
        float threats = 0f;

        // Check trump cards that beat our card
        if (card.Suit == RelativeSuit.Trump)
        {
            threats += CountTrickThreatsInSuit(card.Rank, RelativeSuit.Trump, TrumpRanks, true, leadSuit, opponentsAfterMe, effectiveAccountedFor, knownPlayerSuitVoids);
        }
        else
        {
            threats += CountTrickThreatsInSuit(Rank.Nine - 1, RelativeSuit.Trump, TrumpRanks, false, leadSuit, opponentsAfterMe, effectiveAccountedFor, knownPlayerSuitVoids);

            if (card.Suit == leadSuit)
            {
                Rank[] ranksForSuit = leadSuit == RelativeSuit.NonTrumpSameColor
                    ? NonTrumpSameColorRanks
                    : NonTrumpOppositeColorRanks;
                threats += CountTrickThreatsInSuit(card.Rank, leadSuit, ranksForSuit, true, leadSuit, opponentsAfterMe, effectiveAccountedFor, knownPlayerSuitVoids);
            }
        }

        return threats;
    }

    private static float CountTrickThreatsInSuit(
        Rank cardRank,
        RelativeSuit threatSuit,
        Rank[] validRanks,
        bool isSameSuitAsLead,
        RelativeSuit leadSuit,
        RelativePlayerPosition[] opponentsAfterMe,
        RelativeCard[] effectiveAccountedFor,
        RelativePlayerSuitVoid[] knownPlayerSuitVoids)
    {
        float count = 0f;

        for (int i = 0; i < validRanks.Length; i++)
        {
            if (validRanks[i] <= cardRank)
            {
                continue;
            }

            if (IsAccountedFor(validRanks[i], threatSuit, effectiveAccountedFor))
            {
                continue;
            }

            if (isSameSuitAsLead || threatSuit == RelativeSuit.Trump)
            {
                bool anyOpponentCanPlay = false;
                for (int j = 0; j < opponentsAfterMe.Length; j++)
                {
                    if (CanOpponentPlaySuit(opponentsAfterMe[j], threatSuit, leadSuit, knownPlayerSuitVoids))
                    {
                        anyOpponentCanPlay = true;
                        break;
                    }
                }

                if (anyOpponentCanPlay)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static bool CanOpponentPlaySuit(
        RelativePlayerPosition opponent,
        RelativeSuit cardSuit,
        RelativeSuit leadSuit,
        RelativePlayerSuitVoid[] knownPlayerSuitVoids)
    {
        if (cardSuit == leadSuit)
        {
            return !IsVoidIn(opponent, cardSuit, knownPlayerSuitVoids);
        }

        // Trump when led suit is not trump — opponent must be void in led suit and not void in trump
        if (cardSuit == RelativeSuit.Trump)
        {
            return IsVoidIn(opponent, leadSuit, knownPlayerSuitVoids) && !IsVoidIn(opponent, RelativeSuit.Trump, knownPlayerSuitVoids);
        }

        return false;
    }

    private static bool IsVoidIn(
        RelativePlayerPosition player,
        RelativeSuit suit,
        RelativePlayerSuitVoid[] knownPlayerSuitVoids)
    {
        for (int i = 0; i < knownPlayerSuitVoids.Length; i++)
        {
            if (knownPlayerSuitVoids[i].PlayerPosition == player && knownPlayerSuitVoids[i].Suit == suit)
            {
                return true;
            }
        }

        return false;
    }
}
