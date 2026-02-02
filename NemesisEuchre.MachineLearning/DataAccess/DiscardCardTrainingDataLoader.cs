using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.DataAccess;

public class DiscardCardTrainingDataLoader(
    IGameRepository gameRepository,
    IFeatureEngineer<DiscardCardDecisionEntity, DiscardCardTrainingData> featureEngineer,
    ILogger<DiscardCardTrainingDataLoader> logger)
    : TrainingDataLoaderBase<DiscardCardDecisionEntity, DiscardCardTrainingData>(gameRepository, featureEngineer, logger)
{
    protected override IAsyncEnumerable<DiscardCardDecisionEntity> GetTrainingDataEntitiesAsync(
        IGameRepository repository,
        ActorType actorType,
        int limit,
        bool winningTeamOnly,
        CancellationToken cancellationToken)
    {
        return repository.GetDiscardCardTrainingDataAsync(actorType, limit, winningTeamOnly, cancellationToken);
    }

    protected override bool IsEntityValid(DiscardCardDecisionEntity entity)
    {
        return entity.RelativeDealPoints != null;
    }
}
