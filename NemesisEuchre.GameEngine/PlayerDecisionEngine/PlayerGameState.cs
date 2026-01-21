namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public class PlayerGameState
{
    public List<RelativeCard> Hand { get; set; } = [];

    public int TeamScore { get; set; }

    public int OpponentScore { get; set; }

    public RelativeHand? CurrentHand { get; set; }
}
