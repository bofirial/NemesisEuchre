using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;

namespace NemesisEuchre.MachineLearning.Trainers;

public class CallTrumpModelTrainer(
    MLContext mlContext,
    IDataSplitter dataSplitter,
    IOptions<MachineLearningOptions> options,
    ILogger<CallTrumpModelTrainer> logger)
    : MulticlassModelTrainerBase<CallTrumpTrainingData>(mlContext, dataSplitter, options, logger)
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
        };

        return MlContext.Transforms
            .Concatenate("Features", featureColumns)
            .Append(MlContext.Transforms.Conversion.MapValueToKey("Label", "Label"))
            .AppendCacheCheckpoint(MlContext)
            .Append(MlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                labelColumnName: "Label",
                featureColumnName: "Features",
                l2Regularization: (float)Options.LearningRate,
                maximumNumberOfIterations: Options.TrainingIterations))
            .Append(MlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
    }

    protected override string GetModelType()
    {
        return "CallTrump";
    }

    protected override int GetNumberOfClasses()
    {
        return 11;
    }
}
