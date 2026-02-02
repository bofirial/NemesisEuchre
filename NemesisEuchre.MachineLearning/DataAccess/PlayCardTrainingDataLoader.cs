using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.DataAccess;

public class PlayCardTrainingDataLoader(
    ITrainingDataRepository trainingDataRepository,
    IFeatureEngineer<PlayCardDecisionEntity, PlayCardTrainingData> featureEngineer,
    ILogger<PlayCardTrainingDataLoader> logger)
    : TrainingDataLoaderBase<PlayCardDecisionEntity, PlayCardTrainingData>(trainingDataRepository, featureEngineer, logger)
{
    protected override bool IsEntityValid(PlayCardDecisionEntity entity)
    {
        return entity.RelativeDealPoints != null;
    }
}
