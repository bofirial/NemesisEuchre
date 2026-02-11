namespace NemesisEuchre.Console.Models;

public record ModelTrainingResult(
    string ModelType,
    bool Success,
    string? ModelPath = null,
    string? ErrorMessage = null,
    double? MeanAbsoluteError = null,
    double? RSquared = null,
    TimeSpan? Duration = null);
