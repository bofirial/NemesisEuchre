using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public interface IPlayerActorFactory
{
    ActorType ActorType { get; }

    IPlayerActor CreatePlayerActor(Actor actor);
}
