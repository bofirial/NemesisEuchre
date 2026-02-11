namespace NemesisEuchre.MachineLearning.Models;

public record ModelMetadata(
    string ModelType,
    int Generation,
    int Version,
    DateTime TrainingDate,
    int TrainingSamples,
    int ValidationSamples,
    int TestSamples,
    HyperparametersMetadata Hyperparameters,
    object Metrics,
    string FeatureSchemaVersion);
