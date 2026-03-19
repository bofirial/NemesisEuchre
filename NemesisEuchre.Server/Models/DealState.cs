using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.Server.Models;

public record DealState
{
    public required DealStatus DealStatus { get; init; }

    public required PlayerPosition? DealerPosition { get; init; }

    public required Card? UpCard { get; init; }

    public required Suit? Trump { get; init; }

    public required PlayerPosition? CallingPlayer { get; init; }

    public required bool CallingPlayerIsGoingAlone { get; init; }

    public required IReadOnlyList<Card> MyHand { get; init; }

    public required IReadOnlyDictionary<PlayerPosition, int> OtherHandCounts { get; init; }

    public required IReadOnlyList<CompletedTrickInfo> CompletedTricks { get; init; }

    public PlayerPosition? CurrentDeciderPosition { get; init; }

    public IReadOnlyList<CallTrumpDecision>? ValidTrumpDecisions { get; init; }

    public IReadOnlyList<Card>? ValidDiscardCards { get; init; }
}
