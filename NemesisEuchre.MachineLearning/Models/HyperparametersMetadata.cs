namespace NemesisEuchre.MachineLearning.Models;

public record HyperparametersMetadata(
    string Algorithm,
    int NumberOfLeaves,
    int NumberOfIterations,
    double LearningRate,
    int MinimumExampleCountPerLeaf,
    float L1Regularization,
    float L2Regularization,
    int EarlyStoppingRound,
    int RandomSeed);
