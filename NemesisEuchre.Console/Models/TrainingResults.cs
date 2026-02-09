namespace NemesisEuchre.Console.Models;

public record TrainingResults(
    int SuccessfulModels,
    int FailedModels,
    List<ModelTrainingResult> Results,
    TimeSpan TotalDuration);
