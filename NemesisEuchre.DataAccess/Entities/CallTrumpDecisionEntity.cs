using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.DataAccess.Entities;

public class CallTrumpDecisionEntity
{
    public int CallTrumpDecisionId { get; set; }

    public int DealId { get; set; }

    public string HandJson { get; set; } = null!;

    public string UpCardJson { get; set; } = null!;

    public PlayerPosition DealerPosition { get; set; }

    public PlayerPosition DecidingPlayerPosition { get; set; }

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public string ValidDecisionsJson { get; set; } = null!;

    public string ChosenDecisionJson { get; set; } = null!;

    public byte DecisionOrder { get; set; }

    public ActorType? ActorType { get; set; }

    public bool? DidTeamWinDeal { get; set; }

    public bool? DidTeamWinGame { get; set; }

    public DealEntity Deal { get; set; } = null!;
}
