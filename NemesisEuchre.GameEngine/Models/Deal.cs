using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.GameEngine.Models;

public class Deal
{
    public DealStatus DealStatus { get; set; } = DealStatus.NotStarted;

    public PlayerPosition? DealerPosition { get; set; }

    public Card[] Deck { get; set; } = [];

    public Card? UpCard { get; set; }

    public Suit? Trump { get; set; }

    public PlayerPosition? CallingPlayer { get; set; }

    public bool CallingPlayerIsGoingAlone { get; set; }

    public Trick? CurrentTrick { get; set; }

    public Trick[] CompletedTricks { get; set; } = [];

    public DealResult? DealResult { get; set; }

    public Team? WinningTeam { get; set; }
}
