using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public interface IPlayerBot
{
    CallTrumpDecision CallTrump(PlayerGameState playerGameState, Card upCard, PlayerPosition dealerPosition, CallTrumpDecision[] validCallTrumpDecisions);

    Card PlayCard(PlayerGameState playerGameState, Card[] validCardsToPlay);
}
