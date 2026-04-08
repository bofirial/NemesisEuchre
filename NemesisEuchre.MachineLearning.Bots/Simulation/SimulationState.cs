using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.MachineLearning.Bots.Simulation;

public class SimulationState
{
    public required Dictionary<PlayerPosition, List<Card>> PlayerHands { get; init; }

    public required Suit Trump { get; set; }

    public required PlayerPosition CallingPlayer { get; set; }

    public required bool CallingPlayerIsGoingAlone { get; set; }

    public required PlayerPosition DealerPosition { get; init; }

    public required Card? DealerPickedUpCard { get; init; }

    public required short Team1Score { get; init; }

    public required short Team2Score { get; init; }

    public required Card? UpCard { get; init; }

    public required Card? DiscardedCard { get; set; }

    public required CallTrumpDecision? ChosenDecision { get; set; }

    public List<SimulatedTrick> CompletedTricks { get; } = [];

    public List<PlayerSuitVoid> KnownPlayerSuitVoids { get; init; } = [];
}

public class SimulatedTrick
{
    public PlayerPosition LeadPosition { get; set; }

    public Suit? LeadSuit { get; set; }

    public List<PlayedCard> CardsPlayed { get; } = [];

    public PlayerPosition? WinningPosition { get; set; }

    public Team? WinningTeam { get; set; }
}
