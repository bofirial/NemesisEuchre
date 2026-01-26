using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.PlayerBots;

public abstract class BotBase : IPlayerActor
{
    private readonly Random _random = new();

    public abstract ActorType ActorType { get; }

    public abstract Task<CallTrumpDecision> CallTrumpAsync(Card[] cardsInHand, Card upCard, PlayerPosition playerPosition, RelativePlayerPosition dealerPosition, short teamScore, short opponentScore, CallTrumpDecision[] validCallTrumpDecisions);

    public abstract Task<RelativeCard> DiscardCardAsync(RelativeCard[] cardsInHand, RelativeDeal? currentDeal, short teamScore, short opponentScore, RelativeCard[] validCardsToDiscard);

    public abstract Task<RelativeCard> PlayCardAsync(RelativeCard[] cardsInHand, RelativeDeal? currentDeal, short teamScore, short opponentScore, RelativeCard[] validCardsToPlay);

    public async Task<Card> DiscardCardAsync(Card[] cardsInHand, Deal? currentDeal, PlayerPosition playerPosition, short teamScore, short opponentScore, Card[] validCardsToDiscard)
    {
        if (currentDeal?.Trump == null)
        {
            throw new InvalidOperationException("Trump must be set before discarding cards");
        }

        var relativeHand = cardsInHand.Select(c => c.ToRelative(currentDeal.Trump.Value)).ToArray();
        var relativeDeal = currentDeal.ToRelative(playerPosition);
        var relativeValidCards = validCardsToDiscard.Select(c => c.ToRelative(currentDeal.Trump.Value)).ToArray();

        var relativeChoice = await DiscardCardAsync(relativeHand, relativeDeal, teamScore, opponentScore, relativeValidCards);
        return relativeChoice.Card;
    }

    public async Task<Card> PlayCardAsync(Card[] cardsInHand, Deal? currentDeal, PlayerPosition playerPosition, short teamScore, short opponentScore, Card[] validCardsToPlay)
    {
        if (currentDeal?.Trump == null)
        {
            throw new InvalidOperationException("Trump must be set before playing cards");
        }

        var relativeHand = cardsInHand.Select(c => c.ToRelative(currentDeal.Trump.Value)).ToArray();
        var relativeDeal = currentDeal.ToRelative(playerPosition);
        var relativeValidCards = validCardsToPlay.Select(c => c.ToRelative(currentDeal.Trump.Value)).ToArray();

        var relativeChoice = await PlayCardAsync(relativeHand, relativeDeal, teamScore, opponentScore, relativeValidCards);
        return relativeChoice.Card;
    }

    protected Task<T> SelectRandomAsync<T>(T[] options)
    {
        return options.Length == 0
            ? throw new ArgumentException("Cannot select from empty array", nameof(options))
            : Task.FromResult(options[_random.Next(options.Length)]);
    }
}
