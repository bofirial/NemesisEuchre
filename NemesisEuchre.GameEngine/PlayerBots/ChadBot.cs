using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.PlayerBots;

public class ChadBot : IPlayerActor
{
    private readonly Random _random = new();

    public ActorType ActorType => ActorType.Chad;

    public Task<CallTrumpDecision> CallTrumpAsync(Card[] cardsInHand, Card upCard, RelativePlayerPosition dealerPosition, short teamScore, short opponentScore, CallTrumpDecision[] validCallTrumpDecisions)
    {
        return validCallTrumpDecisions.Contains(CallTrumpDecision.OrderItUpAndGoAlone)
            ? Task.FromResult(CallTrumpDecision.OrderItUpAndGoAlone)
            : SelectRandomAsync(validCallTrumpDecisions);
    }

    public Task<RelativeCard> DiscardCardAsync(RelativeCard[] cardsInHand, RelativeDeal? currentDeal, short teamScore, short opponentScore, RelativeCard[] validCardsToDiscard)
    {
        var nonTrumpCards = validCardsToDiscard
            .Where(card => card.Suit != RelativeSuit.Trump)
            .ToArray();

        return nonTrumpCards.Length > 0
            ? Task.FromResult(nonTrumpCards.OrderBy(card => card.Rank).First())
            : Task.FromResult(validCardsToDiscard.OrderBy(card => card.Rank).First());
    }

    public Task<RelativeCard> PlayCardAsync(RelativeCard[] cardsInHand, RelativeDeal? currentDeal, short teamScore, short opponentScore, RelativeCard[] validCardsToPlay)
    {
        var trumpCards = validCardsToPlay
            .Where(card => card.Suit == RelativeSuit.Trump)
            .ToArray();

        return trumpCards.Length > 0
            ? Task.FromResult(trumpCards.OrderByDescending(card => card.Rank).First())
            : Task.FromResult(validCardsToPlay.OrderByDescending(card => card.Rank).First());
    }

    private Task<T> SelectRandomAsync<T>(T[] options)
    {
        return options.Length == 0
            ? throw new ArgumentException("Cannot select from empty array", nameof(options))
            : Task.FromResult(options[_random.Next(options.Length)]);
    }
}
