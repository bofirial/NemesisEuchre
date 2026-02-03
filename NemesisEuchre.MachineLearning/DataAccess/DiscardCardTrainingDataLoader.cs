using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.DataAccess;

public class DiscardCardTrainingDataLoader(
    ITrainingDataRepository trainingDataRepository,
    IFeatureEngineer<DiscardCardDecisionEntity, DiscardCardTrainingData> featureEngineer,
    ILogger<DiscardCardTrainingDataLoader> logger)
    : TrainingDataLoaderBase<DiscardCardDecisionEntity, DiscardCardTrainingData>(trainingDataRepository, featureEngineer, logger)
{
    protected override bool IsEntityValid(DiscardCardDecisionEntity entity)
    {
        return entity.RelativeDealPoints != null;
    }
}
