using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.DataAccess.Entities;

public class TrickEntity
{
    public int TrickId { get; set; }

    public int DealId { get; set; }

    public int TrickNumber { get; set; }

    public PlayerPosition LeadPosition { get; set; }

    public string CardsPlayedJson { get; set; } = null!;

    public Suit? LeadSuit { get; set; }

    public PlayerPosition? WinningPosition { get; set; }

    public Team? WinningTeam { get; set; }

    public DealEntity Deal { get; set; } = null!;
}
