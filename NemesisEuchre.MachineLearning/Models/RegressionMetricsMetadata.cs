namespace NemesisEuchre.MachineLearning.Models;

public record RegressionMetricsMetadata(
    double RSquared,
    double MeanAbsoluteError,
    double RootMeanSquaredError,
    double MeanSquaredError);
