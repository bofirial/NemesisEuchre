using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.GameEngine.Models;

public class Trick
{
    public short TrickNumber { get; set; }

    public PlayerPosition LeadPosition { get; set; }

    public Suit? LeadSuit { get; set; }

    public List<PlayedCard> CardsPlayed { get; } = [];

    public PlayerPosition? WinningPosition { get; set; }

    public Team? WinningTeam { get; set; }

    public List<PlayCardDecisionRecord> PlayCardDecisions { get; set; } = [];
}
