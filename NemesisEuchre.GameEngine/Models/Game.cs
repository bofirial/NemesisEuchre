using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.GameEngine.Models;

public class Game
{
    public GameStatus GameStatus { get; set; } = GameStatus.NotStarted;

    public Dictionary<PlayerPosition, Player> Players { get; } = [];

    public short Team1Score { get; set; }

    public short Team2Score { get; set; }

    public Deal? CurrentDeal { get; set; }

    public List<Deal> CompletedDeals { get; } = [];

    public Team? WinningTeam { get; set; }
}
