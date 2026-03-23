using System.Text.Json.Serialization;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.Server.Models.Events;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "eventType")]
[JsonDerivedType(typeof(PlayerNewDealStartedEvent), "NewDealStarted")]
[JsonDerivedType(typeof(PlayerTrumpDecisionMadeEvent), "TrumpDecisionMade")]
[JsonDerivedType(typeof(PlayerUpCardFlippedEvent), "UpCardFlipped")]
[JsonDerivedType(typeof(PlayerUpCardPickedUpEvent), "UpCardPickedUp")]
[JsonDerivedType(typeof(PlayerDealerDiscardedEvent), "DealerDiscarded")]
[JsonDerivedType(typeof(PlayerCardPlayedEvent), "CardPlayed")]
[JsonDerivedType(typeof(PlayerTrickCompletedEvent), "TrickCompleted")]
[JsonDerivedType(typeof(PlayerDealCompletedEvent), "DealCompleted")]
[JsonDerivedType(typeof(PlayerGameCompletedEvent), "GameCompleted")]
[JsonDerivedType(typeof(PlayerWaitingForDecisionEvent), "WaitingForDecision")]
public abstract record PlayerGameEvent(int EventIndex);

public record PlayerNewDealStartedEvent(
    int EventIndex,
    int DealNumber,
    PlayerPosition DealerPosition,
    Card UpCard,
    IReadOnlyList<Card> MyHand) : PlayerGameEvent(EventIndex);

public record PlayerTrumpDecisionMadeEvent(
    int EventIndex,
    PlayerPosition Position,
    CallTrumpDecision Decision) : PlayerGameEvent(EventIndex);

public record PlayerUpCardFlippedEvent(int EventIndex) : PlayerGameEvent(EventIndex);

public record PlayerUpCardPickedUpEvent(
    int EventIndex,
    PlayerPosition DealerPosition) : PlayerGameEvent(EventIndex);

public record PlayerDealerDiscardedEvent(
    int EventIndex,
    PlayerPosition Position,
    Card? Card) : PlayerGameEvent(EventIndex);

public record PlayerCardPlayedEvent(
    int EventIndex,
    PlayerPosition Position,
    Card Card) : PlayerGameEvent(EventIndex);

public record PlayerTrickCompletedEvent(
    int EventIndex,
    int TrickNumber,
    PlayerPosition WinnerPosition,
    Team WinningTeam) : PlayerGameEvent(EventIndex);

public record PlayerDealCompletedEvent(
    int EventIndex,
    int DealNumber,
    DealResult Result,
    Team WinningTeam,
    short PointsAwarded,
    short Team1Score,
    short Team2Score,
    PlayerPosition? CallingPlayer,
    int WinningTeamTrickCount) : PlayerGameEvent(EventIndex);

public record PlayerGameCompletedEvent(
    int EventIndex,
    Team WinningTeam,
    short Team1Score,
    short Team2Score) : PlayerGameEvent(EventIndex);

public record PlayerWaitingForDecisionEvent(
    int EventIndex,
    PlayerPosition Position) : PlayerGameEvent(EventIndex);
