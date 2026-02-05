using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Pooling;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public class PlayCardFeatureEngineer : IFeatureEngineer<PlayCardDecisionEntity, PlayCardTrainingData>
{
    private const int MaxCardsInHand = 5;

    public PlayCardTrainingData Transform(PlayCardDecisionEntity entity)
    {
        var context = PlayCardEntityDeserializer.Deserialize(entity);

        var validityArray = GameEnginePoolManager.RentFloatArray(MaxCardsInHand);
        try
        {
            Array.Clear(validityArray, 0, MaxCardsInHand);

            foreach (var validCard in context.ValidCards)
            {
                var index = Array.FindIndex(context.CardsInHand, c =>
                    c.Rank == validCard.Rank && c.Suit == validCard.Suit);
                if (index != -1)
                {
                    validityArray[index] = 1.0f;
                }
            }

            context.PlayedCards.TryGetValue(RelativePlayerPosition.LeftHandOpponent, out RelativeCard? leftHandOpponentPlayedCard);
            context.PlayedCards.TryGetValue(RelativePlayerPosition.Partner, out RelativeCard? partnerPlayedCard);
            context.PlayedCards.TryGetValue(RelativePlayerPosition.RightHandOpponent, out RelativeCard? rightHandOpponentPlayedCard);

            var chosenCardIndex = Array.FindIndex(context.CardsInHand, c =>
                c.Rank == context.ChosenCard.Rank && c.Suit == context.ChosenCard.Suit);

            if (chosenCardIndex == -1)
            {
                throw new InvalidOperationException(
                    $"Chosen card {context.ChosenCard.Rank} of {context.ChosenCard.Suit} not found in hand");
            }

            return new PlayCardTrainingData
            {
                Card1Rank = context.CardsInHand.Length > 0 ? (float)context.CardsInHand[0].Rank : -1.0f,
                Card1Suit = context.CardsInHand.Length > 0 ? (float)context.CardsInHand[0].Suit : -1.0f,
                Card2Rank = context.CardsInHand.Length > 1 ? (float)context.CardsInHand[1].Rank : -1.0f,
                Card2Suit = context.CardsInHand.Length > 1 ? (float)context.CardsInHand[1].Suit : -1.0f,
                Card3Rank = context.CardsInHand.Length > 2 ? (float)context.CardsInHand[2].Rank : -1.0f,
                Card3Suit = context.CardsInHand.Length > 2 ? (float)context.CardsInHand[2].Suit : -1.0f,
                Card4Rank = context.CardsInHand.Length > 3 ? (float)context.CardsInHand[3].Rank : -1.0f,
                Card4Suit = context.CardsInHand.Length > 3 ? (float)context.CardsInHand[3].Suit : -1.0f,
                Card5Rank = context.CardsInHand.Length > 4 ? (float)context.CardsInHand[4].Rank : -1.0f,
                Card5Suit = context.CardsInHand.Length > 4 ? (float)context.CardsInHand[4].Suit : -1.0f,
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
                CardsPlayedInTrick = context.PlayedCards.Count,
                WinningTrickPlayer = entity.WinningTrickPlayer.HasValue ? (float)entity.WinningTrickPlayer.Value : -1.0f,
                Card1IsValid = validityArray[0],
                Card2IsValid = validityArray[1],
                Card3IsValid = validityArray[2],
                Card4IsValid = validityArray[3],
                Card5IsValid = validityArray[4],
                CallingPlayerPosition = (float)entity.CallingPlayer,
                CallingPlayerGoingAlone = entity.CallingPlayerGoingAlone ? 1.0f : 0.0f,
                DealerPlayerPosition = (float)entity.DealerPosition,
                DealerPickedUpCardRank = context.DealerPickedUpCard != null ? (float)context.DealerPickedUpCard.Rank : -1.0f,
                DealerPickedUpCardSuit = context.DealerPickedUpCard != null ? (float)context.DealerPickedUpCard.Suit : -1.0f,
                LeftHandOpponentMayHaveTrump = context.KnownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.LeftHandOpponent && x.Suit == RelativeSuit.Trump) ? 0.0f : 1.0f,
                LeftHandOpponentMayHaveNonTrumpSameColor = context.KnownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.LeftHandOpponent && x.Suit == RelativeSuit.NonTrumpSameColor) ? 0.0f : 1.0f,
                LeftHandOpponentMayHaveNonTrumpOppositeColor1 = context.KnownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.LeftHandOpponent && x.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 0.0f : 1.0f,
                LeftHandOpponentMayHaveNonTrumpOppositeColor2 = context.KnownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.LeftHandOpponent && x.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 0.0f : 1.0f,
                PartnerMayHaveTrump = context.KnownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.Partner && x.Suit == RelativeSuit.Trump) ? 0.0f : 1.0f,
                PartnerMayHaveNonTrumpSameColor = context.KnownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.Partner && x.Suit == RelativeSuit.NonTrumpSameColor) ? 0.0f : 1.0f,
                PartnerMayHaveNonTrumpOppositeColor1 = context.KnownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.Partner && x.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 0.0f : 1.0f,
                PartnerMayHaveNonTrumpOppositeColor2 = context.KnownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.Partner && x.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 0.0f : 1.0f,
                RightHandOpponentMayHaveTrump = context.KnownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.RightHandOpponent && x.Suit == RelativeSuit.Trump) ? 0.0f : 1.0f,
                RightHandOpponentMayHaveNonTrumpSameColor = context.KnownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.RightHandOpponent && x.Suit == RelativeSuit.NonTrumpSameColor) ? 0.0f : 1.0f,
                RightHandOpponentMayHaveNonTrumpOppositeColor1 = context.KnownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.RightHandOpponent && x.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 0.0f : 1.0f,
                RightHandOpponentMayHaveNonTrumpOppositeColor2 = context.KnownPlayerSuitVoids.Any(x => x.PlayerPosition == RelativePlayerPosition.RightHandOpponent && x.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 0.0f : 1.0f,
                RightBowerOfTrumpHasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.RightBower && card.Suit == RelativeSuit.Trump) ? 1.0f : 0.0f,
                LeftBowerOfTrumpHasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.LeftBower && card.Suit == RelativeSuit.Trump) ? 1.0f : 0.0f,
                AceOfTrumpHasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Ace && card.Suit == RelativeSuit.Trump) ? 1.0f : 0.0f,
                KingOfTrumpHasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.King && card.Suit == RelativeSuit.Trump) ? 1.0f : 0.0f,
                QueenOfTrumpHasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Queen && card.Suit == RelativeSuit.Trump) ? 1.0f : 0.0f,
                TenOfTrumpHasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Ten && card.Suit == RelativeSuit.Trump) ? 1.0f : 0.0f,
                NineOfTrumpHasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Nine && card.Suit == RelativeSuit.Trump) ? 1.0f : 0.0f,
                AceOfNonTrumpSameColorHasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Ace && card.Suit == RelativeSuit.NonTrumpSameColor) ? 1.0f : 0.0f,
                KingOfNonTrumpSameColorHasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.King && card.Suit == RelativeSuit.NonTrumpSameColor) ? 1.0f : 0.0f,
                QueenOfNonTrumpSameColorHasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Queen && card.Suit == RelativeSuit.NonTrumpSameColor) ? 1.0f : 0.0f,
                TenOfNonTrumpSameColorHasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Ten && card.Suit == RelativeSuit.NonTrumpSameColor) ? 1.0f : 0.0f,
                NineOfNonTrumpSameColorHasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Nine && card.Suit == RelativeSuit.NonTrumpSameColor) ? 1.0f : 0.0f,
                AceOfNonTrumpOppositeColor1HasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Ace && card.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 1.0f : 0.0f,
                KingOfNonTrumpOppositeColor1HasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.King && card.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 1.0f : 0.0f,
                QueenOfNonTrumpOppositeColor1HasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Queen && card.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 1.0f : 0.0f,
                JackOfNonTrumpOppositeColor1HasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Jack && card.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 1.0f : 0.0f,
                TenOfNonTrumpOppositeColor1HasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Ten && card.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 1.0f : 0.0f,
                NineOfNonTrumpOppositeColor1HasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Nine && card.Suit == RelativeSuit.NonTrumpOppositeColor1) ? 1.0f : 0.0f,
                AceOfNonTrumpOppositeColor2HasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Ace && card.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 1.0f : 0.0f,
                KingOfNonTrumpOppositeColor2HasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.King && card.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 1.0f : 0.0f,
                QueenOfNonTrumpOppositeColor2HasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Queen && card.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 1.0f : 0.0f,
                JackOfNonTrumpOppositeColor2HasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Jack && card.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 1.0f : 0.0f,
                TenOfNonTrumpOppositeColor2HasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Ten && card.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 1.0f : 0.0f,
                NineOfNonTrumpOppositeColor2HasBeenAccountedFor = context.CardsAccountedFor.Any(card => card.Rank == Rank.Nine && card.Suit == RelativeSuit.NonTrumpOppositeColor2) ? 1.0f : 0.0f,
                Card1Chosen = chosenCardIndex == 0 ? 1.0f : 0.0f,
                Card2Chosen = chosenCardIndex == 1 ? 1.0f : 0.0f,
                Card3Chosen = chosenCardIndex == 2 ? 1.0f : 0.0f,
                Card4Chosen = chosenCardIndex == 3 ? 1.0f : 0.0f,
                Card5Chosen = chosenCardIndex == 4 ? 1.0f : 0.0f,
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
