using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Utilities;

namespace NemesisEuchre.GameEngine.PlayerBots;

public abstract class BotBase(IRandomNumberGenerator random) : IPlayerActor
{
    public abstract ActorType ActorType { get; }

    protected IRandomNumberGenerator Random { get; } = random ?? throw new ArgumentNullException(nameof(random));

    public Task<CallTrumpDecisionContext> CallTrumpAsync(CallTrumpContext context)
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

    public abstract Task<CallTrumpDecisionContext> CallTrumpAsync(
        Card[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition dealerPosition,
        Card upCard,
        CallTrumpDecision[] validCallTrumpDecisions);

    public abstract Task<RelativeCardDecisionContext> DiscardCardAsync(
        RelativeCard[] cardsInHand,
        short teamScore,
        short opponentScore,
        RelativePlayerPosition callingPlayer,
        bool callingPlayerGoingAlone,
        RelativeCard[] validCardsToDiscard);

    public abstract Task<RelativeCardDecisionContext> PlayCardAsync(
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
        short wonTricks,
        short opponentsWonTricks,
        RelativeCard[] validCardsToPlay);

    public Task<CardDecisionContext> DiscardCardAsync(DiscardCardContext context)
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

    public async Task<CardDecisionContext> DiscardCardAsync(
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

        return new CardDecisionContext()
        {
            ChosenCard = relativeChoice.ChosenCard.Card!,
            DecisionPredictedPoints = relativeChoice.DecisionPredictedPoints.ToDictionary(kvp => kvp.Key.Card!, kvp => kvp.Value),
        };
    }

    public Task<CardDecisionContext> PlayCardAsync(PlayCardContext context)
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
            context.WonTricks,
            context.OpponentsWonTricks,
            context.ValidCardsToPlay);
    }

    public async Task<CardDecisionContext> PlayCardAsync(
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
        short wonTricks,
        short opponentsWonTricks,
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
            wonTricks,
            opponentsWonTricks,
            relativeValidCards);

        return new CardDecisionContext()
        {
            ChosenCard = relativeChoice.ChosenCard.Card!,
            DecisionPredictedPoints = relativeChoice.DecisionPredictedPoints.ToDictionary(kvp => kvp.Key.Card!, kvp => kvp.Value),
        };
    }

    protected T SelectRandom<T>(T[] options)
    {
        return options.Length == 0
            ? throw new ArgumentException("Cannot select from empty array", nameof(options))
            : options[Random.NextInt(options.Length)];
    }
}
