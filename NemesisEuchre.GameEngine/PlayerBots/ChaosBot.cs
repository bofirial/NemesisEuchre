using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;

namespace NemesisEuchre.GameEngine.PlayerBots;

public class ChaosBot(IRandomNumberGenerator random) : BotBase(random)
{
    public override ActorType ActorType => ActorType.Chaos;

    public override Task<CallTrumpDecision> CallTrumpAsync(
        Card[] cardsInHand,
        PlayerPosition playerPosition,
        short teamScore,
        short opponentScore,
        PlayerPosition dealerPosition,
        Card upCard,
        CallTrumpDecision[] validCallTrumpDecisions)
    {
        return SelectRandomAsync(validCallTrumpDecisions);
    }

    public override Task<RelativeCard> DiscardCardAsync(
        RelativeCard[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativeCard[] validCardsToDiscard)
    {
        return SelectRandomAsync(validCardsToDiscard);
    }

    public override Task<RelativeCard> PlayCardAsync(
        RelativeCard[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativePlayerPosition leadPlayer,
        RelativeSuit? leadSuit,
        (RelativePlayerPosition PlayerPosition, RelativeSuit Suit)[] knownPlayerSuitVoids,
        RelativeCard[] cardsAccountedFor,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCardsInTrick,
        RelativePlayerPosition? currentlyWinningTrickPlayer,
        short trickNumber,
        RelativeCard[] validCardsToPlay)
    {
        return SelectRandomAsync(validCardsToPlay);
    }
}
