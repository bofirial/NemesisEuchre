using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.GameEngine;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Services;

public interface ISingleGameRunner
{
    Task<Game> RunAsync(bool doNotPersist = false, CancellationToken cancellationToken = default);
}

public class SingleGameRunner(
    IGameOrchestrator gameOrchestrator,
    IGameRepository gameRepository,
    IGameResultsRenderer gameResultsRenderer,
    ILogger<SingleGameRunner> logger) : ISingleGameRunner
{
    public async Task<Game> RunAsync(bool doNotPersist = false, CancellationToken cancellationToken = default)
    {
        var game = await gameOrchestrator.OrchestrateGameAsync();

        if (!doNotPersist)
        {
            try
            {
                await gameRepository.SaveCompletedGameAsync(game, cancellationToken);
            }
            catch (Exception ex)
            {
                Foundation.LoggerMessages.LogGamePersistenceFailed(logger, ex);
            }
        }
        else
        {
            Foundation.LoggerMessages.LogGamePersistenceSkipped(logger);
        }

        gameResultsRenderer.RenderResults(game);

        return game;
    }
}
