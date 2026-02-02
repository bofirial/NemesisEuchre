using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.DataAccess;

public class PlayCardTrainingDataLoader(
    IGameRepository gameRepository,
    IFeatureEngineer<PlayCardDecisionEntity, PlayCardTrainingData> featureEngineer,
    ILogger<PlayCardTrainingDataLoader> logger)
    : TrainingDataLoaderBase<PlayCardDecisionEntity, PlayCardTrainingData>(gameRepository, featureEngineer, logger)
{
    protected override IAsyncEnumerable<PlayCardDecisionEntity> GetTrainingDataEntitiesAsync(
        IGameRepository repository,
        ActorType actorType,
        int limit,
        bool winningTeamOnly,
        CancellationToken cancellationToken)
    {
        return repository.GetPlayCardTrainingDataAsync(actorType, limit, winningTeamOnly, cancellationToken);
    }

    protected override bool IsEntityValid(PlayCardDecisionEntity entity)
    {
        return entity.RelativeDealPoints != null;
    }
}
