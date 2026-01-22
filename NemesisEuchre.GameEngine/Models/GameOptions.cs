using NemesisEuchre.GameEngine.PlayerBots;

namespace NemesisEuchre.GameEngine.Models;

public class GameOptions
{
    public short WinningScore { get; set; } = 10;

    public BotType[] Team1BotTypes { get; set; } = [BotType.Chaos, BotType.Chaos];

    public BotType[] Team2BotTypes { get; set; } = [BotType.Chaos, BotType.Chaos];
}
