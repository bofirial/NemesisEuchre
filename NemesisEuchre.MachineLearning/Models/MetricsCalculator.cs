namespace NemesisEuchre.MachineLearning.Models;

/// <summary>
/// Utility class for calculating classification metrics from confusion matrices.
/// </summary>
public static class MetricsCalculator
{
    /// <summary>
    /// Calculates precision, recall, F1-score, and support for each class from a confusion matrix.
    /// </summary>
    /// <param name="confusionMatrix">Square confusion matrix where rows are actual classes and columns are predicted classes.</param>
    /// <returns>Array of per-class metrics, one entry per class.</returns>
    /// <exception cref="ArgumentNullException">Thrown when confusionMatrix is null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Code Smell", "S2368:Public methods should not have multidimensional array parameters", Justification = "Need this for Machine Learning")]
    public static PerClassMetrics[] CalculatePerClassMetrics(int[][] confusionMatrix)
    {
        ArgumentNullException.ThrowIfNull(confusionMatrix);

        int numClasses = confusionMatrix.Length;
        var metrics = new PerClassMetrics[numClasses];

        for (int i = 0; i < numClasses; i++)
        {
            int truePositives = confusionMatrix[i][i];
            int falseNegatives = 0;
            int falsePositives = 0;

            // Calculate FN: sum of row i excluding diagonal
            for (int j = 0; j < numClasses; j++)
            {
                if (j != i)
                {
                    falseNegatives += confusionMatrix[i][j];
                }
            }

            // Calculate FP: sum of column i excluding diagonal
            for (int j = 0; j < numClasses; j++)
            {
                if (j != i)
                {
                    falsePositives += confusionMatrix[j][i];
                }
            }

            int support = truePositives + falseNegatives;
            double precision = CalculatePrecision(truePositives, falsePositives);
            double recall = CalculateRecall(truePositives, falseNegatives);
            double f1Score = CalculateF1Score(precision, recall);

            metrics[i] = new PerClassMetrics(i, precision, recall, f1Score, support);
        }

        return metrics;
    }

    private static double CalculatePrecision(int truePositives, int falsePositives)
    {
        int totalPredicted = truePositives + falsePositives;
        return totalPredicted > 0 ? (double)truePositives / totalPredicted : double.NaN;
    }

    private static double CalculateRecall(int truePositives, int falseNegatives)
    {
        int totalActual = truePositives + falseNegatives;
        return totalActual > 0 ? (double)truePositives / totalActual : double.NaN;
    }

    private static double CalculateF1Score(double precision, double recall)
    {
        if (double.IsNaN(precision) || double.IsNaN(recall))
        {
            return double.NaN;
        }

        double sum = precision + recall;
        return sum > 0 ? 2 * precision * recall / sum : double.NaN;
    }
}
