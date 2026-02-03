namespace NemesisEuchre.Console.Models;

public enum TrainingPhase
{
    LoadingData = 0,
    Training = 1,
    Saving = 2,
    Complete = 3,
    Failed = 4,
}
