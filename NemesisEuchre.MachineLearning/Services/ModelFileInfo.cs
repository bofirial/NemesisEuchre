namespace NemesisEuchre.MachineLearning.Services;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Records")]
public record ModelFileInfo(
    string FilePath,
    string MetadataPath,
    int Generation,
    string DecisionType,
    int Version);
