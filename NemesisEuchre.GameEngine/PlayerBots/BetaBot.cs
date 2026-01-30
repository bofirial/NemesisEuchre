using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.PlayerBots;

public class BetaBot : BotBase
{
    public override ActorType ActorType => ActorType.Beta;

    public override Task<CallTrumpDecision> CallTrumpAsync(Card[] cardsInHand, PlayerPosition playerPosition, short teamScore, short opponentScore, PlayerPosition dealerPosition, Card upCard, CallTrumpDecision[] validCallTrumpDecisions)
    {
        return validCallTrumpDecisions.Contains(CallTrumpDecision.Pass)
            ? Task.FromResult(CallTrumpDecision.Pass)
            : SelectRandomAsync(validCallTrumpDecisions);
    }

    public override Task<RelativeCard> DiscardCardAsync(RelativeCard[] cardsInHand, short teamScore, short opponentScore, RelativePlayerPosition callingPlayer, bool callingPlayerGoingAlone, RelativeCard[] validCardsToDiscard)
    {
        return Task.FromResult(SelectLowestNonTrumpCardOrLowest(validCardsToDiscard));
    }

    public override Task<RelativeCard> PlayCardAsync(RelativeCard[] cardsInHand, short teamScore, short opponentScore, RelativePlayerPosition leadPlayer, RelativeSuit? leadSuit, Dictionary<RelativePlayerPosition, RelativeCard> playedCards, RelativePlayerPosition? winningTrickPlayer, RelativeCard[] validCardsToPlay)
    {
        return Task.FromResult(SelectLowestNonTrumpCardOrLowest(validCardsToPlay));
    }

    private static RelativeCard SelectLowestNonTrumpCardOrLowest(RelativeCard[] cards)
    {
        var nonTrumpCards = cards
            .Where(card => card.Suit != RelativeSuit.Trump)
            .ToArray();

        return nonTrumpCards.Length > 0
            ? SelectLowestCard(nonTrumpCards)
            : SelectLowestCard(cards);
    }

    private static RelativeCard SelectLowestCard(RelativeCard[] cards)
    {
        return cards.OrderBy(card => card.Rank).First();
    }
}
