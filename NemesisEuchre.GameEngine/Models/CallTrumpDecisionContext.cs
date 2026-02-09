using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Models;

public class CallTrumpDecisionContext
{
    public CallTrumpDecision ChosenCallTrumpDecision { get; set; }

    public Dictionary<CallTrumpDecision, float> DecisionPredictedPoints { get; set; } = [];
}
