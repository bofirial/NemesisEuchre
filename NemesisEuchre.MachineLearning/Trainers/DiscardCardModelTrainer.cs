using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;

namespace NemesisEuchre.MachineLearning.Trainers;

public class DiscardCardModelTrainer(
    MLContext mlContext,
    IDataSplitter dataSplitter,
    IOptions<MachineLearningOptions> options,
    ILogger<DiscardCardModelTrainer> logger)
    : MulticlassModelTrainerBase<DiscardCardTrainingData>(mlContext, dataSplitter, options, logger)
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
        };

        return MlContext.Transforms
            .Concatenate("Features", featureColumns)
            .Append(MlContext.Transforms.Conversion.MapValueToKey("Label", "Label"))
            .AppendCacheCheckpoint(MlContext)

            // Using LightGbm for maximum accuracy on large datasets (4.5M+ samples).
            // See class documentation for algorithm trade-offs and alternatives.
            .Append(MlContext.MulticlassClassification.Trainers.LightGbm(
                labelColumnName: "Label",
                featureColumnName: "Features",
                numberOfLeaves: Options.NumberOfLeaves,
                minimumExampleCountPerLeaf: Options.MinimumExampleCountPerLeaf,
                learningRate: Options.LearningRate,
                numberOfIterations: Options.NumberOfIterations))
            .Append(MlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
    }

    protected override string GetModelType()
    {
        return "DiscardCard";
    }

    protected override int GetNumberOfClasses()
    {
        return 6;
    }
}
