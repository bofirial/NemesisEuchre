using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Models;

public class CallTrumpDecisionRecord
{
    public Card[] CardsInHand { get; set; } = [];

    public PlayerPosition PlayerPosition { get; set; }

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public short DecisionOrder { get; set; }

    public PlayerPosition DealerPosition { get; set; }

    public Card UpCard { get; set; } = new();

    public CallTrumpDecision[] ValidCallTrumpDecisions { get; set; } = [];

    public CallTrumpDecision ChosenDecision { get; set; }

    public Dictionary<CallTrumpDecision, float> DecisionPredictedPoints { get; set; } = [];
}
