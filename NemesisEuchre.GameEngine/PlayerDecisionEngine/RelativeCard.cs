using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public class RelativeCard
{
    public Rank Rank { get; set; }

    public RelativeSuit Suit { get; set; }

    public required Card Card { get; set; }
}
