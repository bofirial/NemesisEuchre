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

        var dataList = data.ToList();

        if (dataList.Count == 0)
        {
            throw new InvalidOperationException("Cannot split empty dataset.");
        }

        if (dataList.Count < 3)
        {
            throw new InvalidOperationException($"Dataset must contain at least 3 samples for splitting. Found {dataList.Count} samples.");
        }

        Shuffle(dataList, options!.Value!.RandomSeed);

        var totalCount = dataList.Count;
        var trainCount = (int)Math.Floor(totalCount * trainRatio);
        var validationCount = (int)Math.Floor(totalCount * validationRatio);
        var testCount = totalCount - trainCount - validationCount;

        var trainData = dataList.Take(trainCount).ToList();
        var validationData = dataList.Skip(trainCount).Take(validationCount).ToList();
        var testData = dataList.Skip(trainCount + validationCount).Take(testCount).ToList();

        var trainDataView = mlContext.Data.LoadFromEnumerable(trainData);
        var validationDataView = mlContext.Data.LoadFromEnumerable(validationData);
        var testDataView = mlContext.Data.LoadFromEnumerable(testData);

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

    private static void Shuffle<T>(List<T> list, int seed)
    {
        var random = new Random(seed);
        var n = list.Count;

        for (var i = n - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
