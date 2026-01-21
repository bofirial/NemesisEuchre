using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public interface IPlayerActor
{
    CallTrumpDecision CallTrump(List<Card> cardsInHand, Card upCard, RelativePlayerPosition dealerPosition, int teamScore, int opponentScore, CallTrumpDecision[] validCallTrumpDecisions);

    RelativeCard DiscardCard(List<RelativeCard> cardsInHand, RelativeDeal? currentDeal, int teamScore, int opponentScore, RelativeCard[] validCardsToDiscard);

    RelativeCard PlayCard(List<RelativeCard> cardsInHand, RelativeDeal? currentDeal, int teamScore, int opponentScore, RelativeCard[] validCardsToPlay);
}
