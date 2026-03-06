using System.Collections.Concurrent;

using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Server.Services;

public interface IActiveGameService
{
    void StoreGame(int sessionId, Game game);

    Game? GetGame(int sessionId);
}

public class ActiveGameService : IActiveGameService
{
    private readonly ConcurrentDictionary<int, Game> _games = new();

    public void StoreGame(int sessionId, Game game)
    {
        _games[sessionId] = game;
    }

    public Game? GetGame(int sessionId)
    {
        return _games.GetValueOrDefault(sessionId);
    }
}
