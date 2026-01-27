using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.DataAccess.Entities;

public class DealEntity
{
    public int DealId { get; set; }

    public int GameId { get; set; }

    public int DealNumber { get; set; }

    public DealStatus DealStatus { get; set; }

    public PlayerPosition? DealerPosition { get; set; }

    public string DeckJson { get; set; } = null!;

    public string? UpCardJson { get; set; }

    public Suit? Trump { get; set; }

    public PlayerPosition? CallingPlayer { get; set; }

    public bool CallingPlayerIsGoingAlone { get; set; }

    public DealResult? DealResult { get; set; }

    public Team? WinningTeam { get; set; }

    public short Team1Score { get; set; }

    public short Team2Score { get; set; }

    public string PlayersJson { get; set; } = null!;

    public GameEntity Game { get; set; } = null!;

    public ICollection<TrickEntity> Tricks { get; set; } = [];

    public ICollection<CallTrumpDecisionEntity> CallTrumpDecisions { get; set; } = [];

    public ICollection<DiscardCardDecisionEntity> DiscardCardDecisions { get; set; } = [];

    public ICollection<PlayCardDecisionEntity> PlayCardDecisions { get; set; } = [];
}
