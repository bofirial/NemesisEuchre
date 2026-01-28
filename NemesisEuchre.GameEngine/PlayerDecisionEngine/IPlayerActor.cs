using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public interface IPlayerActor
{
    ActorType ActorType { get; }

    Task<CallTrumpDecision> CallTrumpAsync(Card[] cardsInHand, PlayerPosition playerPosition, short teamScore, short opponentScore, PlayerPosition dealerPosition, Card upCard, CallTrumpDecision[] validCallTrumpDecisions);

    Task<Card> DiscardCardAsync(Card[] cardsInHand, PlayerPosition playerPosition, short teamScore, short opponentScore, Suit trumpSuit, PlayerPosition callingPlayer, bool callingPlayerGoingAlone, Card[] validCardsToDiscard);

    Task<Card> PlayCardAsync(Card[] cardsInHand, PlayerPosition playerPosition, short teamScore, short opponentScore, Suit trumpSuit, PlayerPosition leadPlayer, Suit? leadSuit, Dictionary<PlayerPosition, Card> playedCards, PlayerPosition? winningTrickPlayer, Card[] validCardsToPlay);
}
