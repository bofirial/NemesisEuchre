using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.DataAccess.Entities;

public class PlayCardDecisionEntity
{
    public int PlayCardDecisionId { get; set; }

    public int DealId { get; set; }

    public int TrickNumber { get; set; }

    public string HandJson { get; set; } = null!;

    public PlayerPosition DecidingPlayerPosition { get; set; }

    public string CurrentTrickJson { get; set; } = null!;

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public string ValidCardsToPlayJson { get; set; } = null!;

    public string ChosenCardJson { get; set; } = null!;

    public PlayerPosition LeadPosition { get; set; }

    public ActorType? ActorType { get; set; }

    public bool? DidTeamWinTrick { get; set; }

    public bool? DidTeamWinDeal { get; set; }

    public bool? DidTeamWinGame { get; set; }

    public DealEntity Deal { get; set; } = null!;
}
