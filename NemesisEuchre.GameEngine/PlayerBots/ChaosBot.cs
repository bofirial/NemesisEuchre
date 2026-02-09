using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;

namespace NemesisEuchre.GameEngine.PlayerBots;

public class ChaosBot(IRandomNumberGenerator random) : BotBase(random)
{
    public override ActorType ActorType => ActorType.Chaos;

    public override Task<CallTrumpDecisionContext> CallTrumpAsync(
        Card[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition dealerPosition,
        Card upCard,
        CallTrumpDecision[] validCallTrumpDecisions)
    {
        return Task.FromResult(new CallTrumpDecisionContext()
        {
            ChosenCallTrumpDecision = SelectRandom(validCallTrumpDecisions),
            DecisionPredictedPoints = validCallTrumpDecisions.ToDictionary(d => d, _ => 0f),
        });
    }

    public override Task<RelativeCardDecisionContext> DiscardCardAsync(
        RelativeCard[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativeCard[] validCardsToDiscard)
    {
        return Task.FromResult(new RelativeCardDecisionContext()
        {
            ChosenCard = SelectRandom(validCardsToDiscard),
            DecisionPredictedPoints = validCardsToDiscard.ToDictionary(d => d, _ => 0f),
        });
    }

    public override Task<RelativeCardDecisionContext> PlayCardAsync(
        RelativeCard[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativePlayerPosition dealer,
        RelativeCard? dealerPickedUpCard,
        RelativePlayerPosition leadPlayer,
        RelativeSuit? leadSuit,
        RelativePlayerSuitVoid[] knownPlayerSuitVoids,
        RelativeCard[] cardsAccountedFor,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCardsInTrick,
        RelativePlayerPosition? currentlyWinningTrickPlayer,
        short trickNumber,
        RelativeCard[] validCardsToPlay)
    {
        return Task.FromResult(new RelativeCardDecisionContext()
        {
            ChosenCard = SelectRandom(validCardsToPlay),
            DecisionPredictedPoints = validCardsToPlay.ToDictionary(d => d, _ => 0f),
        });
    }
}
