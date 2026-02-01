using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.DataAccess;

public class PlayCardTrainingDataLoader(
    IGameRepository gameRepository,
    IFeatureEngineer<PlayCardDecisionEntity, PlayCardTrainingData> featureEngineer,
    ILogger<PlayCardTrainingDataLoader> logger)
    : ITrainingDataLoader<PlayCardTrainingData>
{
    public async Task<IEnumerable<PlayCardTrainingData>> LoadTrainingDataAsync(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        CancellationToken cancellationToken = default)
    {
        LoggerMessages.LogLoadingTrainingData(logger, actorType.ToString(), limit, winningTeamOnly);

        var trainingData = new List<PlayCardTrainingData>();
        var entityCount = 0;
        var transformErrorCount = 0;

        await foreach (var entity in gameRepository.GetPlayCardTrainingDataAsync(
            actorType, limit, winningTeamOnly, cancellationToken))
        {
            if (entity.RelativeDealPoints == null)
            {
                continue;
            }

            try
            {
                var transformed = featureEngineer.Transform(entity);
                trainingData.Add(transformed);
                entityCount++;

                if (entityCount % 10000 == 0)
                {
                    LoggerMessages.LogTrainingDataProgress(logger, entityCount);
                }
            }
            catch (Exception ex)
            {
                transformErrorCount++;
                LoggerMessages.LogFeatureEngineeringError(logger, ex);
            }
        }

        LoggerMessages.LogTrainingDataLoadComplete(logger, entityCount, transformErrorCount);
        return trainingData;
    }
}
