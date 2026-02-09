namespace NemesisEuchre.MachineLearning.Models;

public record HyperparametersMetadata(
    string Algorithm,
    int NumberOfLeaves,
    int NumberOfIterations,
    double LearningRate,
    int RandomSeed);
