using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public interface IPlayerActor
{
    ActorType ActorType { get; }

    Task<CallTrumpDecision> CallTrumpAsync(Card[] cardsInHand, Card upCard, PlayerPosition playerPosition, RelativePlayerPosition dealerPosition, short teamScore, short opponentScore, CallTrumpDecision[] validCallTrumpDecisions);

    Task<Card> DiscardCardAsync(Card[] cardsInHand, Deal? currentDeal, PlayerPosition playerPosition, short teamScore, short opponentScore, Card[] validCardsToDiscard);

    Task<Card> PlayCardAsync(Card[] cardsInHand, Deal? currentDeal, PlayerPosition playerPosition, short teamScore, short opponentScore, Card[] validCardsToPlay);
}
