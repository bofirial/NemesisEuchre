using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.GameEngine.Models;

public class PlayCardDecisionRecord
{
    public Card[] Hand { get; set; } = [];

    public PlayerPosition DecidingPlayerPosition { get; set; }

    public Trick CurrentTrick { get; set; } = new();

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public Card[] ValidCardsToPlay { get; set; } = [];

    public Card ChosenCard { get; set; } = null!;

    public PlayerPosition LeadPosition { get; set; }
}
