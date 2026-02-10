using Bogus;

using FluentAssertions;

using Microsoft.ML;

using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;

namespace NemesisEuchre.MachineLearning.Tests.DataAccess;

public class DataSplitterTests
{
    private readonly MLContext _mlContext;
    private readonly Faker<CallTrumpTrainingData> _trainingDataFaker;

    public DataSplitterTests()
    {
        _mlContext = new MLContext(seed: 42);
        _trainingDataFaker = new Faker<CallTrumpTrainingData>()
            .RuleFor(x => x.Card1Rank, f => f.Random.Float(0, 5))
            .RuleFor(x => x.Card1Suit, f => f.Random.Float(0, 3))
            .RuleFor(x => x.Card2Rank, f => f.Random.Float(0, 5))
            .RuleFor(x => x.Card2Suit, f => f.Random.Float(0, 3))
            .RuleFor(x => x.Card3Rank, f => f.Random.Float(0, 5))
            .RuleFor(x => x.Card3Suit, f => f.Random.Float(0, 3))
            .RuleFor(x => x.Card4Rank, f => f.Random.Float(0, 5))
            .RuleFor(x => x.Card4Suit, f => f.Random.Float(0, 3))
            .RuleFor(x => x.Card5Rank, f => f.Random.Float(0, 5))
            .RuleFor(x => x.Card5Suit, f => f.Random.Float(0, 3))
            .RuleFor(x => x.UpCardRank, f => f.Random.Float(0, 5))
            .RuleFor(x => x.UpCardSuit, f => f.Random.Float(0, 3))
            .RuleFor(x => x.DealerPosition, f => f.Random.Float(0, 3))
            .RuleFor(x => x.TeamScore, f => f.Random.Float(0, 10))
            .RuleFor(x => x.OpponentScore, f => f.Random.Float(0, 10))
            .RuleFor(x => x.DecisionOrder, f => f.Random.Float(0, 7))
            .RuleFor(x => x.Decision0IsValid, f => f.Random.Float(0, 1))
            .RuleFor(x => x.Decision1IsValid, f => f.Random.Float(0, 1))
            .RuleFor(x => x.Decision2IsValid, f => f.Random.Float(0, 1))
            .RuleFor(x => x.Decision3IsValid, f => f.Random.Float(0, 1))
            .RuleFor(x => x.Decision4IsValid, f => f.Random.Float(0, 1))
            .RuleFor(x => x.Decision5IsValid, f => f.Random.Float(0, 1))
            .RuleFor(x => x.Decision6IsValid, f => f.Random.Float(0, 1))
            .RuleFor(x => x.Decision7IsValid, f => f.Random.Float(0, 1))
            .RuleFor(x => x.Decision8IsValid, f => f.Random.Float(0, 1))
            .RuleFor(x => x.Decision9IsValid, f => f.Random.Float(0, 1))
            .RuleFor(x => x.Decision10IsValid, f => f.Random.Float(0, 1))
            .RuleFor(x => x.ExpectedDealPoints, f => (short)f.Random.Int(-2, 4));
    }

