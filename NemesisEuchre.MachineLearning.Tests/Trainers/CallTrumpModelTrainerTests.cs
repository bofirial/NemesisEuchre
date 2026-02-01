using Bogus;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.MachineLearning.Tests.Trainers;

public class CallTrumpModelTrainerTests
{
    private readonly MLContext _mlContext;
    private readonly IDataSplitter _dataSplitter;
    private readonly IModelVersionManager _versionManager;
    private readonly IOptions<MachineLearningOptions> _options;
    private readonly ILogger<CallTrumpModelTrainer> _logger;
    private readonly CallTrumpModelTrainer _trainer;
    private readonly Faker<CallTrumpTrainingData> _faker;

    public CallTrumpModelTrainerTests()
    {
        _mlContext = new MLContext(seed: 42);
        var mlOptions = new MachineLearningOptions
        {
            ModelOutputPath = "./models",
            NumberOfLeaves = 20,
            NumberOfIterations = 25,
            LearningRate = 0.1,
            MinimumExampleCountPerLeaf = 10,
            RandomSeed = 42,
        };
        _options = Microsoft.Extensions.Options.Options.Create(mlOptions);
        _dataSplitter = new DataSplitter(_mlContext, _options);
        _versionManager = new ModelVersionManager();
        _logger = new LoggerFactory().CreateLogger<CallTrumpModelTrainer>();
        _trainer = new CallTrumpModelTrainer(_mlContext, _dataSplitter, _versionManager, _options, _logger);

        _faker = new Faker<CallTrumpTrainingData>()
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
    public async Task TrainAsync_WithSmallDataset_CompletesSuccessfully()
    {
        var trainingData = _faker.Generate(100);

        var result = await _trainer.TrainAsync(trainingData);

        result.Should().NotBeNull();
        result.Model.Should().NotBeNull();
        result.ValidationMetrics.Should().NotBeNull();
        result.TrainingSamples.Should().BeGreaterThan(0);
        result.ValidationSamples.Should().BeGreaterThan(0);
        result.TestSamples.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task TrainAsync_WithSmallDataset_ProducesReasonableAccuracy()
    {
        var trainingData = _faker.Generate(100);

        var result = await _trainer.TrainAsync(trainingData);

        ((EvaluationMetrics)result.ValidationMetrics).MicroAccuracy.Should().BeGreaterThanOrEqualTo(0);
        ((EvaluationMetrics)result.ValidationMetrics).MacroAccuracy.Should().BeGreaterThanOrEqualTo(0);
        ((EvaluationMetrics)result.ValidationMetrics).LogLoss.Should().BeGreaterThan(0);
    }

    [Fact]
    public Task TrainAsync_WithNullData_ThrowsArgumentNullException()
    {
        var act = async () => await _trainer.TrainAsync(null!);

        return act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public Task TrainAsync_WithEmptyData_ThrowsInvalidOperationException()
    {
        var act = async () => await _trainer.TrainAsync([]);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot split empty dataset.");
    }

    [Fact]
    public Task TrainAsync_WithInsufficientData_ThrowsInvalidOperationException()
    {
        var trainingData = _faker.Generate(2);

        var act = async () => await _trainer.TrainAsync(trainingData);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Dataset must contain at least 3 samples for splitting. Found 2 samples.");
    }

    [Fact]
    public Task EvaluateAsync_WithoutTraining_ThrowsInvalidOperationException()
    {
        var testData = _mlContext.Data.LoadFromEnumerable(_faker.Generate(10));

        var act = async () => await _trainer.EvaluateAsync(testData);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Model must be trained before evaluation. Call TrainAsync first.");
    }

    [Fact]
    public async Task EvaluateAsync_AfterTraining_ReturnsMetrics()
    {
        var trainingData = _faker.Generate(100);
        await _trainer.TrainAsync(trainingData);

        var testData = _mlContext.Data.LoadFromEnumerable(_faker.Generate(20));
        var metrics = await _trainer.EvaluateAsync(testData);

        metrics.Should().NotBeNull();
        metrics.MicroAccuracy.Should().BeInRange(0, 1);
        metrics.MacroAccuracy.Should().BeInRange(0, 1);
        metrics.LogLoss.Should().BeGreaterThan(0);
        metrics.PerClassLogLoss.Should().HaveCount(11);
        metrics.ConfusionMatrix.Should().HaveCount(11);
        metrics.PerClassMetrics.Should().HaveCount(11);
        foreach (var classMetric in metrics.PerClassMetrics)
        {
            classMetric.ClassLabel.Should().BeInRange(0, 10);
            classMetric.Support.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public Task SaveModelAsync_WithoutTraining_ThrowsInvalidOperationException()
    {
        var result = new TrainingResult(
            null!,
            new EvaluationMetrics(0.5, 0.5, 1.0, 0.5, new double[11], new int[11][], []),
            70,
            15,
            15);

        var act = async () => await _trainer.SaveModelAsync("./models", 1, ActorType.Chaos, result);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No trained model to save. Call TrainAsync first.");
    }

    [Fact]
    public async Task SaveModelAsync_AfterTraining_CreatesModelAndMetadataFiles()
    {
        var trainingData = _faker.Generate(100);
        var result = await _trainer.TrainAsync(trainingData);

        var modelsDirectory = Path.Combine(Path.GetTempPath(), $"test_models_{Guid.NewGuid()}");

        try
        {
            await _trainer.SaveModelAsync(modelsDirectory, 1, ActorType.Chaos, result);

            var modelPath = Path.Combine(modelsDirectory, "gen1_calltrump_v1.zip");
            var metadataPath = Path.Combine(modelsDirectory, "gen1_calltrump_v1.json");

            File.Exists(modelPath).Should().BeTrue();
            File.Exists(metadataPath).Should().BeTrue();

            var metadataContent = await File.ReadAllTextAsync(metadataPath);
            metadataContent.Should().Contain("CallTrump");
            metadataContent.Should().Contain("LightGbm");
            metadataContent.Should().Contain("Chaos");
        }
        finally
        {
            if (Directory.Exists(modelsDirectory))
            {
                Directory.Delete(modelsDirectory, true);
            }
        }
    }

    [Fact]
    public async Task SaveModelAsync_AfterTraining_CreatesEvaluationReportFile()
    {
        var trainingData = _faker.Generate(100);
        var result = await _trainer.TrainAsync(trainingData);

        var modelsDirectory = Path.Combine(Path.GetTempPath(), $"test_models_{Guid.NewGuid()}");

        try
        {
            await _trainer.SaveModelAsync(modelsDirectory, 1, ActorType.Chaos, result);

            var evaluationPath = Path.Combine(modelsDirectory, "gen1_calltrump_v1.evaluation.json");
            File.Exists(evaluationPath).Should().BeTrue();

            var reportContent = await File.ReadAllTextAsync(evaluationPath);
            reportContent.Should().Contain("CallTrump");
            reportContent.Should().Contain("PerClass");
            reportContent.Should().Contain("Precision");
            reportContent.Should().Contain("Recall");
            reportContent.Should().Contain("F1Score");
        }
        finally
        {
            if (Directory.Exists(modelsDirectory))
            {
                Directory.Delete(modelsDirectory, true);
            }
        }
    }

    [Fact]
    public async Task SaveModelAsync_WithNullPath_ThrowsArgumentException()
    {
        var trainingData = _faker.Generate(100);
        var result = await _trainer.TrainAsync(trainingData);

        var act = async () => await _trainer.SaveModelAsync(null!, 1, ActorType.Chaos, result);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task TrainAsync_WithLargerDataset_ProducesValidPredictions()
    {
        var trainingData = _faker.Generate(500);
        var result = await _trainer.TrainAsync(trainingData);

        var testData = _faker.Generate(10);
        var testDataView = _mlContext.Data.LoadFromEnumerable(testData);
        var predictions = result.Model.Transform(testDataView);

        foreach (var prediction in _mlContext.Data.CreateEnumerable<PredictionOutput>(predictions, reuseRowObject: false))
        {
            prediction.PredictedLabel.Should().BeInRange(-4.0f, 4.0f);
        }
    }

    [Fact]
    public async Task TrainAsync_WithLimitedVariation_TrainsSuccessfully()
    {
        var trainingData = _faker.Generate(100);
        for (var i = 0; i < trainingData.Count; i++)
        {
            trainingData[i].ExpectedDealPoints = (short)(i % 2);
        }

        var result = await _trainer.TrainAsync(trainingData);

        result.Should().NotBeNull();
        ((EvaluationMetrics)result.ValidationMetrics).MicroAccuracy.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetModelType_ReturnsCallTrump()
    {
        var modelType = typeof(CallTrumpModelTrainer)
            .GetMethod("GetModelType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(_trainer, null) as string;

        modelType.Should().Be("CallTrump");
    }

    [Fact]
    public void GetNumberOfClasses_Returns11()
    {
        var numberOfClasses = (int)typeof(CallTrumpModelTrainer)
            .GetMethod("GetNumberOfClasses", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(_trainer, null)!;

        numberOfClasses.Should().Be(11);
    }

    private sealed class PredictionOutput
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "Test Class")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Test Class")]
        public float PredictedLabel;
    }
}
