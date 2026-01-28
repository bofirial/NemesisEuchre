using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.PlayerBots;

public abstract class BotBase : IPlayerActor
{
    private readonly Random _random = new();

    public abstract string ActorType { get; }

    public abstract Task<CallTrumpDecision> CallTrumpAsync(Card[] cardsInHand, PlayerPosition playerPosition, short teamScore, short opponentScore, PlayerPosition dealerPosition, Card upCard, CallTrumpDecision[] validCallTrumpDecisions);

    public abstract Task<RelativeCard> DiscardCardAsync(RelativeCard[] cardsInHand, short teamScore, short opponentScore, RelativePlayerPosition callingPlayer, bool callingPlayerGoingAlone, RelativeCard[] validCardsToDiscard);

    public abstract Task<RelativeCard> PlayCardAsync(RelativeCard[] cardsInHand, short teamScore, short opponentScore, RelativePlayerPosition leadPlayer, RelativeSuit? leadSuit, Dictionary<RelativePlayerPosition, RelativeCard> playedCards, RelativePlayerPosition? winningTrickPlayer, RelativeCard[] validCardsToPlay);

    public async Task<Card> DiscardCardAsync(Card[] cardsInHand, PlayerPosition playerPosition, short teamScore, short opponentScore, Suit trumpSuit, PlayerPosition callingPlayer, bool callingPlayerGoingAlone, Card[] validCardsToDiscard)
    {
        var relativeHand = cardsInHand.Select(c => c.ToRelative(trumpSuit)).ToArray();
        var relativeValidCards = validCardsToDiscard.Select(c => c.ToRelative(trumpSuit)).ToArray();

        var relativeChoice = await DiscardCardAsync(relativeHand, teamScore, opponentScore, callingPlayer.ToRelativePosition(playerPosition), callingPlayerGoingAlone, relativeValidCards);
        return relativeChoice.Card;
    }

    public async Task<Card> PlayCardAsync(Card[] cardsInHand, PlayerPosition playerPosition, short teamScore, short opponentScore, Suit trumpSuit, PlayerPosition leadPlayer, Suit? leadSuit, Dictionary<PlayerPosition, Card> playedCards, PlayerPosition? winningTrickPlayer, Card[] validCardsToPlay)
    {
        var relativeHand = cardsInHand.Select(c => c.ToRelative(trumpSuit)).ToArray();
        var relativeValidCards = validCardsToPlay.Select(c => c.ToRelative(trumpSuit)).ToArray();

        var relativeChoice = await PlayCardAsync(relativeHand, teamScore, opponentScore, leadPlayer.ToRelativePosition(playerPosition), leadSuit?.ToRelativeSuit(trumpSuit), playedCards.ToDictionary(kvp => kvp.Key.ToRelativePosition(playerPosition), kvp => kvp.Value.ToRelative(trumpSuit)), winningTrickPlayer?.ToRelativePosition(playerPosition), relativeValidCards);
        return relativeChoice.Card;
    }

    protected Task<T> SelectRandomAsync<T>(T[] options)
    {
        return options.Length == 0
            ? throw new ArgumentException("Cannot select from empty array", nameof(options))
            : Task.FromResult(options[_random.Next(options.Length)]);
    }
}
