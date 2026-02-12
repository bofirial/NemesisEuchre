using FluentAssertions;

using Microsoft.ML;

using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Options;

namespace NemesisEuchre.MachineLearning.Tests.DataAccess;

public class DataSplitterTests
{
    private readonly MLContext _mlContext;

    public DataSplitterTests()
    {
        _mlContext = new MLContext(seed: 42);
    }

    [Fact]
    public void Split_IDataView_WithNullDataView_ThrowsArgumentNullException()
    {
        var splitter = CreateSplitter();
        var act = () => splitter.Split(dataView: null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("dataView");
    }

    [Fact]
    public void Split_IDataView_WithEmptyDataView_ThrowsInvalidOperationException()
    {
        var splitter = CreateSplitter();
        var emptyDataView = _mlContext.Data.LoadFromEnumerable(Enumerable.Empty<TestData>());

        var act = () => splitter.Split(emptyDataView);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Split_IDataView_WithInsufficientData_ThrowsInvalidOperationException()
    {
        var splitter = CreateSplitter();
        var twoItems = new List<TestData>
        {
            new() { Feature1 = 1f, Feature2 = 2f, Label = 3f },
            new() { Feature1 = 4f, Feature2 = 5f, Label = 6f },
        };
        var dataView = _mlContext.Data.LoadFromEnumerable(twoItems);

        var act = () => splitter.Split(dataView);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(-0.1, 0.15, 0.95)]
    [InlineData(0.7, -0.1, 0.4)]
    [InlineData(0.7, 0.15, -0.15)]
    public void Split_IDataView_WithNegativeRatio_ThrowsArgumentException(double trainRatio, double validationRatio, double testRatio)
    {
        var splitter = CreateSplitter();
        var dataView = CreateTestDataView(100);

        var act = () => splitter.Split(dataView, trainRatio, validationRatio, testRatio);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*ratio must be between 0 and 1*");
    }

    [Theory]
    [InlineData(0.5, 0.3, 0.1)]
    [InlineData(0.8, 0.1, 0.05)]
    [InlineData(0.33, 0.33, 0.33)]
    public void Split_IDataView_WithRatiosSumNotEqualToOne_ThrowsArgumentException(double trainRatio, double validationRatio, double testRatio)
    {
        var splitter = CreateSplitter();
        var dataView = CreateTestDataView(100);

        var act = () => splitter.Split(dataView, trainRatio, validationRatio, testRatio);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Split ratios must sum to 1.0*");
    }

    [Fact]
    public void Split_IDataView_ProducesCorrectSplitRatios()
    {
        var splitter = CreateSplitter();
        var dataView = CreateTestDataView(1000);

        var result = splitter.Split(dataView);

        result.TrainRowCount.Should().BeInRange(650, 750);
        result.ValidationRowCount.Should().BeInRange(100, 200);
        result.TestRowCount.Should().BeInRange(100, 200);
        (result.TrainRowCount + result.ValidationRowCount + result.TestRowCount).Should().Be(1000);
    }

    [Fact]
    public void Split_IDataView_WithDefaultTrainRatio_Uses70PercentForTraining()
    {
        var splitter = CreateSplitter();
        var dataView = CreateTestDataView(100);

        var result = splitter.Split(dataView);

        result.TrainRowCount.Should().BeInRange(63, 77);
    }

    [Fact]
    public void Split_IDataView_WithPreShuffledTrue_SkipsShuffling()
    {
        var splitter = CreateSplitter();
        var dataView = CreateTestDataView(100);

        var result = splitter.Split(dataView, preShuffled: true);

        result.TrainRowCount.Should().BeInRange(63, 77);
        (result.TrainRowCount + result.ValidationRowCount + result.TestRowCount).Should().Be(100);
    }

    [Fact]
    public void Split_IDataView_WithCustomRatios_ReturnsCorrectSplitCounts()
    {
        var splitter = CreateSplitter();
        var dataView = CreateTestDataView(200);

        var result = splitter.Split(dataView, trainRatio: 0.6, validationRatio: 0.2, testRatio: 0.2);

        result.TrainRowCount.Should().BeInRange(110, 130);
        (result.TrainRowCount + result.ValidationRowCount + result.TestRowCount).Should().Be(200);
    }

    [Fact]
    public void Split_IDataView_ReturnsEnumerableDataViews()
    {
        var splitter = CreateSplitter();
        var dataView = CreateTestDataView(50);

        var result = splitter.Split(dataView);

        result.Train.Should().NotBeNull();
        result.Validation.Should().NotBeNull();
        result.Test.Should().NotBeNull();
        result.TrainRowCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Split_IDataView_WithSameSeed_ProducesSameSplit()
    {
        var data = Enumerable.Range(0, 100).Select(i => new TestData
        {
            Feature1 = i,
            Feature2 = i * 2,
            Label = i * 0.5f,
        }).ToList();

        var splitter1 = CreateSplitter(randomSeed: 123);
        var dataView1 = _mlContext.Data.LoadFromEnumerable(data);
        var result1 = splitter1.Split(dataView1);

        var splitter2 = CreateSplitter(randomSeed: 123);
        var dataView2 = _mlContext.Data.LoadFromEnumerable(data);
        var result2 = splitter2.Split(dataView2);

        result1.TrainRowCount.Should().Be(result2.TrainRowCount);
        result1.ValidationRowCount.Should().Be(result2.ValidationRowCount);
        result1.TestRowCount.Should().Be(result2.TestRowCount);
    }

    private IDataView CreateTestDataView(int count)
    {
        var data = Enumerable.Range(0, count).Select(i => new TestData
        {
            Feature1 = i,
            Feature2 = i * 2,
            Label = i * 0.5f,
        }).ToList();
        return _mlContext.Data.LoadFromEnumerable(data);
    }

    private DataSplitter CreateSplitter(int randomSeed = 42)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new MachineLearningOptions
        {
            RandomSeed = randomSeed,
            ModelOutputPath = "models",
            NumberOfLeaves = 31,
            NumberOfIterations = 200,
            LearningRate = 0.1,
            MinimumExampleCountPerLeaf = 20,
        });

        return new DataSplitter(_mlContext, options);
    }

    private sealed class TestData
    {
        [Microsoft.ML.Data.LoadColumn(0)]
        public float Feature1 { get; set; }

        [Microsoft.ML.Data.LoadColumn(1)]
        public float Feature2 { get; set; }

        [Microsoft.ML.Data.LoadColumn(2)]
        public float Label { get; set; }
    }
}
