namespace NemesisEuchre.MachineLearning.Models;

public record TrainingDataSourceMetadata(
    string GenerationName,
    int GameCount,
    List<ActorInfo> Actors);
