using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.GameEngine.Models;

public class Trick
{
    public PlayerPosition LeadPosition { get; set; }

    public List<PlayedCard> CardsPlayed { get; } = [];

    public Suit? LeadSuit { get; set; }

    public PlayerPosition? WinningPosition { get; set; }

    public Team? WinningTeam { get; set; }
}
