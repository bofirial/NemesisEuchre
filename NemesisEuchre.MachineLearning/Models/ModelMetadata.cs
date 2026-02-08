using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.MachineLearning.Models;

public record ModelMetadata(
    string ModelType,
    ActorType ActorType,
    int Generation,
    int Version,
    DateTime TrainingDate,
    int TrainingSamples,
    int ValidationSamples,
    int TestSamples,
    HyperparametersMetadata Hyperparameters,
    object Metrics,
    string FeatureSchemaVersion);
