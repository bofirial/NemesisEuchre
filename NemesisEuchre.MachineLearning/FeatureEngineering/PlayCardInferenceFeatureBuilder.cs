using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public interface IPlayCardInferenceFeatureBuilder
{
    PlayCardTrainingData BuildFeatures(
        RelativeCard[] cardsInHand,
        RelativePlayerPosition leadPlayer,
        RelativeSuit? leadSuit,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCards,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativePlayerPosition dealer,
        RelativeCard? dealerPickedUpCard,
        (RelativePlayerPosition PlayerPosition, RelativeSuit Suit)[] knownPlayerSuitVoids,
        RelativeCard[] cardsAccountedFor,
        RelativePlayerPosition? winningTrickPlayer,
        short trickNumber,
        RelativeCard[] validCardsToPlay,
        RelativeCard chosenCard);
}

public class PlayCardInferenceFeatureBuilder : IPlayCardInferenceFeatureBuilder
{
    public PlayCardTrainingData BuildFeatures(
        RelativeCard[] cardsInHand,
        RelativePlayerPosition leadPlayer,
        RelativeSuit? leadSuit,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCards,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativePlayerPosition dealer,
        RelativeCard? dealerPickedUpCard,
        (RelativePlayerPosition PlayerPosition, RelativeSuit Suit)[] knownPlayerSuitVoids,
        RelativeCard[] cardsAccountedFor,
        RelativePlayerPosition? winningTrickPlayer,
        short trickNumber,
        RelativeCard[] validCardsToPlay,
        RelativeCard chosenCard)
    {
        var validityArray = new float[5];
        foreach (var card in validCardsToPlay)
        {
            var index = Array.IndexOf(cardsInHand, card);
            if (index is >= 0 and < 5)
            {
                validityArray[index] = 1.0f;
            }
        }

        playedCards.TryGetValue(RelativePlayerPosition.LeftHandOpponent, out RelativeCard? leftHandOpponentPlayedCard);
        playedCards.TryGetValue(RelativePlayerPosition.Partner, out RelativeCard? partnerPlayedCard);
        playedCards.TryGetValue(RelativePlayerPosition.RightHandOpponent, out RelativeCard? rightHandOpponentPlayedCard);

        var chosenIndex = Array.IndexOf(cardsInHand, chosenCard);

        return new PlayCardTrainingData
        {
            Card1Rank = cardsInHand.Length > 0 ? (float)cardsInHand[0].Rank : -1.0f,
            Card1Suit = cardsInHand.Length > 0 ? (float)cardsInHand[0].Suit : -1.0f,
            Card2Rank = cardsInHand.Length > 1 ? (float)cardsInHand[1].Rank : -1.0f,
            Card2Suit = cardsInHand.Length > 1 ? (float)cardsInHand[1].Suit : -1.0f,
            Card3Rank = cardsInHand.Length > 2 ? (float)cardsInHand[2].Rank : -1.0f,
            Card3Suit = cardsInHand.Length > 2 ? (float)cardsInHand[2].Suit : -1.0f,
            Card4Rank = cardsInHand.Length > 3 ? (float)cardsInHand[3].Rank : -1.0f,
            Card4Suit = cardsInHand.Length > 3 ? (float)cardsInHand[3].Suit : -1.0f,
            Card5Rank = cardsInHand.Length > 4 ? (float)cardsInHand[4].Rank : -1.0f,
            Card5Suit = cardsInHand.Length > 4 ? (float)cardsInHand[4].Suit : -1.0f,
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
            LeftHandOpponentMayHaveTrump = knownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.LeftHandOpponent && x.Suit == RelativeSuit.Trump) ? 0.0f : 1.0f,
            LeftHandOpponentMayHaveNonTrumpSameColor = knownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.LeftHandOpponent && x.Suit == RelativeSuit.NonTrumpSameColor) ? 0.0f : 1.0f,
            LeftHandOpponentMayHaveNonTrumpOppositeColor1 = knownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.LeftHandOpponent && x.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 0.0f : 1.0f,
            LeftHandOpponentMayHaveNonTrumpOppositeColor2 = knownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.LeftHandOpponent && x.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 0.0f : 1.0f,
            PartnerMayHaveTrump = knownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.Partner && x.Suit == RelativeSuit.Trump) ? 0.0f : 1.0f,
            PartnerMayHaveNonTrumpSameColor = knownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.Partner && x.Suit == RelativeSuit.NonTrumpSameColor) ? 0.0f : 1.0f,
            PartnerMayHaveNonTrumpOppositeColor1 = knownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.Partner && x.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 0.0f : 1.0f,
            PartnerMayHaveNonTrumpOppositeColor2 = knownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.Partner && x.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 0.0f : 1.0f,
            RightHandOpponentMayHaveTrump = knownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.RightHandOpponent && x.Suit == RelativeSuit.Trump) ? 0.0f : 1.0f,
            RightHandOpponentMayHaveNonTrumpSameColor = knownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.RightHandOpponent && x.Suit == RelativeSuit.NonTrumpSameColor) ? 0.0f : 1.0f,
            RightHandOpponentMayHaveNonTrumpOppositeColor1 = knownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.RightHandOpponent && x.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 0.0f : 1.0f,
            RightHandOpponentMayHaveNonTrumpOppositeColor2 = knownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.RightHandOpponent && x.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 0.0f : 1.0f,
            RightBowerOfTrumpHasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.RightBower && card.Suit == RelativeSuit.Trump) ? 1.0f : 0.0f,
            LeftBowerOfTrumpHasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.LeftBower && card.Suit == RelativeSuit.Trump) ? 1.0f : 0.0f,
            AceOfTrumpHasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Ace && card.Suit == RelativeSuit.Trump) ? 1.0f : 0.0f,
            KingOfTrumpHasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.King && card.Suit == RelativeSuit.Trump) ? 1.0f : 0.0f,
            QueenOfTrumpHasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Queen && card.Suit == RelativeSuit.Trump) ? 1.0f : 0.0f,
            TenOfTrumpHasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Ten && card.Suit == RelativeSuit.Trump) ? 1.0f : 0.0f,
            NineOfTrumpHasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Nine && card.Suit == RelativeSuit.Trump) ? 1.0f : 0.0f,
            AceOfNonTrumpSameColorHasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Ace && card.Suit == RelativeSuit.NonTrumpSameColor) ? 1.0f : 0.0f,
            KingOfNonTrumpSameColorHasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.King && card.Suit == RelativeSuit.NonTrumpSameColor) ? 1.0f : 0.0f,
            QueenOfNonTrumpSameColorHasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Queen && card.Suit == RelativeSuit.NonTrumpSameColor) ? 1.0f : 0.0f,
            TenOfNonTrumpSameColorHasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Ten && card.Suit == RelativeSuit.NonTrumpSameColor) ? 1.0f : 0.0f,
            NineOfNonTrumpSameColorHasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Nine && card.Suit == RelativeSuit.NonTrumpSameColor) ? 1.0f : 0.0f,
            AceOfNonTrumpOppositeColor1HasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Ace && card.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 1.0f : 0.0f,
            KingOfNonTrumpOppositeColor1HasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.King && card.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 1.0f : 0.0f,
            QueenOfNonTrumpOppositeColor1HasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Queen && card.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 1.0f : 0.0f,
            JackOfNonTrumpOppositeColor1HasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Jack && card.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 1.0f : 0.0f,
            TenOfNonTrumpOppositeColor1HasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Ten && card.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 1.0f : 0.0f,
            NineOfNonTrumpOppositeColor1HasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Nine && card.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 1.0f : 0.0f,
            AceOfNonTrumpOppositeColor2HasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Ace && card.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 1.0f : 0.0f,
            KingOfNonTrumpOppositeColor2HasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.King && card.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 1.0f : 0.0f,
            QueenOfNonTrumpOppositeColor2HasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Queen && card.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 1.0f : 0.0f,
            JackOfNonTrumpOppositeColor2HasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Jack && card.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 1.0f : 0.0f,
            TenOfNonTrumpOppositeColor2HasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Ten && card.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 1.0f : 0.0f,
            NineOfNonTrumpOppositeColor2HasBeenAccountedFor = cardsAccountedFor.Any(card => card.Rank == Rank.Nine && card.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 1.0f : 0.0f,
            Card1Chosen = chosenIndex == 0 ? 1.0f : 0.0f,
            Card2Chosen = chosenIndex == 1 ? 1.0f : 0.0f,
            Card3Chosen = chosenIndex == 2 ? 1.0f : 0.0f,
            Card4Chosen = chosenIndex == 3 ? 1.0f : 0.0f,
            Card5Chosen = chosenIndex == 4 ? 1.0f : 0.0f,
        };
    }
}
