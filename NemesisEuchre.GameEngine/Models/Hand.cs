using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.GameEngine.Models;

public class Hand
{
    public HandStatus HandStatus { get; set; } = HandStatus.NotStarted;

    public PlayerPosition? DealerPosition { get; set; }

    public Card? UpCard { get; set; }

    public Suit? Trump { get; set; }

    public PlayerPosition? CallingPlayer { get; set; }

    public bool CallingPlayerIsGoingAlone { get; set; }

    public Trick? CurrentTrick { get; set; }

    public List<Trick> CompletedTricks { get; } = [];
}
