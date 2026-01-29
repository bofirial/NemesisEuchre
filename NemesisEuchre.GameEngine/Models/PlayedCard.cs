using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.GameEngine.Models;

public class PlayedCard
{
    public required Card Card { get; set; }

    public PlayerPosition PlayerPosition { get; set; }
}
