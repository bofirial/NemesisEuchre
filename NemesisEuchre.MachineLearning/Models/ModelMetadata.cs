namespace NemesisEuchre.MachineLearning.Models;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Records")]
public record ModelMetadata(
    string ModelType,
    DateTime TrainingDate,
    int TrainingSamples,
    int ValidationSamples,
    int TestSamples,
    HyperparametersMetadata Hyperparameters,
    MetricsMetadata Metrics,
    string FeatureSchemaVersion);

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Records")]
public record HyperparametersMetadata(
    string Algorithm,
    int NumberOfLeaves,
    int NumberOfIterations,
    double LearningRate,
    int RandomSeed);

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Records")]
public record MetricsMetadata(
    double MicroAccuracy,
    double MacroAccuracy,
    double LogLoss,
    double LogLossReduction);
