namespace NemesisEuchre.MachineLearning.Models;

public record ModelMetadata(
    string ModelType,
    string ModelName,
    DateTime TrainingDate,
    int TrainingSamples,
    int ValidationSamples,
    int TestSamples,
    HyperparametersMetadata Hyperparameters,
    object Metrics,
    string FeatureSchemaVersion,
    TrainingDataSourceMetadata? TrainingDataSource = null);
