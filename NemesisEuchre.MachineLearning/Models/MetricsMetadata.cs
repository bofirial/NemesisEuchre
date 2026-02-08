namespace NemesisEuchre.MachineLearning.Models;

public record MetricsMetadata(
    double MicroAccuracy,
    double MacroAccuracy,
    double LogLoss,
    double LogLossReduction);
