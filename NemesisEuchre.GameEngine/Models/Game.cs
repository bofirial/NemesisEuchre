using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.GameEngine.Models;

public class Game
{
    public GameStatus GameStatus { get; set; } = GameStatus.NotStarted;

    public Dictionary<PlayerPosition, Player> Players { get; } = [];

    public int Team1Score { get; set; }

    public int Team2Score { get; set; }

    public Deal? CurrentDeal { get; set; }

    public List<Deal> CompletedDeals { get; } = [];
}
