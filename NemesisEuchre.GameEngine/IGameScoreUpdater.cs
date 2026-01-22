using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public interface IGameScoreUpdater
{
    Task UpdateGameScoreAsync(Game game, Deal deal);
}
