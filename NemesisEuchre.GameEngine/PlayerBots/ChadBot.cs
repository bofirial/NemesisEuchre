using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;

namespace NemesisEuchre.GameEngine.PlayerBots;

public class ChadBot(IRandomNumberGenerator random) : BotBase(random)
{
    public override ActorType ActorType => ActorType.Chad;

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
            ChosenCallTrumpDecision = validCallTrumpDecisions.Contains(CallTrumpDecision.Pass)
                ? CallTrumpDecision.OrderItUpAndGoAlone
                : SelectRandom(validCallTrumpDecisions),
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
        var nonTrumpCards = validCardsToDiscard
            .Where(card => card.Suit != RelativeSuit.Trump)
            .ToArray();

        return Task.FromResult(new RelativeCardDecisionContext()
        {
            ChosenCard = nonTrumpCards.Length > 0
            ? nonTrumpCards.OrderBy(card => card.Rank).First()
            : validCardsToDiscard.OrderBy(card => card.Rank).First(),
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
        short wonTricks,
        short opponentsWonTricks,
        RelativeCard[] validCardsToPlay)
    {
        var trumpCards = validCardsToPlay
            .Where(card => card.Suit == RelativeSuit.Trump)
            .ToArray();

        return Task.FromResult(new RelativeCardDecisionContext()
        {
            ChosenCard = trumpCards.Length > 0
            ? trumpCards.OrderByDescending(card => card.Rank).First()
            : validCardsToPlay.OrderByDescending(card => card.Rank).First(),
            DecisionPredictedPoints = validCardsToPlay.ToDictionary(d => d, _ => 0f),
        });
    }
}
