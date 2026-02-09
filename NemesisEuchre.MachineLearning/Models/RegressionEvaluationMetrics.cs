namespace NemesisEuchre.MachineLearning.Models;

public record RegressionEvaluationMetrics(
    double RSquared,
    double MeanAbsoluteError,
    double RootMeanSquaredError,
    double MeanSquaredError,
    double LossFunction);
