using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Models;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Services;

public interface ISingleGameRunner
{
    Task<Game> RunAsync(GamePersistenceOptions? persistenceOptions = null, ActorType[]? team1ActorTypes = null, ActorType[]? team2ActorTypes = null, bool showDecisions = false, CancellationToken cancellationToken = default);
}

public class SingleGameRunner(
    IGameOrchestrator gameOrchestrator,
    IGameRepository gameRepository,
    IGameResultsRenderer gameResultsRenderer,
    ILogger<SingleGameRunner> logger) : ISingleGameRunner
{
    public async Task<Game> RunAsync(GamePersistenceOptions? persistenceOptions = null, ActorType[]? team1ActorTypes = null, ActorType[]? team2ActorTypes = null, bool showDecisions = false, CancellationToken cancellationToken = default)
    {
        persistenceOptions ??= new GamePersistenceOptions(false, null);

        var game = await gameOrchestrator.OrchestrateGameAsync(team1ActorTypes, team2ActorTypes);

        if (persistenceOptions.IdvGenerationName != null)
        {
            Foundation.LoggerMessages.LogIdvSkippedSingleGame(logger);
        }

        if (persistenceOptions.PersistToSql)
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

        gameResultsRenderer.RenderResults(game, showDecisions);

        return game;
    }
}
