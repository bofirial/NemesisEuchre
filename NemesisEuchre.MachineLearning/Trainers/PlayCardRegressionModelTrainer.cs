using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Trainers.LightGbm;

using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Utilities;

namespace NemesisEuchre.MachineLearning.Trainers;

public class PlayCardRegressionModelTrainer(
    MLContext mlContext,
    IDataSplitter dataSplitter,
    IModelPersistenceService persistenceService,
    IOptions<MachineLearningOptions> options,
    ILogger<PlayCardRegressionModelTrainer> logger)
    : RegressionModelTrainerBase<PlayCardTrainingData>(mlContext, dataSplitter, persistenceService, options, logger)
{
    protected override IEstimator<ITransformer> BuildPipeline(IDataView trainingData)
    {
        var featureColumns = FeatureColumnProvider.GetFeatureColumns<PlayCardTrainingData>();

        return MlContext.Transforms
            .Concatenate("Features", featureColumns)
            .AppendCacheCheckpoint(MlContext)
            .Append(MlContext.Regression.Trainers.LightGbm(
                new LightGbmRegressionTrainer.Options
                {
                    LabelColumnName = "Label",
                    FeatureColumnName = "Features",
                    NumberOfLeaves = Options.NumberOfLeaves,
                    MinimumExampleCountPerLeaf = Options.MinimumExampleCountPerLeaf,
                    LearningRate = Options.LearningRate,
                    NumberOfIterations = Options.NumberOfIterations,
                    Verbose = true,
                    Silent = false,
                }));
    }

    protected override string GetModelType()
    {
        return "PlayCard";
    }
}
