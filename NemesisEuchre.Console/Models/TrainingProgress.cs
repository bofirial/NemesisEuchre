namespace NemesisEuchre.Console.Models;

public record TrainingProgress(
    string ModelType,
    TrainingPhase Phase,
    int PercentComplete,
    string? Message = null,
    int? CurrentIteration = null,
    int? TotalIterations = null,
    double? TrainingMetric = null,
    double? ValidationMae = null,
    double? ValidationRSquared = null);
