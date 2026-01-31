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
            .RuleFor(x => x.ChosenDecisionIndex, f => f.Random.UInt(0, 10));
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

        result.TrainRowCount.Should().Be(70);
        result.ValidationRowCount.Should().Be(15);
        result.TestRowCount.Should().Be(15);
    }

    [Fact]
    public void Split_With1000Samples_ReturnsCorrectSplitCounts()
    {
        var splitter = CreateSplitter();
        var data = _trainingDataFaker.Generate(1000);

        var result = splitter.Split(data);

        result.TrainRowCount.Should().Be(700);
        result.ValidationRowCount.Should().Be(150);
        result.TestRowCount.Should().Be(150);
    }

    [Fact]
    public void Split_With97IndivisibleSamples_HandlesRemainder()
    {
        var splitter = CreateSplitter();
        var data = _trainingDataFaker.Generate(97);

        var result = splitter.Split(data);

        result.TrainRowCount.Should().Be(67);
        result.ValidationRowCount.Should().Be(14);
        result.TestRowCount.Should().Be(16);
        (result.TrainRowCount + result.ValidationRowCount + result.TestRowCount).Should().Be(97);
    }

    [Fact]
    public void Split_WithCustomRatios_ReturnsCorrectSplitCounts()
    {
        var splitter = CreateSplitter();
        var data = _trainingDataFaker.Generate(300);

        var result = splitter.Split(data, trainRatio: 0.6, validationRatio: 0.2, testRatio: 0.2);

        result.TrainRowCount.Should().Be(180);
        result.ValidationRowCount.Should().Be(60);
        result.TestRowCount.Should().Be(60);
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
            train1[i].ChosenDecisionIndex.Should().Be(train2[i].ChosenDecisionIndex);
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
                train1[i].ChosenDecisionIndex != train2[i].ChosenDecisionIndex)
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

        trainData.Should().HaveCount(70);
        validationData.Should().HaveCount(15);
        testData.Should().HaveCount(15);
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
            item.ChosenDecisionIndex.Should().BeInRange(0u, 10u);
        });
    }

    [Fact]
    public void Split_IntegrationTest_With300CallTrumpTrainingDataSamples()
    {
        var splitter = CreateSplitter(randomSeed: 999);
        var data = _trainingDataFaker.Generate(300);

        var result = splitter.Split(data, trainRatio: 0.6, validationRatio: 0.2, testRatio: 0.2);

        result.TrainRowCount.Should().Be(180);
        result.ValidationRowCount.Should().Be(60);
        result.TestRowCount.Should().Be(60);

        var trainData = _mlContext.Data.CreateEnumerable<CallTrumpTrainingData>(result.Train, reuseRowObject: false).ToList();
        var validationData = _mlContext.Data.CreateEnumerable<CallTrumpTrainingData>(result.Validation, reuseRowObject: false).ToList();
        var testData = _mlContext.Data.CreateEnumerable<CallTrumpTrainingData>(result.Test, reuseRowObject: false).ToList();

        trainData.Should().HaveCount(180);
        validationData.Should().HaveCount(60);
        testData.Should().HaveCount(60);

        trainData.Should().AllSatisfy(item => item.Should().NotBeNull());
        validationData.Should().AllSatisfy(item => item.Should().NotBeNull());
        testData.Should().AllSatisfy(item => item.Should().NotBeNull());
    }

    private DataSplitter CreateSplitter(int randomSeed = 42)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new MachineLearningOptions
        {
            RandomSeed = randomSeed,
            ModelOutputPath = "./models",
            TrainingIterations = 100,
            LearningRate = 0.1,
        });

        return new DataSplitter(_mlContext, options);
    }
}
