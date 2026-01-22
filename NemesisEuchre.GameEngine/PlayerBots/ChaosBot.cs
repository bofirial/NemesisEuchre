using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.PlayerBots;

public class ChaosBot : IPlayerBot
{
    private readonly Random _random = new();

    public BotType BotType => BotType.Chaos;

    public Task<CallTrumpDecision> CallTrumpAsync(Card[] cardsInHand, Card upCard, RelativePlayerPosition dealerPosition, short teamScore, short opponentScore, CallTrumpDecision[] validCallTrumpDecisions)
    {
        return SelectRandomAsync(validCallTrumpDecisions);
    }

    public Task<RelativeCard> DiscardCardAsync(RelativeCard[] cardsInHand, RelativeDeal? currentDeal, short teamScore, short opponentScore, RelativeCard[] validCardsToDiscard)
    {
        return SelectRandomAsync(validCardsToDiscard);
    }

    public Task<RelativeCard> PlayCardAsync(RelativeCard[] cardsInHand, RelativeDeal? currentDeal, short teamScore, short opponentScore, RelativeCard[] validCardsToPlay)
    {
        return SelectRandomAsync(validCardsToPlay);
    }

    private Task<T> SelectRandomAsync<T>(T[] options)
    {
        return options.Length == 0
            ? throw new ArgumentException("Cannot select from empty array", nameof(options))
            : Task.FromResult(options[_random.Next(options.Length)]);
    }
}
