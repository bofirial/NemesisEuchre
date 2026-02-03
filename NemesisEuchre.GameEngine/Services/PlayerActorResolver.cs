using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Services;

public interface IPlayerActorResolver
{
    IPlayerActor GetPlayerActor(DealPlayer player);
}

public class PlayerActorResolver(IEnumerable<IPlayerActor> playerActors) : IPlayerActorResolver
{
    private readonly Dictionary<ActorType, IPlayerActor> _playerActors = playerActors.ToDictionary(x => x.ActorType, x => x);

    public IPlayerActor GetPlayerActor(DealPlayer player)
    {
        return _playerActors[player.ActorType!.Value];
    }
}
