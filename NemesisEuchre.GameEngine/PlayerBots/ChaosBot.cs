using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.PlayerBots;

public class ChaosBot : BotBase
{
    public override ActorType ActorType => ActorType.Chaos;

    public override Task<CallTrumpDecision> CallTrumpAsync(Card[] cardsInHand, Card upCard, PlayerPosition playerPosition, RelativePlayerPosition dealerPosition, short teamScore, short opponentScore, CallTrumpDecision[] validCallTrumpDecisions)
    {
        return SelectRandomAsync(validCallTrumpDecisions);
    }

    public override Task<RelativeCard> DiscardCardAsync(RelativeCard[] cardsInHand, RelativeDeal? currentDeal, short teamScore, short opponentScore, RelativeCard[] validCardsToDiscard)
    {
        return SelectRandomAsync(validCardsToDiscard);
    }

    public override Task<RelativeCard> PlayCardAsync(RelativeCard[] cardsInHand, RelativeDeal? currentDeal, short teamScore, short opponentScore, RelativeCard[] validCardsToPlay)
    {
        return SelectRandomAsync(validCardsToPlay);
    }
}
