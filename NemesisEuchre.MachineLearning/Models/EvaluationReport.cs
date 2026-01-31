namespace NemesisEuchre.MachineLearning.Models;

/// <summary>
/// Comprehensive evaluation report combining overall and per-class metrics.
/// </summary>
/// <param name="ModelType">Type of model evaluated (e.g., "PlayCard", "CallTrump").</param>
/// <param name="EvaluationDate">UTC timestamp of evaluation.</param>
/// <param name="TestSamples">Total number of samples in the test set.</param>
/// <param name="Overall">Aggregate metrics across all classes.</param>
/// <param name="PerClass">Per-class precision, recall, F1, and support.</param>
/// <param name="ConfusionMatrix">Confusion matrix used for evaluation.</param>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Records")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Code Smell", "S2368:Public methods should not have multidimensional array parameters", Justification = "Need this for Machine Learning")]
public record EvaluationReport(
    string ModelType,
    DateTime EvaluationDate,
    int TestSamples,
    OverallMetrics Overall,
    PerClassMetrics[] PerClass,
    int[][] ConfusionMatrix);

/// <summary>
/// Overall classification metrics aggregated across all classes.
/// </summary>
/// <param name="MicroAccuracy">Accuracy calculated from total TP/TN/FP/FN.</param>
/// <param name="MacroAccuracy">Average accuracy per class.</param>
/// <param name="LogLoss">Cross-entropy loss.</param>
/// <param name="LogLossReduction">Reduction in log loss compared to baseline.</param>
/// <param name="WeightedPrecision">Precision averaged across classes weighted by support.</param>
/// <param name="WeightedRecall">Recall averaged across classes weighted by support.</param>
/// <param name="WeightedF1Score">F1 score averaged across classes weighted by support.</param>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Records")]
public record OverallMetrics(
    double MicroAccuracy,
    double MacroAccuracy,
    double LogLoss,
    double LogLossReduction,
    double WeightedPrecision,
    double WeightedRecall,
    double WeightedF1Score);
