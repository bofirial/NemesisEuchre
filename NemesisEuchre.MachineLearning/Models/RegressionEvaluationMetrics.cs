namespace NemesisEuchre.MachineLearning.Models;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Records")]
public record RegressionEvaluationMetrics(
    double RSquared,
    double MeanAbsoluteError,
    double RootMeanSquaredError,
    double MeanSquaredError,
    double LossFunction);
