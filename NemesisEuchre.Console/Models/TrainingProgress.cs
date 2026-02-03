namespace NemesisEuchre.Console.Models;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Records")]
public record TrainingProgress(
    string ModelType,
    TrainingPhase Phase,
    int PercentComplete,
    string? Message = null);
