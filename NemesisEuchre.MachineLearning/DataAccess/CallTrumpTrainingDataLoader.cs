using Microsoft.Extensions.Logging;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.DataAccess;

public class CallTrumpTrainingDataLoader(
    ITrainingDataRepository trainingDataRepository,
    IFeatureEngineer<CallTrumpDecisionEntity, CallTrumpTrainingData> featureEngineer,
    ILogger<CallTrumpTrainingDataLoader> logger)
    : TrainingDataLoaderBase<CallTrumpDecisionEntity, CallTrumpTrainingData>(trainingDataRepository, featureEngineer, logger)
{
    protected override bool IsEntityValid(CallTrumpDecisionEntity entity)
    {
        return entity.RelativeDealPoints != null;
    }
}
