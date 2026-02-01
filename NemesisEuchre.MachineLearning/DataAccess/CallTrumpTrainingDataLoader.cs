using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.DataAccess;

public class CallTrumpTrainingDataLoader(
    IGameRepository gameRepository,
    IFeatureEngineer<CallTrumpDecisionEntity, CallTrumpTrainingData> featureEngineer,
    ILogger<CallTrumpTrainingDataLoader> logger)
    : TrainingDataLoaderBase<CallTrumpDecisionEntity, CallTrumpTrainingData>(gameRepository, featureEngineer, logger)
{
    protected override IAsyncEnumerable<CallTrumpDecisionEntity> GetTrainingDataEntitiesAsync(
        IGameRepository repository,
        ActorType actorType,
        int limit,
        bool winningTeamOnly,
        CancellationToken cancellationToken)
    {
        return repository.GetCallTrumpTrainingDataAsync(actorType, limit, winningTeamOnly, cancellationToken);
    }

    protected override bool IsEntityValid(CallTrumpDecisionEntity entity)
    {
        return entity.RelativeDealPoints != null;
    }
}
