using System.ComponentModel.DataAnnotations;

namespace NemesisEuchre.MachineLearning.Options;

/// <summary>
/// Configuration options for ML.NET model training using LightGbm algorithm.
/// </summary>
public class MachineLearningOptions
{
    [Required]
    public string ModelOutputPath { get; set; } = "models";

    /// <summary>
    /// Gets or sets the maximum number of leaves in one tree. Controls tree complexity and model capacity.
    /// Higher values increase accuracy but risk overfitting.
    /// </summary>
    [Range(2, 4096)]
    public int NumberOfLeaves { get; set; } = 511;

    /// <summary>
    /// Gets or sets the number of boosting iterations. More iterations can improve accuracy but increase training time.
    /// </summary>
    [Range(10, 2500)]
    public int NumberOfIterations { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the learning rate for gradient boosting. Controls step size during optimization.
    /// Lower values require more iterations but may achieve better accuracy.
    /// </summary>
    [Range(0.01, 2.0)]
    public double LearningRate { get; set; } = 0.05;

    /// <summary>
    /// Gets or sets the minimum number of samples required in a leaf node. Prevents overfitting on small sample counts.
    /// </summary>
    [Range(1, 1000)]
    public int MinimumExampleCountPerLeaf { get; set; } = 400;

    /// <summary>
    /// Gets or sets the L1 regularization term. Promotes sparsity by driving unimportant feature contributions toward zero.
    /// Set to 0 to disable.
    /// </summary>
    [Range(0.0f, 5.0f)]
    public float L1Regularization { get; set; } = 0.3f;

    /// <summary>
    /// Gets or sets the L2 regularization term. Penalizes large leaf weights to stabilize the model.
    /// Matches the LightGBM default of 0.01.
    /// </summary>
    [Range(0.0f, 5.0f)]
    public float L2Regularization { get; set; } = 0.01f;

    /// <summary>
    /// Gets or sets the number of rounds with no improvement before early stopping triggers.
    /// Set to 0 to disable early stopping.
    /// </summary>
    [Range(0, 500)]
    public int EarlyStoppingRound { get; set; }

    [Range(0.001f, 5.0f)]
    public float ExplorationTemperature { get; set; } = 0.2f;

    [Range(0, int.MaxValue)]
    public int RandomSeed { get; set; } = 42;

    /// <summary>
    /// Gets or sets the maximum number of training rows to use. Set to 0 for unlimited.
    /// Reduces memory usage when training from very large IDV files.
    /// Default of 700M targets ~45-50 GB RAM on a 64 GB machine
    /// (70% train split × 700M × ~75 bytes/row ≈ 37 GB LightGBM dataset + ~12 GB overhead).
    /// </summary>
    [Range(0, long.MaxValue)]
    public long MaxTrainingRows { get; set; } = 700_000_000;
}
