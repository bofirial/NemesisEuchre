namespace NemesisEuchre.MachineLearning.Models;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Records")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Code Smell", "S2368:Public methods should not have multidimensional array parameters", Justification = "Need this for Machine Learning")]
public record EvaluationMetrics(
    double MicroAccuracy,
    double MacroAccuracy,
    double LogLoss,
    double LogLossReduction,
    double[] PerClassLogLoss,
    int[][] ConfusionMatrix,
    PerClassMetrics[] PerClassMetrics);
