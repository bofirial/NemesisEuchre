using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public class RelativePlayedCard
{
    public required Card Card { get; set; }

    public RelativePlayerPosition PlayerPosition { get; set; }
}
