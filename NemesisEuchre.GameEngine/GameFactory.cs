using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public class GameFactory : IGameFactory
{
    public async Task<Game> CreateGameAsync(GameOptions gameOptions)
    {
        ArgumentNullException.ThrowIfNull(gameOptions);
        ArgumentOutOfRangeException.ThrowIfLessThan(gameOptions.WinningScore, 1);

        var game = new Game();

        game.Players.Add(PlayerPosition.North, new Player { Position = PlayerPosition.North });
        game.Players.Add(PlayerPosition.East, new Player { Position = PlayerPosition.East });
        game.Players.Add(PlayerPosition.South, new Player { Position = PlayerPosition.South });
        game.Players.Add(PlayerPosition.West, new Player { Position = PlayerPosition.West });

        return game;
    }
}
