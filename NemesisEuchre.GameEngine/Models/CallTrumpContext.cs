using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Models;

public sealed class CallTrumpContext
{
    public required Card[] CardsInHand { get; init; }

    public required PlayerPosition PlayerPosition { get; init; }

    public required short TeamScore { get; init; }

    public required short OpponentScore { get; init; }

    public required PlayerPosition DealerPosition { get; init; }

    public required Card UpCard { get; init; }

    public required CallTrumpDecision[] ValidCallTrumpDecisions { get; init; }
}
