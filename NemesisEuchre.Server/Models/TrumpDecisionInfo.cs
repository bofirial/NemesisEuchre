using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.Server.Models;

public record TrumpDecisionInfo
{
    public required PlayerPosition Position { get; init; }

    public required CallTrumpDecision Decision { get; init; }
}
