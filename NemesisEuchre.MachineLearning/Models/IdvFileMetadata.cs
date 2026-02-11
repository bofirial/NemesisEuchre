using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.MachineLearning.Models;

public record IdvFileMetadata(
    string GenerationName,
    DecisionType DecisionType,
    int RowCount,
    int GameCount,
    int DealCount,
    int TrickCount,
    List<ActorInfo> Actors,
    DateTime CreatedAtUtc);

public record ActorInfo(
    ActorType ActorType,
    string? ModelName,
    float ExplorationTemperature);
