namespace NemesisEuchre.GameEngine;

public class GameFactory : IGameFactory
{
    public async Task<Game> CreateGameAsync(GameOptions gameOptions)
    {
        return new Game();
    }
}
