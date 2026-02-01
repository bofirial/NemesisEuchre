using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.MachineLearning.Trainers;

public class PlayCardRegressionModelTrainer(
    MLContext mlContext,
    IDataSplitter dataSplitter,
    IModelVersionManager versionManager,
    IOptions<MachineLearningOptions> options,
    ILogger<PlayCardRegressionModelTrainer> logger)
    : RegressionModelTrainerBase<PlayCardTrainingData>(mlContext, dataSplitter, versionManager, options, logger)
{
    protected override IEstimator<ITransformer> BuildPipeline(IDataView trainingData)
    {
        var featureColumns = new[]
        {
            nameof(PlayCardTrainingData.Card1Rank),
            nameof(PlayCardTrainingData.Card1Suit),
            nameof(PlayCardTrainingData.Card2Rank),
            nameof(PlayCardTrainingData.Card2Suit),
            nameof(PlayCardTrainingData.Card3Rank),
            nameof(PlayCardTrainingData.Card3Suit),
            nameof(PlayCardTrainingData.Card4Rank),
            nameof(PlayCardTrainingData.Card4Suit),
            nameof(PlayCardTrainingData.Card5Rank),
            nameof(PlayCardTrainingData.Card5Suit),
            nameof(PlayCardTrainingData.LeadPlayer),
            nameof(PlayCardTrainingData.LeadSuit),
            nameof(PlayCardTrainingData.PlayedCard1Rank),
            nameof(PlayCardTrainingData.PlayedCard1Suit),
            nameof(PlayCardTrainingData.PlayedCard2Rank),
            nameof(PlayCardTrainingData.PlayedCard2Suit),
            nameof(PlayCardTrainingData.PlayedCard3Rank),
            nameof(PlayCardTrainingData.PlayedCard3Suit),
            nameof(PlayCardTrainingData.TeamScore),
            nameof(PlayCardTrainingData.OpponentScore),
            nameof(PlayCardTrainingData.TrickNumber),
            nameof(PlayCardTrainingData.CardsPlayedInTrick),
            nameof(PlayCardTrainingData.WinningTrickPlayer),
            nameof(PlayCardTrainingData.Card1IsValid),
            nameof(PlayCardTrainingData.Card2IsValid),
            nameof(PlayCardTrainingData.Card3IsValid),
            nameof(PlayCardTrainingData.Card4IsValid),
            nameof(PlayCardTrainingData.Card5IsValid),
            nameof(PlayCardTrainingData.CallingPlayerPosition),
            nameof(PlayCardTrainingData.CallingPlayerGoingAlone),
            nameof(PlayCardTrainingData.Card1Chosen),
            nameof(PlayCardTrainingData.Card2Chosen),
            nameof(PlayCardTrainingData.Card3Chosen),
            nameof(PlayCardTrainingData.Card4Chosen),
            nameof(PlayCardTrainingData.Card5Chosen),
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
        return "PlayCardRegression";
    }
}
