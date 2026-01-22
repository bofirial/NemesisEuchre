using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.PlayerBots;

public class BetaBot : IPlayerBot
{
    private readonly Random _random = new();

    public BotType BotType => BotType.Chad;

    public CallTrumpDecision CallTrump(List<Card> cardsInHand, Card upCard, RelativePlayerPosition dealerPosition, short teamScore, short opponentScore, CallTrumpDecision[] validCallTrumpDecisions)
    {
        return validCallTrumpDecisions.Contains(CallTrumpDecision.Pass)
            ? CallTrumpDecision.OrderItUpAndGoAlone
            : SelectRandom(validCallTrumpDecisions);
    }

    public RelativeCard DiscardCard(List<RelativeCard> cardsInHand, RelativeDeal? currentDeal, short teamScore, short opponentScore, RelativeCard[] validCardsToDiscard)
    {
        var nonTrumpCards = validCardsToDiscard
            .Where(card => card.Suit != RelativeSuit.Trump)
            .ToArray();

        return nonTrumpCards.Length > 0
            ? nonTrumpCards.OrderBy(card => card.Rank).First()
            : validCardsToDiscard.OrderBy(card => card.Rank).First();
    }

    public RelativeCard PlayCard(List<RelativeCard> cardsInHand, RelativeDeal? currentDeal, short teamScore, short opponentScore, RelativeCard[] validCardsToPlay)
    {
        var nonTrumpCards = validCardsToPlay
            .Where(card => card.Suit != RelativeSuit.Trump)
            .ToArray();

        return nonTrumpCards.Length > 0
            ? nonTrumpCards.OrderBy(card => card.Rank).First()
            : validCardsToPlay.OrderBy(card => card.Rank).First();
    }

    private T SelectRandom<T>(T[] options)
    {
        return options.Length == 0
            ? throw new ArgumentException("Cannot select from empty array", nameof(options))
            : options[_random.Next(options.Length)];
    }
}
