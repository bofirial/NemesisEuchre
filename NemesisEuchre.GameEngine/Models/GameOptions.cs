using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Models;

public class GameOptions
{
    public short WinningScore { get; set; } = 10;

    public ActorType[] Team1ActorTypes { get; set; } = [ActorType.Chaos, ActorType.Chaos];

    public ActorType[] Team2ActorTypes { get; set; } = [ActorType.Chaos, ActorType.Chaos];
}
