using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.DataAccess.Entities;

public class DiscardCardDecisionEntity
{
    public int DiscardCardDecisionId { get; set; }

    public int DealId { get; set; }

    public string HandJson { get; set; } = null!;

    public RelativePlayerPosition CallingPlayerPosition { get; set; }

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public string ValidCardsToDiscardJson { get; set; } = null!;

    public string ChosenCardJson { get; set; } = null!;

    public ActorType? ActorType { get; set; }

    public bool? DidTeamWinDeal { get; set; }

    public bool? DidTeamWinGame { get; set; }

    public DealEntity Deal { get; set; } = null!;
}
