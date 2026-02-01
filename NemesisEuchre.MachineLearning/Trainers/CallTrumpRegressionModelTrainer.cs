using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.MachineLearning.Trainers;

public class CallTrumpRegressionModelTrainer(
    MLContext mlContext,
    IDataSplitter dataSplitter,
    IModelVersionManager versionManager,
    IOptions<MachineLearningOptions> options,
    ILogger<CallTrumpRegressionModelTrainer> logger)
    : RegressionModelTrainerBase<CallTrumpTrainingData>(mlContext, dataSplitter, versionManager, options, logger)
{
    protected override IEstimator<ITransformer> BuildPipeline(IDataView trainingData)
    {
        var featureColumns = new[]
        {
            nameof(CallTrumpTrainingData.Card1Rank),
            nameof(CallTrumpTrainingData.Card1Suit),
            nameof(CallTrumpTrainingData.Card2Rank),
            nameof(CallTrumpTrainingData.Card2Suit),
            nameof(CallTrumpTrainingData.Card3Rank),
            nameof(CallTrumpTrainingData.Card3Suit),
            nameof(CallTrumpTrainingData.Card4Rank),
            nameof(CallTrumpTrainingData.Card4Suit),
            nameof(CallTrumpTrainingData.Card5Rank),
            nameof(CallTrumpTrainingData.Card5Suit),
            nameof(CallTrumpTrainingData.UpCardRank),
            nameof(CallTrumpTrainingData.UpCardSuit),
            nameof(CallTrumpTrainingData.DealerPosition),
            nameof(CallTrumpTrainingData.TeamScore),
            nameof(CallTrumpTrainingData.OpponentScore),
            nameof(CallTrumpTrainingData.DecisionOrder),
            nameof(CallTrumpTrainingData.Decision0IsValid),
            nameof(CallTrumpTrainingData.Decision1IsValid),
            nameof(CallTrumpTrainingData.Decision2IsValid),
            nameof(CallTrumpTrainingData.Decision3IsValid),
            nameof(CallTrumpTrainingData.Decision4IsValid),
            nameof(CallTrumpTrainingData.Decision5IsValid),
            nameof(CallTrumpTrainingData.Decision6IsValid),
            nameof(CallTrumpTrainingData.Decision7IsValid),
            nameof(CallTrumpTrainingData.Decision8IsValid),
            nameof(CallTrumpTrainingData.Decision9IsValid),
            nameof(CallTrumpTrainingData.Decision10IsValid),
            nameof(CallTrumpTrainingData.Decision0Chosen),
            nameof(CallTrumpTrainingData.Decision1Chosen),
            nameof(CallTrumpTrainingData.Decision2Chosen),
            nameof(CallTrumpTrainingData.Decision3Chosen),
            nameof(CallTrumpTrainingData.Decision4Chosen),
            nameof(CallTrumpTrainingData.Decision5Chosen),
            nameof(CallTrumpTrainingData.Decision6Chosen),
            nameof(CallTrumpTrainingData.Decision7Chosen),
            nameof(CallTrumpTrainingData.Decision8Chosen),
            nameof(CallTrumpTrainingData.Decision9Chosen),
            nameof(CallTrumpTrainingData.Decision10Chosen),
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
        return "CallTrumpRegression";
    }
}
