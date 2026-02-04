using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public interface IPlayerActor
{
    ActorType ActorType { get; }

    Task<CallTrumpDecision> CallTrumpAsync(
        Card[] cardsInHand,
        PlayerPosition playerPosition,
        short teamScore,
        short opponentScore,
        PlayerPosition dealerPosition,
        Card upCard,
        CallTrumpDecision[] validCallTrumpDecisions);

    Task<Card> DiscardCardAsync(
        Card[] cardsInHand,
        PlayerPosition playerPosition,
        short teamScore,
        short opponentScore,
        Suit trumpSuit,
        PlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        Card[] validCardsToDiscard);

    Task<Card> PlayCardAsync(
        Card[] cardsInHand,
        PlayerPosition playerPosition,
        short teamScore,
        short opponentScore,
        Suit trumpSuit,
        PlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        PlayerPosition leadPlayer,
        Suit? leadSuit,
        (PlayerPosition PlayerPosition, Suit Suit)[] knownPlayerSuitVoids,
        Card[] cardsAccountedFor,
        Dictionary<PlayerPosition, Card> playedCardsInTrick,
        PlayerPosition? currentlyWinningTrickPlayer,
        short trickNumber,
        Card[] validCardsToPlay);
}
