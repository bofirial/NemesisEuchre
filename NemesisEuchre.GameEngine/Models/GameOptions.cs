using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Models;

public class GameOptions
{
    public bool StickTheDealer { get; set; } = true;

    public short WinningScore { get; set; } = 10;

    public string[] Team1ActorTypes { get; set; } = [ActorType.Chaos, ActorType.Chaos];

    public string[] Team2ActorTypes { get; set; } = [ActorType.Chaos, ActorType.Chaos];
}
