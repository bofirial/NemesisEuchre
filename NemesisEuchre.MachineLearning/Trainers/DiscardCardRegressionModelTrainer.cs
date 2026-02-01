using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.MachineLearning.Trainers;

public class DiscardCardRegressionModelTrainer(
    MLContext mlContext,
    IDataSplitter dataSplitter,
    IModelVersionManager versionManager,
    IOptions<MachineLearningOptions> options,
    ILogger<DiscardCardRegressionModelTrainer> logger)
    : RegressionModelTrainerBase<DiscardCardTrainingData>(mlContext, dataSplitter, versionManager, options, logger)
{
    protected override IEstimator<ITransformer> BuildPipeline(IDataView trainingData)
    {
        var featureColumns = new[]
        {
            nameof(DiscardCardTrainingData.Card1Rank),
            nameof(DiscardCardTrainingData.Card1Suit),
            nameof(DiscardCardTrainingData.Card2Rank),
            nameof(DiscardCardTrainingData.Card2Suit),
            nameof(DiscardCardTrainingData.Card3Rank),
            nameof(DiscardCardTrainingData.Card3Suit),
            nameof(DiscardCardTrainingData.Card4Rank),
            nameof(DiscardCardTrainingData.Card4Suit),
            nameof(DiscardCardTrainingData.Card5Rank),
            nameof(DiscardCardTrainingData.Card5Suit),
            nameof(DiscardCardTrainingData.Card6Rank),
            nameof(DiscardCardTrainingData.Card6Suit),
            nameof(DiscardCardTrainingData.CallingPlayerPosition),
            nameof(DiscardCardTrainingData.CallingPlayerGoingAlone),
            nameof(DiscardCardTrainingData.TeamScore),
            nameof(DiscardCardTrainingData.OpponentScore),
            nameof(DiscardCardTrainingData.Card1Chosen),
            nameof(DiscardCardTrainingData.Card2Chosen),
            nameof(DiscardCardTrainingData.Card3Chosen),
            nameof(DiscardCardTrainingData.Card4Chosen),
            nameof(DiscardCardTrainingData.Card5Chosen),
            nameof(DiscardCardTrainingData.Card6Chosen),
        };

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
