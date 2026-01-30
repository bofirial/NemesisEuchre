using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation;
using NemesisEuchre.GameEngine;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Services;

public interface ISingleGameRunner
{
    Task<Game> RunAsync(CancellationToken cancellationToken = default);
}

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
