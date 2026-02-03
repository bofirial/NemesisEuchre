using Microsoft.ML;

namespace NemesisEuchre.MachineLearning.Models;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Records")]
public record TrainingResult(
    ITransformer Model,
    object ValidationMetrics,
    int TrainingSamples,
    int ValidationSamples,
    int TestSamples);
