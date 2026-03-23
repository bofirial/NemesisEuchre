using System.Text.Json.Serialization;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "eventType")]
[JsonDerivedType(typeof(NewDealStartedEvent), "NewDealStarted")]
[JsonDerivedType(typeof(TrumpDecisionMadeEvent), "TrumpDecisionMade")]
[JsonDerivedType(typeof(UpCardFlippedEvent), "UpCardFlipped")]
[JsonDerivedType(typeof(UpCardPickedUpEvent), "UpCardPickedUp")]
[JsonDerivedType(typeof(DealerDiscardedEvent), "DealerDiscarded")]
[JsonDerivedType(typeof(CardPlayedEvent), "CardPlayed")]
[JsonDerivedType(typeof(TrickCompletedEvent), "TrickCompleted")]
[JsonDerivedType(typeof(DealCompletedEvent), "DealCompleted")]
[JsonDerivedType(typeof(GameCompletedEvent), "GameCompleted")]
[JsonDerivedType(typeof(WaitingForDecisionEvent), "WaitingForDecision")]
public abstract record GameEvent(int EventIndex);

public record NewDealStartedEvent(
    int EventIndex,
    int DealNumber,
    PlayerPosition DealerPosition,
    Card UpCard,
    Dictionary<PlayerPosition, IReadOnlyList<Card>> Hands) : GameEvent(EventIndex);

public record TrumpDecisionMadeEvent(
    int EventIndex,
    PlayerPosition Position,
    CallTrumpDecision Decision) : GameEvent(EventIndex);

public record UpCardFlippedEvent(int EventIndex) : GameEvent(EventIndex);

public record UpCardPickedUpEvent(
    int EventIndex,
    PlayerPosition DealerPosition) : GameEvent(EventIndex);

public record DealerDiscardedEvent(
    int EventIndex,
    PlayerPosition Position,
    Card Card) : GameEvent(EventIndex);

public record CardPlayedEvent(
    int EventIndex,
    PlayerPosition Position,
    Card Card) : GameEvent(EventIndex);

public record TrickCompletedEvent(
    int EventIndex,
    int TrickNumber,
    PlayerPosition WinnerPosition,
    Team WinningTeam) : GameEvent(EventIndex);

public record DealCompletedEvent(
    int EventIndex,
    int DealNumber,
    DealResult Result,
    Team WinningTeam,
    short PointsAwarded,
    short Team1Score,
    short Team2Score,
    PlayerPosition? CallingPlayer,
    int WinningTeamTrickCount) : GameEvent(EventIndex);

public record GameCompletedEvent(
    int EventIndex,
    Team WinningTeam,
    short Team1Score,
    short Team2Score) : GameEvent(EventIndex);

public record WaitingForDecisionEvent(
    int EventIndex,
    PlayerPosition Position) : GameEvent(EventIndex);
