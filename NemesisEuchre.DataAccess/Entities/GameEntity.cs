using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.DataAccess.Entities;

public class GameEntity
{
    public int GameId { get; set; }

    public GameStatus GameStatus { get; set; }

    public string PlayersJson { get; set; } = null!;

    public short Team1Score { get; set; }

    public short Team2Score { get; set; }

    public Team? WinningTeam { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<DealEntity> Deals { get; set; } = [];
}
