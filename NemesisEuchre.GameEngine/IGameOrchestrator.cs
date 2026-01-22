using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public interface IGameOrchestrator
{
    Task<Game> OrchestrateGameAsync(GameOptions gameOptions);
}
