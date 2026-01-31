using System.ComponentModel.DataAnnotations;

namespace NemesisEuchre.MachineLearning.Options;

public class MachineLearningOptions
{
    [Required]
    public string ModelOutputPath { get; set; } = "./models";

    [Range(1, 10000)]
    public int TrainingIterations { get; set; } = 100;

    [Range(0.0001, 1.0)]
    public double LearningRate { get; set; } = 0.1;

    [Range(0, int.MaxValue)]
    public int RandomSeed { get; set; } = 42;
}
