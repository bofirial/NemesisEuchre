namespace NemesisEuchre.MachineLearning.Models;

/// <summary>
/// Comprehensive evaluation report for regression models.
/// </summary>
/// <param name="ModelType">Type of model evaluated (e.g., "CallTrumpRegression", "PlayCardRegression").</param>
/// <param name="EvaluationDate">UTC timestamp of evaluation.</param>
/// <param name="TestSamples">Total number of samples in the test set.</param>
/// <param name="RSquared">Coefficient of determination (R²).</param>
/// <param name="MeanAbsoluteError">Mean absolute error (MAE).</param>
/// <param name="RootMeanSquaredError">Root mean squared error (RMSE).</param>
/// <param name="MeanSquaredError">Mean squared error (MSE).</param>
/// <param name="LossFunction">Value of the loss function.</param>
public record RegressionEvaluationReport(
    string ModelType,
    DateTime EvaluationDate,
    int TestSamples,
    double RSquared,
    double MeanAbsoluteError,
    double RootMeanSquaredError,
    double MeanSquaredError,
    double LossFunction);
