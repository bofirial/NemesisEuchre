using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;

namespace NemesisEuchre.GameEngine.PlayerBots;

public abstract class BotBase(IRandomNumberGenerator random) : IPlayerActor
{
    private readonly IRandomNumberGenerator _random = random ?? throw new ArgumentNullException(nameof(random));

    public abstract ActorType ActorType { get; }

    public abstract Task<CallTrumpDecision> CallTrumpAsync(
        Card[] cardsInHand,
        PlayerPosition playerPosition,
        short teamScore,
        short opponentScore,
        PlayerPosition dealerPosition,
        Card upCard,
        CallTrumpDecision[] validCallTrumpDecisions);

    public abstract Task<RelativeCard> DiscardCardAsync(
        RelativeCard[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativeCard[] validCardsToDiscard);

    public abstract Task<RelativeCard> PlayCardAsync(
        RelativeCard[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativePlayerPosition dealer,
        RelativeCard? dealerPickedUpCard,
        RelativePlayerPosition leadPlayer,
        RelativeSuit? leadSuit,
        (RelativePlayerPosition PlayerPosition, RelativeSuit Suit)[] knownPlayerSuitVoids,
        RelativeCard[] cardsAccountedFor,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCardsInTrick,
        RelativePlayerPosition? currentlyWinningTrickPlayer,
        short trickNumber,
        RelativeCard[] validCardsToPlay);

    public async Task<Card> DiscardCardAsync(
        Card[] cardsInHand,
        PlayerPosition playerPosition,
        short teamScore,
        short opponentScore,
        Suit trumpSuit,
        PlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        Card[] validCardsToDiscard)
    {
        var relativeHand = cardsInHand.Select(c => c.ToRelative(trumpSuit)).ToArray();
        var relativeValidCards = validCardsToDiscard.Select(c => c.ToRelative(trumpSuit)).ToArray();

        var relativeChoice = await DiscardCardAsync(relativeHand, teamScore, opponentScore, callingPlayer.ToRelativePosition(playerPosition), callingPlayerGoingAlone, relativeValidCards);
        return relativeChoice.Card!;
    }

    public async Task<Card> PlayCardAsync(
        Card[] cardsInHand,
        PlayerPosition playerPosition,
        short teamScore,
        short opponentScore,
        Suit trumpSuit,
        PlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        PlayerPosition dealer,
        Card? dealerPickedUpCard,
        PlayerPosition leadPlayer,
        Suit? leadSuit,
        (PlayerPosition PlayerPosition, Suit Suit)[] knownPlayerSuitVoids,
        Card[] cardsAccountedFor,
        Dictionary<PlayerPosition, Card> playedCardsInTrick,
        PlayerPosition? currentlyWinningTrickPlayer,
        short trickNumber,
        Card[] validCardsToPlay)
    {
        var relativeHand = cardsInHand.Select(c => c.ToRelative(trumpSuit)).ToArray();
        var relativeValidCards = validCardsToPlay.Select(c => c.ToRelative(trumpSuit)).ToArray();
        var relativeAccountedForCards = cardsAccountedFor.Select(c => c.ToRelative(trumpSuit)).ToArray();

        var relativeChoice = await PlayCardAsync(
            relativeHand,
            teamScore,
            opponentScore,
            callingPlayer.ToRelativePosition(playerPosition),
            callingPlayerGoingAlone,
            dealer.ToRelativePosition(playerPosition),
            dealerPickedUpCard?.ToRelative(trumpSuit),
            leadPlayer.ToRelativePosition(playerPosition),
            leadSuit?.ToRelativeSuit(trumpSuit),
            [.. knownPlayerSuitVoids.Select(kpv => (kpv.PlayerPosition.ToRelativePosition(playerPosition), kpv.Suit.ToRelativeSuit(trumpSuit)))],
            relativeAccountedForCards,
            playedCardsInTrick.ToDictionary(kvp => kvp.Key.ToRelativePosition(playerPosition), kvp => kvp.Value.ToRelative(trumpSuit)),
            currentlyWinningTrickPlayer?.ToRelativePosition(playerPosition),
            trickNumber,
            relativeValidCards);
        return relativeChoice.Card!;
    }

    protected Task<T> SelectRandomAsync<T>(T[] options)
    {
        return options.Length == 0
            ? throw new ArgumentException("Cannot select from empty array", nameof(options))
            : Task.FromResult(options[_random.NextInt(options.Length)]);
    }
}
