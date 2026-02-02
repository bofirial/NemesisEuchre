using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Services;

public interface ISingleGameRunner
{
    Task<Game> RunAsync(bool doNotPersist = false, ActorType[]? team1ActorTypes = null, ActorType[]? team2ActorTypes = null, CancellationToken cancellationToken = default);
}

public class SingleGameRunner(
    IGameOrchestrator gameOrchestrator,
    IGameRepository gameRepository,
    IGameResultsRenderer gameResultsRenderer,
    ILogger<SingleGameRunner> logger) : ISingleGameRunner
{
    public async Task<Game> RunAsync(bool doNotPersist = false, ActorType[]? team1ActorTypes = null, ActorType[]? team2ActorTypes = null, CancellationToken cancellationToken = default)
    {
        var game = await gameOrchestrator.OrchestrateGameAsync(team1ActorTypes, team2ActorTypes);

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
