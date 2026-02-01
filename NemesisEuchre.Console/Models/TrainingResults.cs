namespace NemesisEuchre.Console.Models;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Records")]
public record TrainingResults(
    int SuccessfulModels,
    int FailedModels,
    List<ModelTrainingResult> Results,
    TimeSpan TotalDuration);

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Records")]
public record ModelTrainingResult(
    string ModelType,
    bool Success,
    string? ModelPath = null,
    string? ErrorMessage = null,
    double? MeanAbsoluteError = null,
    double? RSquared = null);
