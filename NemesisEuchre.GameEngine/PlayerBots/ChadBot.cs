using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.PlayerBots;

public class ChadBot : BotBase
{
    public override ActorType ActorType => ActorType.Chad;

    public override Task<CallTrumpDecision> CallTrumpAsync(Card[] cardsInHand, PlayerPosition playerPosition, short teamScore, short opponentScore, PlayerPosition dealerPosition, Card upCard, CallTrumpDecision[] validCallTrumpDecisions)
    {
        return validCallTrumpDecisions.Contains(CallTrumpDecision.OrderItUpAndGoAlone)
            ? Task.FromResult(CallTrumpDecision.OrderItUpAndGoAlone)
            : SelectRandomAsync(validCallTrumpDecisions);
    }

    public override Task<RelativeCard> DiscardCardAsync(RelativeCard[] cardsInHand, short teamScore, short opponentScore, RelativePlayerPosition callingPlayer, bool callingPlayerGoingAlone, RelativeCard[] validCardsToDiscard)
    {
        var nonTrumpCards = validCardsToDiscard
            .Where(card => card.Suit != RelativeSuit.Trump)
            .ToArray();

        return nonTrumpCards.Length > 0
            ? Task.FromResult(nonTrumpCards.OrderBy(card => card.Rank).First())
            : Task.FromResult(validCardsToDiscard.OrderBy(card => card.Rank).First());
    }

    public override Task<RelativeCard> PlayCardAsync(RelativeCard[] cardsInHand, short teamScore, short opponentScore, RelativePlayerPosition callingPlayer, bool callingPlayerGoingAlone, RelativePlayerPosition leadPlayer, RelativeSuit? leadSuit, Dictionary<RelativePlayerPosition, RelativeCard> playedCards, RelativePlayerPosition? winningTrickPlayer, RelativeCard[] validCardsToPlay)
    {
        var trumpCards = validCardsToPlay
            .Where(card => card.Suit == RelativeSuit.Trump)
            .ToArray();

        return trumpCards.Length > 0
            ? Task.FromResult(trumpCards.OrderByDescending(card => card.Rank).First())
            : Task.FromResult(validCardsToPlay.OrderByDescending(card => card.Rank).First());
    }
}
