using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public class PlayerGameState
{
    public required Player Player { get; set; }

    public int TeamScore { get; set; }

    public int OpponentScore { get; set; }

    public Hand? CurrentHand { get; set; }
}
