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
    double? ValidationMae,
    double? ValidationRSquared,
    TimeSpan Elapsed);
