using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public class GameFactory : IGameFactory
{
    public async Task<Game> CreateGameAsync(GameOptions gameOptions)
    {
        ArgumentNullException.ThrowIfNull(gameOptions);
        ArgumentOutOfRangeException.ThrowIfLessThan(gameOptions.WinningScore, 1);

        ValidateActorTypes(gameOptions);

        var game = new Game
        {
            WinningScore = gameOptions.WinningScore,
        };

        game.Players.Add(PlayerPosition.North, new Player
        {
            Position = PlayerPosition.North,
            ActorType = gameOptions.Team1ActorTypes[0],
        });
        game.Players.Add(PlayerPosition.East, new Player
        {
            Position = PlayerPosition.East,
            ActorType = gameOptions.Team2ActorTypes[0],
        });
        game.Players.Add(PlayerPosition.South, new Player
        {
            Position = PlayerPosition.South,
            ActorType = gameOptions.Team1ActorTypes[1],
        });
        game.Players.Add(PlayerPosition.West, new Player
        {
            Position = PlayerPosition.West,
            ActorType = gameOptions.Team2ActorTypes[1],
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
