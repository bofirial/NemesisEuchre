using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.PlayerBots;

namespace NemesisEuchre.GameEngine.Models;

public class Player
{
    public PlayerPosition Position { get; set; }

    public Team Team => Position.GetTeam();

    public List<Card> Hand { get; set; } = [];

    public BotType? BotType { get; set; }
}
