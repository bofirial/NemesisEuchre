using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.PlayerBots;

namespace NemesisEuchre.GameEngine.Models;

public class DealPlayer
{
    public PlayerPosition Position { get; set; }

    public Card[] StartingHand { get; set; } = [];

    public List<Card> CurrentHand { get; set; } = [];

    public BotType? BotType { get; set; }
}
