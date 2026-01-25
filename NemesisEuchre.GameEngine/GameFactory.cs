using Microsoft.Extensions.Options;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public interface IGameFactory
{
    Task<Game> CreateGameAsync();
}

public class GameFactory(IOptions<GameOptions> gameOptions) : IGameFactory
{
    public async Task<Game> CreateGameAsync()
    {
        ArgumentNullException.ThrowIfNull(gameOptions);

        ValidateActorTypes(gameOptions.Value);

        var game = new Game();

        game.Players.Add(PlayerPosition.North, new Player
        {
            Position = PlayerPosition.North,
            ActorType = gameOptions.Value.Team1ActorTypes[0],
        });
        game.Players.Add(PlayerPosition.East, new Player
        {
            Position = PlayerPosition.East,
            ActorType = gameOptions.Value.Team2ActorTypes[0],
        });
        game.Players.Add(PlayerPosition.South, new Player
        {
            Position = PlayerPosition.South,
            ActorType = gameOptions.Value.Team1ActorTypes[1],
        });
        game.Players.Add(PlayerPosition.West, new Player
        {
            Position = PlayerPosition.West,
            ActorType = gameOptions.Value.Team2ActorTypes[1],
        });

        return game;
    }

    private static void ValidateActorTypes(GameOptions gameOptions)
    {
        ArgumentNullException.ThrowIfNull(gameOptions.Team1ActorTypes);
        ArgumentNullException.ThrowIfNull(gameOptions.Team2ActorTypes);

        if (gameOptions.Team1ActorTypes.Length != 2)
        {
            throw new ArgumentException("Team1ActorTypes must contain exactly 2 actor types.", nameof(gameOptions));
        }

        if (gameOptions.Team2ActorTypes.Length != 2)
        {
            throw new ArgumentException("Team2ActorTypes must contain exactly 2 actor types.", nameof(gameOptions));
        }
    }
}
