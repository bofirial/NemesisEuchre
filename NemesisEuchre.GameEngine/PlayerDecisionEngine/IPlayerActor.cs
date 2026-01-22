using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public interface IPlayerActor
{
    Task<CallTrumpDecision> CallTrumpAsync(Card[] cardsInHand, Card upCard, RelativePlayerPosition dealerPosition, short teamScore, short opponentScore, CallTrumpDecision[] validCallTrumpDecisions);

    Task<RelativeCard> DiscardCardAsync(RelativeCard[] cardsInHand, RelativeDeal? currentDeal, short teamScore, short opponentScore, RelativeCard[] validCardsToDiscard);

    Task<RelativeCard> PlayCardAsync(RelativeCard[] cardsInHand, RelativeDeal? currentDeal, short teamScore, short opponentScore, RelativeCard[] validCardsToPlay);
}
