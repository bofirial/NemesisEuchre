using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation;
using NemesisEuchre.GameEngine;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Services;

/// <summary>
/// Orchestrates single game execution including game play, persistence, and result rendering.
/// </summary>
/// <param name="gameOrchestrator">The game orchestrator for executing games.</param>
/// <param name="gameRepository">The repository for persisting completed games.</param>
/// <param name="gameResultsRenderer">The renderer for displaying game results.</param>
/// <param name="logger">The logger for recording execution details.</param>
public class SingleGameRunner(
    IGameOrchestrator gameOrchestrator,
    IGameRepository gameRepository,
    IGameResultsRenderer gameResultsRenderer,
    ILogger<SingleGameRunner> logger) : ISingleGameRunner
{
    public async Task<Game> RunAsync(CancellationToken cancellationToken = default)
    {
        var game = await gameOrchestrator.OrchestrateGameAsync();

        try
        {
            await gameRepository.SaveCompletedGameAsync(game, cancellationToken);
        }
        catch (Exception ex)
        {
            LoggerMessages.LogGamePersistenceFailed(logger, ex);
        }

        gameResultsRenderer.RenderResults(game);

        return game;
    }
}
