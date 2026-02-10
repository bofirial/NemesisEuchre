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
        double testRatio = 0.15,
        bool preShuffled = false)
        where T : class;
}

public class DataSplitter : IDataSplitter
{
    private readonly MLContext _mlContext;
    private readonly MachineLearningOptions _options;

    public DataSplitter(MLContext mlContext, IOptions<MachineLearningOptions> options)
    {
        ArgumentNullException.ThrowIfNull(mlContext);
        ArgumentNullException.ThrowIfNull(options);

        _mlContext = mlContext;
        _options = options.Value ?? throw new ArgumentNullException(nameof(options), "Options value cannot be null");
    }

    public DataSplit Split<T>(
        IEnumerable<T> data,
        double trainRatio = 0.7,
        double validationRatio = 0.15,
        double testRatio = 0.15,
        bool preShuffled = false)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(data);

        ValidateRatios(trainRatio, validationRatio, testRatio);

        var materializedData = data as IList<T> ?? [.. data];
        var dataView = _mlContext.Data.LoadFromEnumerable(materializedData);

        var rowCount = (int)(dataView.GetRowCount() ?? 0);

        if (rowCount == 0)
        {
            throw new InvalidOperationException("Cannot split empty dataset.");
        }

        if (rowCount < 3)
        {
            throw new InvalidOperationException($"Dataset must contain at least 3 samples for splitting. Found {rowCount} samples.");
        }

        var shuffledData = preShuffled
            ? dataView
            : _mlContext.Data.ShuffleRows(dataView, seed: _options.RandomSeed);

        var trainFraction = trainRatio;
        var firstSplit = _mlContext.Data.TrainTestSplit(shuffledData, testFraction: 1.0 - trainFraction, seed: _options.RandomSeed);
        var trainDataView = firstSplit.TrainSet;
        var remaining = firstSplit.TestSet;

        var validationFractionOfRemaining = validationRatio / (validationRatio + testRatio);
        var secondSplit = _mlContext.Data.TrainTestSplit(remaining, testFraction: 1.0 - validationFractionOfRemaining, seed: _options.RandomSeed);
        var validationDataView = secondSplit.TrainSet;
        var testDataView = secondSplit.TestSet;

        var trainCount = GetRowCountOrFallback(trainDataView);
        var validationCount = GetRowCountOrFallback(validationDataView);
        var testCount = rowCount - trainCount - validationCount;

        return new DataSplit(
            trainDataView,
            validationDataView,
            testDataView,
            trainCount,
            validationCount,
            testCount);
    }

    private static int GetRowCountOrFallback(IDataView dataView)
    {
        var rowCount = dataView.GetRowCount();
        if (rowCount.HasValue)
        {
            return (int)rowCount.Value;
        }

        using var cursor = dataView.GetRowCursor(dataView.Schema);
        int count = 0;
        while (cursor.MoveNext())
        {
            count++;
        }

        return count;
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
}