    [Fact]
    public void Split_WithNullData_ThrowsArgumentNullException()
    {
        var splitter = CreateSplitter();
        var act = () => splitter.Split<CallTrumpTrainingData>(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("data");
    }

    [Fact]
    public void Split_WithEmptyData_ThrowsInvalidOperationException()
    {
        var splitter = CreateSplitter();
        var data = new List<CallTrumpTrainingData>();
        var act = () => splitter.Split(data);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot split empty dataset.");
    }

    [Fact]
    public void Split_WithInsufficientData_ThrowsInvalidOperationException()
    {
        var splitter = CreateSplitter();
        var data = _trainingDataFaker.Generate(2);
        var act = () => splitter.Split(data);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Dataset must contain at least 3 samples for splitting. Found 2 samples.");
    }

    [Theory]
    [InlineData(-0.1, 0.15, 0.95)]
    [InlineData(0.7, -0.1, 0.4)]
    [InlineData(0.7, 0.15, -0.15)]
    public void Split_WithNegativeRatio_ThrowsArgumentException(double trainRatio, double validationRatio, double testRatio)
    {
        var splitter = CreateSplitter();
        var data = _trainingDataFaker.Generate(100);
        var act = () => splitter.Split(data, trainRatio, validationRatio, testRatio);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ratio must be between 0 and 1*");
    }

    [Theory]
    [InlineData(0.5, 0.3, 0.1)]
    [InlineData(0.8, 0.1, 0.05)]
    [InlineData(0.33, 0.33, 0.33)]
    public void Split_WithRatiosSumNotEqualToOne_ThrowsArgumentException(double trainRatio, double validationRatio, double testRatio)
    {
        var splitter = CreateSplitter();
        var data = _trainingDataFaker.Generate(100);
        var act = () => splitter.Split(data, trainRatio, validationRatio, testRatio);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Split ratios must sum to 1.0*");
    }

    [Fact]
    public void Split_With100Samples_ReturnsCorrectSplitCounts()
    {
        var splitter = CreateSplitter();
        var data = _trainingDataFaker.Generate(100);

        var result = splitter.Split(data);

        result.TrainRowCount.Should().BeInRange(63, 77);
        result.ValidationRowCount.Should().BeInRange(8, 22);
        result.TestRowCount.Should().BeInRange(8, 22);
        (result.TrainRowCount + result.ValidationRowCount + result.TestRowCount).Should().Be(100);
    }

    [Fact]
    public void Split_With1000Samples_ReturnsCorrectSplitCounts()
    {
        var splitter = CreateSplitter();
        var data = _trainingDataFaker.Generate(1000);

        var result = splitter.Split(data);

        result.TrainRowCount.Should().BeInRange(630, 770);
        result.ValidationRowCount.Should().BeInRange(80, 220);
        result.TestRowCount.Should().BeInRange(80, 220);
        (result.TrainRowCount + result.ValidationRowCount + result.TestRowCount).Should().Be(1000);
    }

    [Fact]
    public void Split_With97IndivisibleSamples_HandlesRemainder()
    {
        var splitter = CreateSplitter();
        var data = _trainingDataFaker.Generate(97);

        var result = splitter.Split(data);

        result.TrainRowCount.Should().BeInRange(60, 75);
        result.ValidationRowCount.Should().BeInRange(8, 21);
        result.TestRowCount.Should().BeInRange(8, 21);
        (result.TrainRowCount + result.ValidationRowCount + result.TestRowCount).Should().Be(97);
    }

    [Fact]
    public void Split_WithCustomRatios_ReturnsCorrectSplitCounts()
    {
        var splitter = CreateSplitter();
        var data = _trainingDataFaker.Generate(300);

        var result = splitter.Split(data, trainRatio: 0.6, validationRatio: 0.2, testRatio: 0.2);

        result.TrainRowCount.Should().BeInRange(162, 198);
        result.ValidationRowCount.Should().BeInRange(30, 90);
        result.TestRowCount.Should().BeInRange(30, 90);
        (result.TrainRowCount + result.ValidationRowCount + result.TestRowCount).Should().Be(300);
    }

    [Fact]
    public void Split_WithSameSeed_ProducesSameSplit()
    {
        var data = _trainingDataFaker.Generate(100);

        var splitter1 = CreateSplitter(randomSeed: 123);
        var result1 = splitter1.Split(data);

        var splitter2 = CreateSplitter(randomSeed: 123);
        var result2 = splitter2.Split(data);

        var train1 = _mlContext.Data.CreateEnumerable<CallTrumpTrainingData>(result1.Train, reuseRowObject: false).ToList();
        var train2 = _mlContext.Data.CreateEnumerable<CallTrumpTrainingData>(result2.Train, reuseRowObject: false).ToList();

        train1.Should().HaveCount(train2.Count);
        for (int i = 0; i < train1.Count; i++)
        {
            train1[i].Card1Rank.Should().Be(train2[i].Card1Rank);
            train1[i].ExpectedDealPoints.Should().Be(train2[i].ExpectedDealPoints);
        }
    }

    [Fact]
    public void Split_WithDifferentSeeds_ProducesDifferentSplits()
    {
        var data = _trainingDataFaker.Generate(100);

        var splitter1 = CreateSplitter(randomSeed: 123);
        var result1 = splitter1.Split(data);

        var splitter2 = CreateSplitter(randomSeed: 456);
        var result2 = splitter2.Split(data);

        var train1 = _mlContext.Data.CreateEnumerable<CallTrumpTrainingData>(result1.Train, reuseRowObject: false).Take(10).ToList();
        var train2 = _mlContext.Data.CreateEnumerable<CallTrumpTrainingData>(result2.Train, reuseRowObject: false).Take(10).ToList();

        var isDifferent = false;
        for (int i = 0; i < train1.Count; i++)
        {
            if (!train1[i].Card1Rank.Equals(train2[i].Card1Rank) ||
                !train1[i].ExpectedDealPoints.Equals(train2[i].ExpectedDealPoints))
            {
                isDifferent = true;
                break;
            }
        }

        isDifferent.Should().BeTrue("different seeds should produce different shuffles");
    }

    [Fact]
    public void Split_ReturnsIDataViewsWithCorrectSchema()
    {
        var splitter = CreateSplitter();
        var data = _trainingDataFaker.Generate(100);

        var result = splitter.Split(data);

        result.Train.Should().NotBeNull();
        result.Validation.Should().NotBeNull();
        result.Test.Should().NotBeNull();

        result.Train.Schema.Should().HaveCountGreaterThan(0);
        result.Train.Schema.Should().Contain(col => col.Name == "Card1Rank");
        result.Train.Schema.Should().Contain(col => col.Name == "Label");
    }

    [Fact]
    public void Split_IDataViewsAreEnumerable()
    {
        var splitter = CreateSplitter();
        var data = _trainingDataFaker.Generate(100);

        var result = splitter.Split(data);

        var trainData = _mlContext.Data.CreateEnumerable<CallTrumpTrainingData>(result.Train, reuseRowObject: false).ToList();
        var validationData = _mlContext.Data.CreateEnumerable<CallTrumpTrainingData>(result.Validation, reuseRowObject: false).ToList();
        var testData = _mlContext.Data.CreateEnumerable<CallTrumpTrainingData>(result.Test, reuseRowObject: false).ToList();

        trainData.Should().HaveCountGreaterThan(0);
        validationData.Should().HaveCountGreaterThan(0);
        testData.Should().HaveCountGreaterThan(0);
        (trainData.Count + validationData.Count + testData.Count).Should().Be(100);
    }

    [Fact]
    public void Split_PreservesDataIntegrity()
    {
        var splitter = CreateSplitter();
        var data = _trainingDataFaker.Generate(100);

        var result = splitter.Split(data);

        var trainData = _mlContext.Data.CreateEnumerable<CallTrumpTrainingData>(result.Train, reuseRowObject: false).ToList();

        trainData.Should().AllSatisfy(item =>
        {
            item.Card1Rank.Should().BeInRange(0, 5);
            item.Card1Suit.Should().BeInRange(0, 3);
            item.ExpectedDealPoints.Should().BeInRange(-2, 4);
        });
    }

    [Fact]
    public void Split_IntegrationTest_With300CallTrumpTrainingDataSamples()
    {
        var splitter = CreateSplitter(randomSeed: 999);
        var data = _trainingDataFaker.Generate(300);

        var result = splitter.Split(data, trainRatio: 0.6, validationRatio: 0.2, testRatio: 0.2);

        result.TrainRowCount.Should().BeInRange(162, 198);
        result.ValidationRowCount.Should().BeInRange(30, 90);
        result.TestRowCount.Should().BeInRange(30, 90);

        var trainData = _mlContext.Data.CreateEnumerable<CallTrumpTrainingData>(result.Train, reuseRowObject: false).ToList();
        var validationData = _mlContext.Data.CreateEnumerable<CallTrumpTrainingData>(result.Validation, reuseRowObject: false).ToList();
        var testData = _mlContext.Data.CreateEnumerable<CallTrumpTrainingData>(result.Test, reuseRowObject: false).ToList();

        trainData.Should().HaveCount(result.TrainRowCount);
        validationData.Should().HaveCount(result.ValidationRowCount);
        testData.Should().HaveCount(result.TestRowCount);

        trainData.Should().AllSatisfy(item => item.Should().NotBeNull());
        validationData.Should().AllSatisfy(item => item.Should().NotBeNull());
        testData.Should().AllSatisfy(item => item.Should().NotBeNull());
    }

    [Fact]
    public void Split_WithPreShuffled_SkipsInternalShuffle()
    {
        var data = _trainingDataFaker.Generate(100);

        var splitter = CreateSplitter(randomSeed: 42);
        var resultPreShuffled = splitter.Split(data, preShuffled: true);

        resultPreShuffled.TrainRowCount.Should().BeInRange(63, 77);
        (resultPreShuffled.TrainRowCount + resultPreShuffled.ValidationRowCount + resultPreShuffled.TestRowCount).Should().Be(100);
    }

    [Fact]
    public void Split_WithPreShuffled_StillSplitsCorrectly()
    {
        var splitter = CreateSplitter();
        var data = _trainingDataFaker.Generate(100);

        var result = splitter.Split(data, preShuffled: true);

        result.TrainRowCount.Should().BeInRange(63, 77);
        result.ValidationRowCount.Should().BeInRange(8, 22);
        result.TestRowCount.Should().BeInRange(8, 22);
        (result.TrainRowCount + result.ValidationRowCount + result.TestRowCount).Should().Be(100);
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

    [Fact]
    public void Split_IDataView_ProducesCorrectSplitRatios()
    {
        var splitter = CreateSplitter();
        var data = Enumerable.Range(0, 1000).Select(i => new TestData
        {
            Feature1 = i,
            Feature2 = i * 2,
            Label = i * 0.5f,
        }).ToList();
        var dataView = _mlContext.Data.LoadFromEnumerable(data);

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
        var data = Enumerable.Range(0, 100).Select(i => new TestData
        {
            Feature1 = i,
            Feature2 = i * 2,
            Label = i * 0.5f,
        }).ToList();
        var dataView = _mlContext.Data.LoadFromEnumerable(data);

        var result = splitter.Split(dataView);

        result.TrainRowCount.Should().BeInRange(63, 77);
    }

    [Fact]
    public void Split_IDataView_WithPreShuffledTrue_SkipsShuffling()
    {
        var splitter = CreateSplitter();
        var data = Enumerable.Range(0, 100).Select(i => new TestData
        {
            Feature1 = i,
            Feature2 = i * 2,
            Label = i * 0.5f,
        }).ToList();
        var dataView = _mlContext.Data.LoadFromEnumerable(data);

        var result = splitter.Split(dataView, preShuffled: true);

        result.TrainRowCount.Should().BeInRange(63, 77);
        (result.TrainRowCount + result.ValidationRowCount + result.TestRowCount).Should().Be(100);
    }

    [Fact]
    public void Split_IDataView_WithCustomRatios_ReturnsCorrectSplitCounts()
    {
        var splitter = CreateSplitter();
        var data = Enumerable.Range(0, 200).Select(i => new TestData
        {
            Feature1 = i,
            Feature2 = i * 2,
            Label = i * 0.5f,
        }).ToList();
        var dataView = _mlContext.Data.LoadFromEnumerable(data);

        var result = splitter.Split(dataView, trainRatio: 0.6, validationRatio: 0.2, testRatio: 0.2);

        result.TrainRowCount.Should().BeInRange(110, 130);
        (result.TrainRowCount + result.ValidationRowCount + result.TestRowCount).Should().Be(200);
    }

    [Fact]
    public void Split_IDataView_ReturnsEnumerableDataViews()
    {
        var splitter = CreateSplitter();
        var data = Enumerable.Range(0, 50).Select(i => new TestData
        {
            Feature1 = i,
            Feature2 = i * 2,
            Label = i * 0.5f,
        }).ToList();
        var dataView = _mlContext.Data.LoadFromEnumerable(data);

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

    [Fact]
    public void Split_WithStreamingEnumerable_SplitsCorrectly()
    {
        var splitter = CreateSplitter();
        var data = StreamData(_trainingDataFaker, 100);

        var result = splitter.Split(data);

        result.TrainRowCount.Should().BeInRange(63, 77);
        result.ValidationRowCount.Should().BeInRange(8, 22);
        result.TestRowCount.Should().BeInRange(8, 22);
        (result.TrainRowCount + result.ValidationRowCount + result.TestRowCount).Should().Be(100);
    }

    private static IEnumerable<CallTrumpTrainingData> StreamData(Faker<CallTrumpTrainingData> faker, int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return faker.Generate();
        }
    }

    private DataSplitter CreateSplitter(int randomSeed = 42)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new MachineLearningOptions
        {
            RandomSeed = randomSeed,
            ModelOutputPath = "./models",
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
