using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.PlayerBots;

public class ChaosBot : IPlayerBot
{
    private readonly Random _random = new();

    public BotType BotType => BotType.Chaos;

    public CallTrumpDecision CallTrump(
        List<Card> cardsInHand,
        Card upCard,
        RelativePlayerPosition dealerPosition,
        int teamScore,
        int opponentScore,
        CallTrumpDecision[] validCallTrumpDecisions)
    {
        return SelectRandom(validCallTrumpDecisions);
    }

    public RelativeCard DiscardCard(
        List<RelativeCard> cardsInHand,
        RelativeDeal? currentDeal,
        int teamScore,
        int opponentScore,
        RelativeCard[] validCardsToDiscard)
    {
        return SelectRandom(validCardsToDiscard);
    }

    public RelativeCard PlayCard(
        List<RelativeCard> cardsInHand,
        RelativeDeal? currentDeal,
        int teamScore,
        int opponentScore,
        RelativeCard[] validCardsToPlay)
    {
        return SelectRandom(validCardsToPlay);
    }

    private T SelectRandom<T>(T[] options)
    {
        return options.Length == 0
            ? throw new ArgumentException("Cannot select from empty array", nameof(options))
            : options[_random.Next(options.Length)];
    }
}
