using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public interface IPlayerActor
{
    CallTrumpDecision CallTrump(List<Card> hand, Card upCard, RelativePlayerPosition dealerPosition, int teamScore, int opponentScore, CallTrumpDecision[] validCallTrumpDecisions);

    RelativeCard DiscardCard(PlayerGameState playerGameState, RelativeCard[] validCardsToDiscard);

    RelativeCard PlayCard(PlayerGameState playerGameState, RelativeCard[] validCardsToPlay);
}
