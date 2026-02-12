namespace NemesisEuchre.MachineLearning.Models;

public record TrainingIterationUpdate(
    int CurrentIteration,
    int TotalIterations,
    double? TrainingMetric);
