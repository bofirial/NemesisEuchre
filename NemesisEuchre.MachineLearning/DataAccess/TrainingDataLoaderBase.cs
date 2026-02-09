using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.FeatureEngineering;

namespace NemesisEuchre.MachineLearning.DataAccess;

public interface ITrainingDataLoader<TTrainingData>
    where TTrainingData : class, new()
{
    Task<IEnumerable<TTrainingData>> LoadTrainingDataAsync(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        CancellationToken cancellationToken = default);

    IEnumerable<TTrainingData> StreamTrainingData(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        bool shuffle = false,
        CancellationToken cancellationToken = default);
}

public abstract class TrainingDataLoaderBase<TEntity, TTrainingData>(
    ITrainingDataRepository trainingDataRepository,
    IFeatureEngineer<TEntity, TTrainingData> featureEngineer,
    ILogger logger)
    : ITrainingDataLoader<TTrainingData>
    where TEntity : class, IDecisionEntity
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

        await foreach (var entity in trainingDataRepository.GetDecisionDataAsync<TEntity>(
            actorType, limit, winningTeamOnly, cancellationToken))
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

    public IEnumerable<TTrainingData> StreamTrainingData(
        ActorType actorType,
        int limit = 0,
        bool winningTeamOnly = false,
        bool shuffle = false,
        CancellationToken cancellationToken = default)
    {
        LoggerMessages.LogLoadingTrainingData(logger, actorType.ToString(), limit, winningTeamOnly);

        var entityCount = 0;
        var transformErrorCount = 0;

        foreach (var entity in trainingDataRepository.GetDecisionData<TEntity>(
            actorType, limit, winningTeamOnly, shuffle))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!IsEntityValid(entity))
            {
                continue;
            }

            TTrainingData? transformed = null;
            try
            {
                transformed = featureEngineer.Transform(entity);
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

            if (transformed != null)
            {
                yield return transformed;
            }
        }

        LoggerMessages.LogTrainingDataLoadComplete(logger, entityCount, transformErrorCount);
    }

    protected abstract bool IsEntityValid(TEntity entity);
}
