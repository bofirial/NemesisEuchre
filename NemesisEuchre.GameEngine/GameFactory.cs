using Microsoft.Extensions.Options;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Options;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine;

public interface IGameFactory
{
    Task<Game> CreateGameAsync();
}

public class GameFactory(IOptions<GameOptions> gameOptions) : IGameFactory
{
    private const int PlayersPerTeam = 2;

    public async Task<Game> CreateGameAsync()
    {
        ArgumentNullException.ThrowIfNull(gameOptions);

        ValidateActorTypes(gameOptions.Value);

        return new Game
        {
            Players =
            {
                [PlayerPosition.North] = CreatePlayer(PlayerPosition.North, gameOptions.Value.Team1ActorTypes[0]),
                [PlayerPosition.East] = CreatePlayer(PlayerPosition.East, gameOptions.Value.Team2ActorTypes[0]),
                [PlayerPosition.South] = CreatePlayer(PlayerPosition.South, gameOptions.Value.Team1ActorTypes[1]),
                [PlayerPosition.West] = CreatePlayer(PlayerPosition.West, gameOptions.Value.Team2ActorTypes[1]),
            },
        };
    }

    private static Player CreatePlayer(PlayerPosition position, ActorType actorType)
    {
        return new Player
        {
            Position = position,
            ActorType = actorType,
        };
    }

    private static void ValidateActorTypes(GameOptions gameOptions)
    {
        ArgumentNullException.ThrowIfNull(gameOptions.Team1ActorTypes);
        ArgumentNullException.ThrowIfNull(gameOptions.Team2ActorTypes);

        if (gameOptions.Team1ActorTypes.Length != PlayersPerTeam)
        {
            throw new ArgumentException($"Team1ActorTypes must contain exactly {PlayersPerTeam} actor types.", nameof(gameOptions));
        }

        if (gameOptions.Team2ActorTypes.Length != PlayersPerTeam)
        {
            throw new ArgumentException($"Team2ActorTypes must contain exactly {PlayersPerTeam} actor types.", nameof(gameOptions));
        }
    }
}
