using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.MachineLearning.Options;

namespace NemesisEuchre.MachineLearning.DataAccess;

public interface IDataSplitter
{
    DataSplit Split<T>(
        IEnumerable<T> data,
        double trainRatio = 0.7,
        double validationRatio = 0.15,
        double testRatio = 0.15)
        where T : class;
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Not Records")]
public record DataSplit(
    IDataView Train,
    IDataView Validation,
    IDataView Test,
    int TrainRowCount,
    int ValidationRowCount,
    int TestRowCount);

public class DataSplitter(MLContext mlContext, IOptions<MachineLearningOptions> options) : IDataSplitter
{
    public DataSplit Split<T>(
        IEnumerable<T> data,
        double trainRatio = 0.7,
        double validationRatio = 0.15,
        double testRatio = 0.15)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(data);

        ValidateRatios(trainRatio, validationRatio, testRatio);

        var dataView = mlContext.Data.LoadFromEnumerable(data);

        var rowCount = (int)(dataView.GetRowCount() ?? 0);

        if (rowCount == 0)
        {
            throw new InvalidOperationException("Cannot split empty dataset.");
        }

        if (rowCount < 3)
        {
            throw new InvalidOperationException($"Dataset must contain at least 3 samples for splitting. Found {rowCount} samples.");
        }

        var shuffledData = mlContext.Data.ShuffleRows(dataView, seed: options!.Value!.RandomSeed);

        var trainFraction = trainRatio;
        var firstSplit = mlContext.Data.TrainTestSplit(shuffledData, testFraction: 1.0 - trainFraction, seed: options!.Value!.RandomSeed);
        var trainDataView = firstSplit.TrainSet;
        var remaining = firstSplit.TestSet;

        var validationFractionOfRemaining = validationRatio / (validationRatio + testRatio);
        var secondSplit = mlContext.Data.TrainTestSplit(remaining, testFraction: 1.0 - validationFractionOfRemaining, seed: options!.Value!.RandomSeed);
        var validationDataView = secondSplit.TrainSet;
        var testDataView = secondSplit.TestSet;

        var trainCount = CountRows(trainDataView);
        var validationCount = CountRows(validationDataView);
        var testCount = CountRows(testDataView);

        return new DataSplit(
            trainDataView,
            validationDataView,
            testDataView,
            trainCount,
            validationCount,
            testCount);
    }

    private static void ValidateRatios(double trainRatio, double validationRatio, double testRatio)
    {
        if (trainRatio is < 0 or > 1)
        {
            throw new ArgumentException($"Train ratio must be between 0 and 1. Got {trainRatio}.", nameof(trainRatio));
        }

        if (validationRatio is < 0 or > 1)
        {
            throw new ArgumentException($"Validation ratio must be between 0 and 1. Got {validationRatio}.", nameof(validationRatio));
        }

        if (testRatio is < 0 or > 1)
        {
            throw new ArgumentException($"Test ratio must be between 0 and 1. Got {testRatio}.", nameof(testRatio));
        }

        var sum = trainRatio + validationRatio + testRatio;
        const double tolerance = 1e-6;

        if (Math.Abs(sum - 1.0) > tolerance)
        {
            throw new ArgumentException($"Split ratios must sum to 1.0 (±{tolerance}). Got {sum}.");
        }
    }

    private static int CountRows(IDataView dataView)
    {
        var cursor = dataView.GetRowCursor(dataView.Schema);
        int count = 0;
        while (cursor.MoveNext())
        {
            count++;
        }

        return count;
    }
}
