using System.ComponentModel.DataAnnotations;

namespace NemesisEuchre.MachineLearning.Options;

/// <summary>
/// Configuration options for ML.NET model training using LightGbm algorithm.
/// </summary>
public class MachineLearningOptions
{
    [Required]
    public string ModelOutputPath { get; set; } = "./models";

    /// <summary>
    /// Gets or sets the maximum number of leaves in one tree. Controls tree complexity and model capacity.
    /// Higher values increase accuracy but risk overfitting.
    /// </summary>
    [Range(2, 1024)]
    public int NumberOfLeaves { get; set; } = 31;

    /// <summary>
    /// Gets or sets the number of boosting iterations. More iterations can improve accuracy but increase training time.
    /// </summary>
    [Range(10, 500)]
    public int NumberOfIterations { get; set; } = 200;

    /// <summary>
    /// Gets or sets the learning rate for gradient boosting. Controls step size during optimization.
    /// Lower values require more iterations but may achieve better accuracy.
    /// </summary>
    [Range(0.01, 0.3)]
    public double LearningRate { get; set; } = 0.1;

    /// <summary>
    /// Gets or sets the minimum number of samples required in a leaf node. Prevents overfitting on small sample counts.
    /// </summary>
    [Range(1, 1000)]
    public int MinimumExampleCountPerLeaf { get; set; } = 20;

    [Range(0, int.MaxValue)]
    public int RandomSeed { get; set; } = 42;
}
