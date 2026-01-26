using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Models;

public class CallTrumpDecisionRecord
{
    public Card[] Hand { get; set; } = [];

    public Card UpCard { get; set; } = new();

    public PlayerPosition DealerPosition { get; set; }

    public PlayerPosition DecidingPlayerPosition { get; set; }

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public CallTrumpDecision[] ValidDecisions { get; set; } = [];

    public CallTrumpDecision ChosenDecision { get; set; }

    public byte DecisionOrder { get; set; }
}
