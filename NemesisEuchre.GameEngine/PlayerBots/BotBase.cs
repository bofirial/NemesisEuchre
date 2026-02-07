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

    public Task<CallTrumpDecision> CallTrumpAsync(CallTrumpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return CallTrumpAsync(
            context.CardsInHand,
            context.TeamScore,
            context.OpponentScore,
            context.DealerPosition.ToRelativePosition(context.PlayerPosition),
            context.UpCard,
            context.ValidCallTrumpDecisions);
    }

    public abstract Task<CallTrumpDecision> CallTrumpAsync(
        Card[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition dealerPosition,
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
        RelativePlayerSuitVoid[] knownPlayerSuitVoids,
        RelativeCard[] cardsAccountedFor,
        Dictionary<RelativePlayerPosition, RelativeCard> playedCardsInTrick,
        RelativePlayerPosition? currentlyWinningTrickPlayer,
        short trickNumber,
        RelativeCard[] validCardsToPlay);

    public Task<Card> DiscardCardAsync(DiscardCardContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return DiscardCardAsync(
            context.CardsInHand,
            context.PlayerPosition,
            context.TeamScore,
            context.OpponentScore,
            context.TrumpSuit,
            context.CallingPlayer,
            context.CallingPlayerGoingAlone,
            context.ValidCardsToDiscard);
    }

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

    public Task<Card> PlayCardAsync(PlayCardContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return PlayCardAsync(
            context.CardsInHand,
            context.PlayerPosition,
            context.TeamScore,
            context.OpponentScore,
            context.TrumpSuit,
            context.CallingPlayer,
            context.CallingPlayerIsGoingAlone,
            context.Dealer,
            context.DealerPickedUpCard,
            context.LeadPlayer,
            context.LeadSuit,
            context.KnownPlayerSuitVoids,
            context.CardsAccountedFor,
            context.PlayedCardsInTrick,
            context.CurrentlyWinningTrickPlayer,
            context.TrickNumber,
            context.ValidCardsToPlay);
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
        PlayerSuitVoid[] knownPlayerSuitVoids,
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
            [.. knownPlayerSuitVoids.Select(kpv => new RelativePlayerSuitVoid() { PlayerPosition = kpv.PlayerPosition.ToRelativePosition(playerPosition), Suit = kpv.Suit.ToRelativeSuit(trumpSuit) })],
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
