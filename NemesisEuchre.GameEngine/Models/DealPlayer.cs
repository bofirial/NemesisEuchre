using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.GameEngine.Models;

public class DealPlayer
{
    public PlayerPosition Position { get; set; }

    public Card[] StartingHand { get; set; } = [];

    public List<Card> CurrentHand { get; set; } = [];

    public string? ActorType { get; set; }
}
