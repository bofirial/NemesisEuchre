namespace NemesisEuchre.Console.Models;

public record TrainingDisplaySnapshot(
    IReadOnlyList<ModelDisplayInfo> Models,
    int TotalModels,
    int CompletedModels);

public record ModelDisplayInfo(
    string ModelType,
    TrainingPhase Phase,
    int PercentComplete,
    string? Message,
    int? CurrentIteration,
    int? TotalIterations,
    double? TrainingMetric,
    double? ValidationMae,
    double? ValidationRSquared,
    TimeSpan Elapsed);
