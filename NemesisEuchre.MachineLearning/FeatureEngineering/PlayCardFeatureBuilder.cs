using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Pooling;
using NemesisEuchre.MachineLearning.Constants;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public static class PlayCardFeatureBuilder
{
    private const int MaxCardsInHand = FeatureEngineeringConstants.MaxCardsInHand;

    public static PlayCardTrainingData BuildFeatures(
        RelativeCard[] cardsInHand,
        RelativeCard[] validCards,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCards,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition leadPlayer,
        RelativeSuit? leadSuit,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativePlayerPosition dealer,
        RelativeCard? dealerPickedUpCard,
        RelativePlayerSuitVoid[] knownPlayerSuitVoids,
        RelativeCard[] cardsAccountedFor,
        RelativePlayerPosition? winningTrickPlayer,
        short trickNumber,
        short wonTricks,
        short opponentsWonTricks,
        RelativeCard chosenCard)
    {
        var validityArray = GameEnginePoolManager.RentFloatArray(MaxCardsInHand);
        try
        {
            Array.Clear(validityArray, 0, MaxCardsInHand);

            foreach (var validCard in validCards)
            {
                var index = Array.FindIndex(cardsInHand, c => c == validCard);
                if (index is >= 0 and < MaxCardsInHand)
                {
                    validityArray[index] = 1.0f;
                }
            }

            playedCards.TryGetValue(RelativePlayerPosition.LeftHandOpponent, out RelativeCard? leftHandOpponentPlayedCard);
            playedCards.TryGetValue(RelativePlayerPosition.Partner, out RelativeCard? partnerPlayedCard);
            playedCards.TryGetValue(RelativePlayerPosition.RightHandOpponent, out RelativeCard? rightHandOpponentPlayedCard);

            var chosenCardIndex = Array.FindIndex(cardsInHand, c => c == chosenCard);

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
                Card1IsValid = validityArray[0],
                Card2IsValid = validityArray[1],
                Card3IsValid = validityArray[2],
                Card4IsValid = validityArray[3],
                Card5IsValid = validityArray[4],
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
                Card1Chosen = chosenCardIndex == 0 ? 1.0f : 0.0f,
                Card2Chosen = chosenCardIndex == 1 ? 1.0f : 0.0f,
                Card3Chosen = chosenCardIndex == 2 ? 1.0f : 0.0f,
                Card4Chosen = chosenCardIndex == 3 ? 1.0f : 0.0f,
                Card5Chosen = chosenCardIndex == 4 ? 1.0f : 0.0f,
            };
        }
        finally
        {
            GameEnginePoolManager.ReturnFloatArray(validityArray);
        }
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
