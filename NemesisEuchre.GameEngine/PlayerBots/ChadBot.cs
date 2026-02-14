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
        var chosenDecision = validCallTrumpDecisions.Contains(CallTrumpDecision.Pass)
            ? CallTrumpDecision.OrderItUpAndGoAlone
            : SelectRandom(validCallTrumpDecisions);
        return CreateCallTrumpDecisionAsync(chosenDecision, validCallTrumpDecisions);
    }

    public override Task<RelativeCardDecisionContext> DiscardCardAsync(
        RelativeCard[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativeCard[] validCardsToDiscard)
    {
        var chosenCard = SelectLowestNonTrumpOr(
            validCardsToDiscard,
            cards => cards.OrderBy(c => c.Rank).First());
        return CreateCardDecisionAsync(chosenCard, validCardsToDiscard);
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
        var chosenCard = SelectHighestTrumpOr(
            validCardsToPlay,
            cards => cards.OrderByDescending(c => c.Rank).First());
        return CreateCardDecisionAsync(chosenCard, validCardsToPlay);
    }
}
