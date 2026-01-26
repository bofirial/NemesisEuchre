using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.GameEngine.Models;

public class DiscardCardDecisionRecord
{
    public Card[] Hand { get; set; } = [];

    public PlayerPosition DealerPosition { get; set; }

    public short TeamScore { get; set; }

    public short OpponentScore { get; set; }

    public Card[] ValidCardsToDiscard { get; set; } = [];

    public Card ChosenCard { get; set; } = new();
}
