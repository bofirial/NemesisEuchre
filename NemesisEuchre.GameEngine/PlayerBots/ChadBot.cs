using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.PlayerBots;

public class ChadBot : IPlayerBot
{
    private readonly Random _random = new();

    public BotType BotType => BotType.Chad;

    public CallTrumpDecision CallTrump(List<Card> cardsInHand, Card upCard, RelativePlayerPosition dealerPosition, int teamScore, int opponentScore, CallTrumpDecision[] validCallTrumpDecisions)
    {
        return validCallTrumpDecisions.Contains(CallTrumpDecision.OrderItUpAndGoAlone)
            ? CallTrumpDecision.OrderItUpAndGoAlone
            : SelectRandom(validCallTrumpDecisions);
    }

    public RelativeCard DiscardCard(List<RelativeCard> cardsInHand, RelativeDeal? currentDeal, int teamScore, int opponentScore, RelativeCard[] validCardsToDiscard)
    {
        var nonTrumpCards = validCardsToDiscard
            .Where(card => card.Suit != RelativeSuit.Trump)
            .ToArray();

        return nonTrumpCards.Length > 0
            ? nonTrumpCards.OrderBy(card => card.Rank).First()
            : validCardsToDiscard.OrderBy(card => card.Rank).First();
    }

    public RelativeCard PlayCard(List<RelativeCard> cardsInHand, RelativeDeal? currentDeal, int teamScore, int opponentScore, RelativeCard[] validCardsToPlay)
    {
        var trumpCards = validCardsToPlay
            .Where(card => card.Suit == RelativeSuit.Trump)
            .ToArray();

        return trumpCards.Length > 0
            ? trumpCards.OrderByDescending(card => card.Rank).First()
            : validCardsToPlay.OrderByDescending(card => card.Rank).First();
    }

    private T SelectRandom<T>(T[] options)
    {
        return options.Length == 0
            ? throw new ArgumentException("Cannot select from empty array", nameof(options))
            : options[_random.Next(options.Length)];
    }
}
