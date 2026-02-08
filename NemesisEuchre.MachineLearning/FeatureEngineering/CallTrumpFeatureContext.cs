using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public sealed class CallTrumpFeatureContext
{
    public required Card[] Cards { get; init; }

    public required Card UpCard { get; init; }

    public required CallTrumpDecision[] ValidDecisions { get; init; }

    public required CallTrumpDecision ChosenDecision { get; init; }
}
