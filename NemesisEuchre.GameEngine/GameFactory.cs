using Microsoft.Extensions.Options;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Options;

namespace NemesisEuchre.GameEngine;

public interface IGameFactory
{
    Task<Game> CreateGameAsync(ActorType[]? team1ActorTypes = null, ActorType[]? team2ActorTypes = null);
}

public class GameFactory(IOptions<GameOptions> gameOptions) : IGameFactory
{
    private const int PlayersPerTeam = 2;

    public async Task<Game> CreateGameAsync(ActorType[]? team1ActorTypes = null, ActorType[]? team2ActorTypes = null)
    {
        ArgumentNullException.ThrowIfNull(gameOptions);

        team1ActorTypes ??= gameOptions.Value.Team1ActorTypes;
        team2ActorTypes ??= gameOptions.Value.Team2ActorTypes;

        ValidateActorTypes(team1ActorTypes, team2ActorTypes);

        return new Game
        {
            Players =
            {
                [PlayerPosition.North] = CreatePlayer(PlayerPosition.North, new Actor(team1ActorTypes[0], null)),
                [PlayerPosition.East] = CreatePlayer(PlayerPosition.East, new Actor(team2ActorTypes[0], null)),
                [PlayerPosition.South] = CreatePlayer(PlayerPosition.South, new Actor(team1ActorTypes[1], null)),
                [PlayerPosition.West] = CreatePlayer(PlayerPosition.West, new Actor(team2ActorTypes[1], null)),
            },
        };
    }

    private static Player CreatePlayer(PlayerPosition position, Actor actor)
    {
        return new Player
        {
            Position = position,
            Actor = actor,
        };
    }

    private static void ValidateActorTypes(ActorType[] team1ActorTypes, ActorType[] team2ActorTypes)
    {
        ArgumentNullException.ThrowIfNull(team1ActorTypes);
        ArgumentNullException.ThrowIfNull(team2ActorTypes);

        if (team1ActorTypes.Length != PlayersPerTeam)
        {
            throw new ArgumentException($"Team1ActorTypes must contain exactly {PlayersPerTeam} actor types.", nameof(team1ActorTypes));
        }

        if (team2ActorTypes.Length != PlayersPerTeam)
        {
            throw new ArgumentException($"Team2ActorTypes must contain exactly {PlayersPerTeam} actor types.", nameof(team2ActorTypes));
        }
    }
}
