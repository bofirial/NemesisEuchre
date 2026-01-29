using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Options;

public class GameOptions
{
    public bool StickTheDealer { get; set; } = true;

    public short WinningScore { get; set; } = 10;

    public ActorType[] Team1ActorTypes { get; set; } = [ActorType.Chaos, ActorType.Chaos];

    public ActorType[] Team2ActorTypes { get; set; } = [ActorType.Chaos, ActorType.Chaos];
}
