using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

using NemesisEuchre.MachineLearning.Options;

namespace NemesisEuchre.Console.Options;

public class MergedMachineLearningOptions : IOptions<MachineLearningOptions>
{
    public MergedMachineLearningOptions(
        MachineLearningOptions baseOptions,
        int? numberOfIterations = null,
        double? learningRate = null,
        int? numberOfLeaves = null,
        int? minimumExampleCountPerLeaf = null,
        long? maxTrainingRows = null,
        float? l1Regularization = null,
        float? l2Regularization = null,
        int? earlyStoppingRound = null)
    {
        Value = new MachineLearningOptions
        {
            ModelOutputPath = baseOptions.ModelOutputPath,
            NumberOfIterations = numberOfIterations ?? baseOptions.NumberOfIterations,
            LearningRate = learningRate ?? baseOptions.LearningRate,
            NumberOfLeaves = numberOfLeaves ?? baseOptions.NumberOfLeaves,
            MinimumExampleCountPerLeaf = minimumExampleCountPerLeaf ?? baseOptions.MinimumExampleCountPerLeaf,
            ExplorationTemperature = baseOptions.ExplorationTemperature,
            RandomSeed = baseOptions.RandomSeed,
            MaxTrainingRows = maxTrainingRows ?? baseOptions.MaxTrainingRows,
            L1Regularization = l1Regularization ?? baseOptions.L1Regularization,
            L2Regularization = l2Regularization ?? baseOptions.L2Regularization,
            EarlyStoppingRound = earlyStoppingRound ?? baseOptions.EarlyStoppingRound,
        };

        var validationContext = new ValidationContext(Value);
        Validator.ValidateObject(Value, validationContext, validateAllProperties: true);
    }

    public MachineLearningOptions Value { get; }
}
