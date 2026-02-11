namespace NemesisEuchre.MachineLearning.Services;

public record ModelFileInfo(
    string FilePath,
    string MetadataPath,
    string ModelName,
    string DecisionType);
