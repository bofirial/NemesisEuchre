using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.MachineLearning.DataAccess;

public abstract class TrainingDataLoaderBase<TEntity, TTrainingData>(
    IGameRepository gameRepository,
    IFeatureEngineer<TEntity, TTrainingData> featureEngineer,
    ILogger logger)
    : ITrainingDataLoader<TTrainingData>
    where TEntity : class
    where TTrainingData : class, new()
{
    public async Task<IEnumerable<TTrainingData>> LoadTrainingDataAsync(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        CancellationToken cancellationToken = default)
    {
        LoggerMessages.LogLoadingTrainingData(logger, actorType.ToString(), limit, winningTeamOnly);

        var trainingData = new List<TTrainingData>();
        var entityCount = 0;
        var transformErrorCount = 0;

        await foreach (var entity in GetTrainingDataEntitiesAsync(
            gameRepository, actorType, limit, winningTeamOnly, cancellationToken))
        {
            if (!IsEntityValid(entity))
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

    protected abstract IAsyncEnumerable<TEntity> GetTrainingDataEntitiesAsync(
        IGameRepository repository,
        ActorType actorType,
        int limit,
        bool winningTeamOnly,
        CancellationToken cancellationToken);

    protected abstract bool IsEntityValid(TEntity entity);
}
