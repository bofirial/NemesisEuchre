using Microsoft.Extensions.Options;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Options;

namespace NemesisEuchre.GameEngine;

public interface IGameFactory
{
    Task<Game> CreateGameAsync(Actor[]? team1Actors = null, Actor[]? team2Actors = null);
}

public class GameFactory(IOptions<GameOptions> gameOptions) : IGameFactory
{
    private const int PlayersPerTeam = 2;

    public async Task<Game> CreateGameAsync(Actor[]? team1Actors = null, Actor[]? team2Actors = null)
    {
        ArgumentNullException.ThrowIfNull(gameOptions);

        team1Actors ??= gameOptions.Value.Team1Actors;
        team2Actors ??= gameOptions.Value.Team2Actors;

        ValidateActorTypes(team1Actors, team2Actors);

        return new Game
        {
            Players =
            {
                [PlayerPosition.North] = CreatePlayer(PlayerPosition.North, team1Actors[0]),
                [PlayerPosition.East] = CreatePlayer(PlayerPosition.East, team2Actors[0]),
                [PlayerPosition.South] = CreatePlayer(PlayerPosition.South, team1Actors[1]),
                [PlayerPosition.West] = CreatePlayer(PlayerPosition.West, team2Actors[1]),
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

    private static void ValidateActorTypes(Actor[] team1Actors, Actor[] team2Actors)
    {
        ArgumentNullException.ThrowIfNull(team1Actors);
        ArgumentNullException.ThrowIfNull(team2Actors);

        if (team1Actors.Length != PlayersPerTeam)
        {
            throw new ArgumentException($"Team1ActorTypes must contain exactly {PlayersPerTeam} actor types.", nameof(team1Actors));
        }

        if (team2Actors.Length != PlayersPerTeam)
        {
            throw new ArgumentException($"Team2ActorTypes must contain exactly {PlayersPerTeam} actor types.", nameof(team2Actors));
        }
    }
}
