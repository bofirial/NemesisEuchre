namespace NemesisEuchre.MachineLearning.Models;

/// <summary>
/// Represents classification metrics for a single class.
/// </summary>
/// <param name="ClassLabel">The numeric label identifying the class.</param>
/// <param name="Precision">Precision score (TP / (TP + FP)), or NaN if no predictions for this class.</param>
/// <param name="Recall">Recall score (TP / (TP + FN)), or NaN if no actual examples of this class.</param>
/// <param name="F1Score">F1 score (harmonic mean of precision and recall), or NaN if undefined.</param>
/// <param name="Support">Number of actual examples in the test set for this class.</param>
public record PerClassMetrics(
    int ClassLabel,
    double Precision,
    double Recall,
    double F1Score,
    int Support);
