namespace NemesisEuchre.MachineLearning.Services;

public record ModelFileInfo(
    string FilePath,
    string MetadataPath,
    int Generation,
    string DecisionType,
    int Version);
