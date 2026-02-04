using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public class PlayCardFeatureEngineer : IFeatureEngineer<PlayCardDecisionEntity, PlayCardTrainingData>
{
    private const int MaxCardsInHand = 5;

    public PlayCardTrainingData Transform(PlayCardDecisionEntity entity)
    {
        var cards = JsonDeserializationHelper.DeserializeRelativeCards(entity.CardsInHandJson);
        var playedCards = JsonDeserializationHelper.DeserializePlayedCards(entity.PlayedCardsJson);

        var validCards = JsonDeserializationHelper.DeserializeRelativeCards(entity.ValidCardsToPlayJson);

        var validityArray = new float[MaxCardsInHand];
        foreach (var validCard in validCards)
        {
            var index = Array.FindIndex(cards, c =>
                c.Rank == validCard.Rank && c.Suit == validCard.Suit);
            if (index != -1)
            {
                validityArray[index] = 1.0f;
            }
        }

        playedCards.TryGetValue(RelativePlayerPosition.LeftHandOpponent, out RelativeCard? leftHandOpponentPlayedCard);
        playedCards.TryGetValue(RelativePlayerPosition.Partner, out RelativeCard? partnerPlayedCard);
        playedCards.TryGetValue(RelativePlayerPosition.RightHandOpponent, out RelativeCard? rightHandOpponentPlayedCard);

        var dealerPickedUpCard = !string.IsNullOrEmpty(entity.DealerPickedUpCardJson) ? JsonDeserializationHelper.DeserializeRelativeCard(entity.DealerPickedUpCardJson) : null;

        var knownPlayerSuitVoids = JsonDeserializationHelper.DeserializeKnownPlayerVoids(entity.KnownPlayerSuitVoidsJson);

        var cardsAccountedFor = JsonDeserializationHelper.DeserializeRelativeCards(entity.CardsAccountedForJson);

        var chosenCard = JsonDeserializationHelper.DeserializeRelativeCard(entity.ChosenCardJson);

        var chosenCardIndex = Array.FindIndex(cards, c =>
            c.Rank == chosenCard.Rank && c.Suit == chosenCard.Suit);

        if (chosenCardIndex == -1)
        {
            throw new InvalidOperationException(
                $"Chosen card {chosenCard.Rank} of {chosenCard.Suit} not found in hand");
        }

        return new PlayCardTrainingData
        {
            Card1Rank = cards.Length > 0 ? (float)cards[0].Rank : -1.0f,
            Card1Suit = cards.Length > 0 ? (float)cards[0].Suit : -1.0f,
            Card2Rank = cards.Length > 1 ? (float)cards[1].Rank : -1.0f,
            Card2Suit = cards.Length > 1 ? (float)cards[1].Suit : -1.0f,
            Card3Rank = cards.Length > 2 ? (float)cards[2].Rank : -1.0f,
            Card3Suit = cards.Length > 2 ? (float)cards[2].Suit : -1.0f,
            Card4Rank = cards.Length > 3 ? (float)cards[3].Rank : -1.0f,
            Card4Suit = cards.Length > 3 ? (float)cards[3].Suit : -1.0f,
            Card5Rank = cards.Length > 4 ? (float)cards[4].Rank : -1.0f,
            Card5Suit = cards.Length > 4 ? (float)cards[4].Suit : -1.0f,
            LeadPlayer = (float)entity.LeadPlayer,
            LeadSuit = entity.LeadSuit.HasValue ? (float)entity.LeadSuit.Value : -1.0f,
            LeftHandOpponentPlayedCardRank = (float?)leftHandOpponentPlayedCard?.Rank ?? -1.0f,
            LeftHandOpponentPlayedCardSuit = (float?)leftHandOpponentPlayedCard?.Suit ?? -1.0f,
            PartnerPlayedCardRank = (float?)partnerPlayedCard?.Rank ?? -1.0f,
            PartnerPlayedCardSuit = (float?)partnerPlayedCard?.Suit ?? -1.0f,
            RightHandOpponentPlayedCardRank = (float?)rightHandOpponentPlayedCard?.Rank ?? -1.0f,
            RightHandOpponentPlayedCardSuit = (float?)rightHandOpponentPlayedCard?.Suit ?? -1.0f,
            TeamScore = entity.TeamScore,
            OpponentScore = entity.OpponentScore,
            TrickNumber = entity.TrickNumber,
            CardsPlayedInTrick = playedCards.Count,
            WinningTrickPlayer = entity.WinningTrickPlayer.HasValue ? (float)entity.WinningTrickPlayer.Value : -1.0f,
            Card1IsValid = validityArray[0],
            Card2IsValid = validityArray[1],
            Card3IsValid = validityArray[2],
            Card4IsValid = validityArray[3],
            Card5IsValid = validityArray[4],
            CallingPlayerPosition = (float)entity.CallingPlayer,
            CallingPlayerGoingAlone = entity.CallingPlayerGoingAlone ? 1.0f : 0.0f,
            DealerPlayerPosition = (float)entity.DealerPosition,
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
            Card1Chosen = chosenCardIndex == 0 ? 1.0f : 0.0f,
            Card2Chosen = chosenCardIndex == 1 ? 1.0f : 0.0f,
            Card3Chosen = chosenCardIndex == 2 ? 1.0f : 0.0f,
            Card4Chosen = chosenCardIndex == 3 ? 1.0f : 0.0f,
            Card5Chosen = chosenCardIndex == 4 ? 1.0f : 0.0f,
            ExpectedDealPoints = entity.RelativeDealPoints ?? throw new InvalidOperationException(
                "RelativeDealPoints is required for regression training"),
        };
    }
}
