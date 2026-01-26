using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.PlayerBots;

/// <summary>
/// Conservative bot that avoids trump and plays lowest cards.
/// Always passes on calling trump when possible.
/// </summary>
public class BetaBot : IPlayerActor
{
    private readonly Random _random = new();

    public ActorType ActorType => ActorType.Beta;

    public Task<CallTrumpDecision> CallTrumpAsync(Card[] cardsInHand, Card upCard, RelativePlayerPosition dealerPosition, short teamScore, short opponentScore, CallTrumpDecision[] validCallTrumpDecisions)
    {
        return validCallTrumpDecisions.Contains(CallTrumpDecision.Pass)
            ? Task.FromResult(CallTrumpDecision.Pass)
            : SelectRandomAsync(validCallTrumpDecisions);
    }

    public Task<RelativeCard> DiscardCardAsync(RelativeCard[] cardsInHand, RelativeDeal? currentDeal, short teamScore, short opponentScore, RelativeCard[] validCardsToDiscard)
    {
        return Task.FromResult(SelectLowestNonTrumpCardOrLowest(validCardsToDiscard));
    }

    public Task<RelativeCard> PlayCardAsync(RelativeCard[] cardsInHand, RelativeDeal? currentDeal, short teamScore, short opponentScore, RelativeCard[] validCardsToPlay)
    {
        return Task.FromResult(SelectLowestNonTrumpCardOrLowest(validCardsToPlay));
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

    private Task<T> SelectRandomAsync<T>(T[] options)
    {
        return options.Length == 0
            ? throw new ArgumentException("Cannot select from empty array", nameof(options))
            : Task.FromResult(options[_random.Next(options.Length)]);
    }
}
