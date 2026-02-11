using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Services;

public interface IPlayerActorResolver
{
    IPlayerActor GetPlayerActor(DealPlayer player);
}

public class PlayerActorResolver(IEnumerable<IPlayerActor> playerActors, IEnumerable<IPlayerActorFactory> playerActorFactories) : IPlayerActorResolver
{
    private readonly Dictionary<Actor, IPlayerActor> _playerActors = playerActors.ToDictionary(x => new Actor(x.ActorType), x => x);
    private readonly Dictionary<ActorType, IPlayerActorFactory> _playerActorFactories = playerActorFactories.ToDictionary(x => x.ActorType, x => x);

    public IPlayerActor GetPlayerActor(DealPlayer player)
    {
        if (!_playerActors.TryGetValue(player.Actor, out IPlayerActor? playerActor))
        {
            if (_playerActorFactories.TryGetValue(player.Actor.ActorType, out IPlayerActorFactory? factory))
            {
                playerActor = factory.CreatePlayerActor(player.Actor);

                _playerActors.Add(player.Actor, playerActor);
            }
            else
            {
                throw new InvalidOperationException($"No player actor or player actor factory found for actor: {player.Actor}");
            }
        }

        return playerActor;
    }
}
