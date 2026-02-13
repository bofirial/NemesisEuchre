namespace NemesisEuchre.Console.Models;

public record TrainingProgress(
    string ModelType,
    TrainingPhase Phase,
    int PercentComplete,
    string? Message = null,
    double? ValidationMae = null,
    double? ValidationRSquared = null);
