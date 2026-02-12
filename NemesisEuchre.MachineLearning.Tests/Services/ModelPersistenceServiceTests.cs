using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

using Moq;

using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.MachineLearning.Tests.Services;

public class ModelPersistenceServiceTests : IDisposable
{
    private readonly MLContext _mlContext;
    private readonly ModelPersistenceService _service;
    private readonly string _tempDirectory;

    public ModelPersistenceServiceTests()
    {
        _mlContext = new MLContext(seed: 42);
        _service = new ModelPersistenceService(Mock.Of<ILogger<ModelPersistenceService>>());
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    }

    [Fact]
    public Task SaveModelAsync_WithNullModel_ThrowsInvalidOperationException()
    {
        var trainingResult = CreateTrainingResult();

        var act = () => _service.SaveModelAsync<CallTrumpTrainingData>(
            null!,
            _mlContext,
            _tempDirectory,
            "test",
            "CallTrump",
            trainingResult,
            CreateMetadata(),
            new object(),
            TestContext.Current.CancellationToken);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*TrainAsync*");
    }

    [Fact]
    public Task SaveModelAsync_WithNullModelsDirectory_ThrowsArgumentException()
    {
        var act = () => _service.SaveModelAsync<CallTrumpTrainingData>(
            Mock.Of<ITransformer>(),
            _mlContext,
            null!,
            "test",
            "CallTrump",
            CreateTrainingResult(),
            CreateMetadata(),
            new object(),
            TestContext.Current.CancellationToken);

        return act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public Task SaveModelAsync_WithNullModelName_ThrowsArgumentException()
    {
        var act = () => _service.SaveModelAsync<CallTrumpTrainingData>(
            Mock.Of<ITransformer>(),
            _mlContext,
            _tempDirectory,
            null!,
            "CallTrump",
            CreateTrainingResult(),
            CreateMetadata(),
            new object(),
            TestContext.Current.CancellationToken);

        return act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public Task SaveModelAsync_WithNullTrainingResult_ThrowsArgumentNullException()
    {
        var act = () => _service.SaveModelAsync<CallTrumpTrainingData>(
            Mock.Of<ITransformer>(),
            _mlContext,
            _tempDirectory,
            "test",
            "CallTrump",
            null!,
            CreateMetadata(),
            new object(),
            TestContext.Current.CancellationToken);

        return act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SaveModelAsync_CreatesDirectoryIfNotExists()
    {
        var outputDir = Path.Combine(_tempDirectory, "subdir");
        Directory.Exists(outputDir).Should().BeFalse();

        var model = TrainTrivialModel();
        var trainingResult = CreateTrainingResult(model);

        await _service.SaveModelAsync<CallTrumpTrainingData>(
            model,
            _mlContext,
            outputDir,
            "test",
            "CallTrump",
            trainingResult,
            CreateMetadata(),
            new { RSquared = 0.5 },
            TestContext.Current.CancellationToken);

        Directory.Exists(outputDir).Should().BeTrue();
    }

    [Fact]
    public async Task SaveModelAsync_CreatesZipJsonAndEvaluationFiles()
    {
        Directory.CreateDirectory(_tempDirectory);

        var model = TrainTrivialModel();
        var trainingResult = CreateTrainingResult(model);

        await _service.SaveModelAsync<CallTrumpTrainingData>(
            model,
            _mlContext,
            _tempDirectory,
            "mymodel",
            "CallTrump",
            trainingResult,
            CreateMetadata(),
            new { RSquared = 0.5, MeanAbsoluteError = 1.2 },
            TestContext.Current.CancellationToken);

        File.Exists(Path.Combine(_tempDirectory, "mymodel_calltrump.zip")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDirectory, "mymodel_calltrump.json")).Should().BeTrue();
        File.Exists(Path.Combine(_tempDirectory, "mymodel_calltrump.evaluation.json")).Should().BeTrue();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, true);
            }
#pragma warning disable S108
            catch (IOException)
            {
            }
#pragma warning restore S108
        }
    }

    private static TrainingResult CreateTrainingResult(ITransformer? model = null)
    {
        return new TrainingResult(
            model ?? Mock.Of<ITransformer>(),
            new RegressionEvaluationMetrics(0.5, 1.0, 1.5, 2.0, 0.3),
            700,
            150,
            150);
    }

    private static ModelMetadata CreateMetadata()
    {
        return new ModelMetadata(
            "CallTrump",
            "test",
            DateTime.UtcNow,
            700,
            150,
            150,
            new HyperparametersMetadata("LightGbm", 31, 200, 0.1, 42),
            new RegressionMetricsMetadata(0.5, 1.0, 1.5, 2.0),
            "1.0");
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "Test method - readability over performance")]
    private ITransformer TrainTrivialModel()
    {
        var trainingData = new List<CallTrumpTrainingData>();
        for (int i = 0; i < 50; i++)
        {
            trainingData.Add(new CallTrumpTrainingData
            {
                Card1Rank = i % 6,
                Card1Suit = i % 4,
                Card2Rank = (i + 1) % 6,
                Card2Suit = (i + 1) % 4,
                Card3Rank = (i + 2) % 6,
                Card3Suit = (i + 2) % 4,
                Card4Rank = (i + 3) % 6,
                Card4Suit = (i + 3) % 4,
                Card5Rank = (i + 4) % 6,
                Card5Suit = (i + 4) % 4,
                UpCardRank = i % 6,
                UpCardSuit = i % 4,
                DealerPosition = i % 4,
                TeamScore = i % 11,
                OpponentScore = (i + 1) % 11,
                DecisionOrder = i % 8,
                ExpectedDealPoints = (short)((i % 5) - 2),
            });
        }

        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        var pipeline = _mlContext.Transforms.Concatenate(
                "Features",
                nameof(CallTrumpTrainingData.Card1Rank),
                nameof(CallTrumpTrainingData.Card1Suit))
            .Append(_mlContext.Regression.Trainers.Sdca(
                labelColumnName: "Label",
                featureColumnName: "Features"));

        return pipeline.Fit(dataView);
    }
}
