using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Utilities;

namespace NemesisEuchre.MachineLearning.Trainers;

public class DiscardCardRegressionModelTrainer(
    MLContext mlContext,
    IDataSplitter dataSplitter,
    IModelPersistenceService persistenceService,
    IOptions<MachineLearningOptions> options,
    ILogger<DiscardCardRegressionModelTrainer> logger)
    : RegressionModelTrainerBase<DiscardCardTrainingData>(mlContext, dataSplitter, persistenceService, options, logger)
{
    protected override IEstimator<ITransformer> BuildPipeline(IDataView trainingData)
    {
        var featureColumns = FeatureColumnProvider.GetFeatureColumns<DiscardCardTrainingData>();

        return MlContext.Transforms
            .Concatenate("Features", featureColumns)
            .AppendCacheCheckpoint(MlContext)
            .Append(MlContext.Regression.Trainers.LightGbm(
                labelColumnName: "Label",
                featureColumnName: "Features",
                numberOfLeaves: Options.NumberOfLeaves,
                minimumExampleCountPerLeaf: Options.MinimumExampleCountPerLeaf,
                learningRate: Options.LearningRate,
                numberOfIterations: Options.NumberOfIterations));
    }

    protected override string GetModelType()
    {
        return "DiscardCardRegression";
    }
}
