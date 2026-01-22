using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public class GameFactory : IGameFactory
{
    public async Task<Game> CreateGameAsync(GameOptions gameOptions)
    {
        ArgumentNullException.ThrowIfNull(gameOptions);
        ArgumentOutOfRangeException.ThrowIfLessThan(gameOptions.WinningScore, 1);

        ValidateBotTypes(gameOptions);

        var game = new Game
        {
            WinningScore = gameOptions.WinningScore,
        };

        game.Players.Add(PlayerPosition.North, new Player
        {
            Position = PlayerPosition.North,
            BotType = gameOptions.Team1BotTypes[0],
        });
        game.Players.Add(PlayerPosition.East, new Player
        {
            Position = PlayerPosition.East,
            BotType = gameOptions.Team2BotTypes[0],
        });
        game.Players.Add(PlayerPosition.South, new Player
        {
            Position = PlayerPosition.South,
            BotType = gameOptions.Team1BotTypes[1],
        });
        game.Players.Add(PlayerPosition.West, new Player
        {
            Position = PlayerPosition.West,
            BotType = gameOptions.Team2BotTypes[1],
        });

        return game;
    }

    private static void ValidateBotTypes(GameOptions gameOptions)
    {
        ArgumentNullException.ThrowIfNull(gameOptions.Team1BotTypes);
        ArgumentNullException.ThrowIfNull(gameOptions.Team2BotTypes);

        if (gameOptions.Team1BotTypes.Length != 2)
        {
            throw new ArgumentException("Team1BotTypes must contain exactly 2 bot types.", nameof(gameOptions));
        }

        if (gameOptions.Team2BotTypes.Length != 2)
        {
            throw new ArgumentException("Team2BotTypes must contain exactly 2 bot types.", nameof(gameOptions));
        }
    }
}
