using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public interface ISimplePlayCardInferenceFeatureBuilder
{
    SimplePlayCardTrainingData BuildFeatures(
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
        RelativePlayerSuitVoid[] knownPlayerSuitVoids,
        RelativeCard[] cardsAccountedFor,
        RelativePlayerPosition? winningTrickPlayer,
        short trickNumber,
        short wonTricks,
        short opponentsWonTricks,
        RelativeCard chosenCard);
}

public class SimplePlayCardInferenceFeatureBuilder : ISimplePlayCardInferenceFeatureBuilder
{
    public SimplePlayCardTrainingData BuildFeatures(
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
        RelativePlayerSuitVoid[] knownPlayerSuitVoids,
        RelativeCard[] cardsAccountedFor,
        RelativePlayerPosition? winningTrickPlayer,
        short trickNumber,
        short wonTricks,
        short opponentsWonTricks,
        RelativeCard chosenCard)
    {
        var context = new PlayCardFeatureBuilderContext(
            cardsInHand,
            playedCards,
            teamScore,
            opponentScore,
            leadPlayer,
            leadSuit,
            callingPlayer,
            callingPlayerGoingAlone,
            dealer,
            dealerPickedUpCard,
            knownPlayerSuitVoids,
            cardsAccountedFor,
            winningTrickPlayer,
            trickNumber,
            wonTricks,
            opponentsWonTricks,
            chosenCard);

        var full = PlayCardFeatureBuilder.BuildFeatures(context);

        return new SimplePlayCardTrainingData
        {
            Card1Rank = full.Card1Rank,
            Card1Suit = full.Card1Suit,
            Card2Rank = full.Card2Rank,
            Card2Suit = full.Card2Suit,
            Card3Rank = full.Card3Rank,
            Card3Suit = full.Card3Suit,
            Card4Rank = full.Card4Rank,
            Card4Suit = full.Card4Suit,
            Card5Rank = full.Card5Rank,
            Card5Suit = full.Card5Suit,
            LeadPlayer = full.LeadPlayer,
            LeadSuit = full.LeadSuit,
            LeftHandOpponentPlayedCardRank = full.LeftHandOpponentPlayedCardRank,
            LeftHandOpponentPlayedCardSuit = full.LeftHandOpponentPlayedCardSuit,
            PartnerPlayedCardRank = full.PartnerPlayedCardRank,
            PartnerPlayedCardSuit = full.PartnerPlayedCardSuit,
            RightHandOpponentPlayedCardRank = full.RightHandOpponentPlayedCardRank,
            RightHandOpponentPlayedCardSuit = full.RightHandOpponentPlayedCardSuit,
            TeamScore = full.TeamScore,
            OpponentScore = full.OpponentScore,
            TrickNumber = full.TrickNumber,
            CardsPlayedInTrick = full.CardsPlayedInTrick,
            WinningTrickPlayer = full.WinningTrickPlayer,
            ChosenCardRank = full.ChosenCardRank,
        };
    }
}
