using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Models;

public class Deal
{
    public short DealNumber { get; set; }

    public DealStatus DealStatus { get; set; } = DealStatus.NotStarted;

    public PlayerPosition? DealerPosition { get; set; }

    public List<Card> Deck { get; set; } = [];

    public Card? UpCard { get; set; }

    public Card? DiscardedCard { get; set; }

    public Suit? Trump { get; set; }

    public CallTrumpDecision? ChosenDecision { get; set; }

    public PlayerPosition? CallingPlayer { get; set; }

    public bool CallingPlayerIsGoingAlone { get; set; }

    public DealResult? DealResult { get; set; }

    public Team? WinningTeam { get; set; }

    public short Team1Score { get; set; }

    public short Team2Score { get; set; }

    public List<PlayerSuitVoid> KnownPlayerSuitVoids { get; set; } = [];

    public List<Trick> CompletedTricks { get; set; } = [];

    public List<CallTrumpDecisionRecord> CallTrumpDecisions { get; set; } = [];

    public List<DiscardCardDecisionRecord> DiscardCardDecisions { get; set; } = [];

    public Dictionary<PlayerPosition, DealPlayer> Players { get; set; } = [];
}
