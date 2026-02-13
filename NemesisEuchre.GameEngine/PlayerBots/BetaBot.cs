using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;

namespace NemesisEuchre.GameEngine.PlayerBots;

public class BetaBot(IRandomNumberGenerator random) : BotBase(random)
{
    public override ActorType ActorType => ActorType.Beta;

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
                ? CallTrumpDecision.Pass
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
        return Task.FromResult(new RelativeCardDecisionContext()
        {
            ChosenCard = SelectLowestNonTrumpCardOrLowest(validCardsToDiscard),
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
        return Task.FromResult(new RelativeCardDecisionContext()
        {
            ChosenCard = SelectLowestNonTrumpCardOrLowest(validCardsToPlay),
            DecisionPredictedPoints = validCardsToPlay.ToDictionary(d => d, _ => 0f),
        });
    }

    private static RelativeCard SelectLowestNonTrumpCardOrLowest(RelativeCard[] cards)
    {
        var nonTrumpCards = cards
            .Where(card => card.Suit != RelativeSuit.Trump)
            .ToArray();

        return nonTrumpCards.Length > 0
            ? SelectLowestCard(nonTrumpCards)
            : SelectLowestCard(cards);
    }

    private static RelativeCard SelectLowestCard(RelativeCard[] cards)
    {
        return cards.OrderBy(card => card.Rank).First();
    }
}
