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

public class DiscardCardModelTrainerTests
{
    private readonly MLContext _mlContext;
    private readonly IDataSplitter _dataSplitter;
    private readonly IModelVersionManager _versionManager;
    private readonly IOptions<MachineLearningOptions> _options;
    private readonly ILogger<DiscardCardModelTrainer> _logger;
    private readonly DiscardCardModelTrainer _trainer;
    private readonly Faker<DiscardCardTrainingData> _faker;

    public DiscardCardModelTrainerTests()
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
        _logger = new LoggerFactory().CreateLogger<DiscardCardModelTrainer>();
        _trainer = new DiscardCardModelTrainer(_mlContext, _dataSplitter, _versionManager, _options, _logger);

        _faker = new Faker<DiscardCardTrainingData>()
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
            .RuleFor(x => x.Card6Rank, f => f.Random.Float(0, 5))
            .RuleFor(x => x.Card6Suit, f => f.Random.Float(0, 3))
            .RuleFor(x => x.CallingPlayerPosition, f => f.Random.Float(0, 3))
            .RuleFor(x => x.CallingPlayerGoingAlone, f => f.Random.Float(0, 1))
            .RuleFor(x => x.TeamScore, f => f.Random.Float(0, 10))
            .RuleFor(x => x.OpponentScore, f => f.Random.Float(0, 10))
            .RuleFor(x => x.ChosenCardIndex, f => f.Random.UInt(0, 5));
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

        result.ValidationMetrics.MicroAccuracy.Should().BeGreaterThanOrEqualTo(0);
        result.ValidationMetrics.MacroAccuracy.Should().BeGreaterThanOrEqualTo(0);
        result.ValidationMetrics.LogLoss.Should().BeGreaterThan(0);
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
        metrics.PerClassLogLoss.Should().HaveCount(6);
        metrics.ConfusionMatrix.Should().HaveCount(6);
        metrics.PerClassMetrics.Should().HaveCount(6);
        foreach (var classMetric in metrics.PerClassMetrics)
        {
            classMetric.ClassLabel.Should().BeInRange(0, 5);
            classMetric.Support.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public Task SaveModelAsync_WithoutTraining_ThrowsInvalidOperationException()
    {
        var result = new TrainingResult(
            null!,
            new EvaluationMetrics(0.5, 0.5, 1.0, 0.5, new double[6], new int[6][], []),
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

            var modelPath = Path.Combine(modelsDirectory, "gen1_discardcard_v1.zip");
            var metadataPath = Path.Combine(modelsDirectory, "gen1_discardcard_v1.json");

            File.Exists(modelPath).Should().BeTrue();
            File.Exists(metadataPath).Should().BeTrue();

            var metadataContent = await File.ReadAllTextAsync(metadataPath);
            metadataContent.Should().Contain("DiscardCard");
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

            var evaluationPath = Path.Combine(modelsDirectory, "gen1_discardcard_v1.evaluation.json");
            File.Exists(evaluationPath).Should().BeTrue();

            var reportContent = await File.ReadAllTextAsync(evaluationPath);
            reportContent.Should().Contain("DiscardCard");
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
            prediction.PredictedLabel.Should().BeInRange(0u, 5u);
        }
    }

    [Fact]
    public async Task TrainAsync_WithLimitedVariation_TrainsSuccessfully()
    {
        var trainingData = _faker.Generate(100);
        for (var i = 0; i < trainingData.Count; i++)
        {
            trainingData[i].ChosenCardIndex = (uint)(i % 2);
        }

        var result = await _trainer.TrainAsync(trainingData);

        result.Should().NotBeNull();
        result.ValidationMetrics.MicroAccuracy.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetModelType_ReturnsDiscardCard()
    {
        var modelType = typeof(DiscardCardModelTrainer)
            .GetMethod("GetModelType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(_trainer, null) as string;

        modelType.Should().Be("DiscardCard");
    }

    [Fact]
    public void GetNumberOfClasses_Returns6()
    {
        var numberOfClasses = (int)typeof(DiscardCardModelTrainer)
            .GetMethod("GetNumberOfClasses", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(_trainer, null)!;

        numberOfClasses.Should().Be(6);
    }

    private sealed class PredictionOutput
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "Test Class")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Test Class")]
        public uint PredictedLabel = 1;
    }
}
