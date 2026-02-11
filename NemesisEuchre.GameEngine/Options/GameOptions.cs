using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.GameEngine.Options;

public class GameOptions
{
    public bool StickTheDealer { get; set; } = true;

    public short WinningScore { get; set; } = 10;

    public Actor[] Team1Actors { get; set; } = [new Actor(ActorType.Chaos), new Actor(ActorType.Chaos)];

    public Actor[] Team2Actors { get; set; } = [new Actor(ActorType.Chaos), new Actor(ActorType.Chaos)];
}
