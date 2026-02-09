using Microsoft.ML;

namespace NemesisEuchre.MachineLearning.Models;

public record TrainingResult(
    ITransformer Model,
    object ValidationMetrics,
    int TrainingSamples,
    int ValidationSamples,
    int TestSamples);
