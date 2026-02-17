using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

using Moq;

using NemesisEuchre.MachineLearning.Caching;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.Tests.Caching;

public class ModelCacheTestFixture : IDisposable
{
    public ModelCacheTestFixture()
    {
        TempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(TempDirectory);
        TestModelPath = Path.Combine(TempDirectory, "test-model.zip");

        CreateTestModel();
    }

    public MLContext MlContext { get; } = new MLContext(seed: 42);

    public string TempDirectory { get; }

    public string TestModelPath { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && Directory.Exists(TempDirectory))
        {
            Directory.Delete(TempDirectory, true);
        }
    }

    private void CreateTestModel()
    {
        var trainingData = new List<CallTrumpTrainingData>();
        for (int i = 0; i < 100; i++)
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
                DecisionNumber = i % 8,
                ChosenDecision = i % 11,
                ExpectedDealPoints = (short)((i % 5) - 2),
            });
        }

        var dataView = MlContext.Data.LoadFromEnumerable(trainingData);

        var pipeline = MlContext.Transforms.Conversion.MapValueToKey("Label")
            .Append(MlContext.Transforms.Concatenate(
                "Features",
                nameof(CallTrumpTrainingData.Card1Rank),
                nameof(CallTrumpTrainingData.Card1Suit),
                nameof(CallTrumpTrainingData.Card2Rank),
                nameof(CallTrumpTrainingData.Card2Suit),
                nameof(CallTrumpTrainingData.Card3Rank),
                nameof(CallTrumpTrainingData.Card3Suit),
                nameof(CallTrumpTrainingData.Card4Rank),
                nameof(CallTrumpTrainingData.Card4Suit),
                nameof(CallTrumpTrainingData.Card5Rank),
                nameof(CallTrumpTrainingData.Card5Suit),
                nameof(CallTrumpTrainingData.UpCardRank),
                nameof(CallTrumpTrainingData.UpCardSuit),
                nameof(CallTrumpTrainingData.DealerPosition),
                nameof(CallTrumpTrainingData.TeamScore),
                nameof(CallTrumpTrainingData.OpponentScore),
                nameof(CallTrumpTrainingData.DecisionNumber),
                nameof(CallTrumpTrainingData.ChosenDecision)))
            .Append(MlContext.MulticlassClassification.Trainers.LightGbm())
            .Append(MlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        var model = pipeline.Fit(dataView);
        MlContext.Model.Save(model, dataView.Schema, TestModelPath);
    }
}

public class ModelCacheTests(ModelCacheTestFixture fixture) : IClassFixture<ModelCacheTestFixture>
{
    private readonly Mock<ILogger<ModelCache>> _mockLogger = new();

    [Fact]
    public void GetOrCreatePredictionEngine_FirstCall_CreatesNewEngine()
    {
        var cache = new ModelCache(fixture.MlContext, _mockLogger.Object);

        var engine = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(fixture.TestModelPath);

        engine.Should().NotBeNull();
    }

    [Fact]
    public void GetOrCreatePredictionEngine_SecondCall_ReturnsCachedEngine()
    {
        var cache = new ModelCache(fixture.MlContext, _mockLogger.Object);

        var engine1 = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(fixture.TestModelPath);
        var engine2 = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(fixture.TestModelPath);

        engine1.Should().BeSameAs(engine2);
    }

    [Fact]
    public void GetOrCreatePredictionEngine_WithConcurrentRequests_CreatesOnlyOnce()
    {
        var cache = new ModelCache(fixture.MlContext, _mockLogger.Object);
        var engines = new List<object>();
        var locker = new object();

        Parallel.For(0, 10, _ =>
        {
            var engine = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(fixture.TestModelPath);
            lock (locker)
            {
                engines.Add(engine);
            }
        });

        engines.Should().HaveCount(10);
        engines.Distinct().Should().HaveCount(1);
    }

    [Fact]
    public void GetOrCreatePredictionEngine_WithInvalidPath_ThrowsFileNotFoundException()
    {
        var cache = new ModelCache(fixture.MlContext, _mockLogger.Object);
        var invalidPath = Path.Combine(fixture.TempDirectory, "nonexistent-model.zip");

        var act = () => cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(invalidPath);

        act.Should().Throw<FileNotFoundException>()
            .WithMessage($"Model file not found at path: {invalidPath}*");
    }

    [Fact]
    public void InvalidateCache_DisposesEngine_AndAllowsRecreation()
    {
        var cache = new ModelCache(fixture.MlContext, _mockLogger.Object);

        var engine1 = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(fixture.TestModelPath);
        cache.InvalidateCache(fixture.TestModelPath);
        var engine2 = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(fixture.TestModelPath);

        engine1.Should().NotBeSameAs(engine2);
    }

    [Fact]
    public void InvalidateAll_ClearsAllCachedEngines()
    {
        var cache = new ModelCache(fixture.MlContext, _mockLogger.Object);

        var engine1 = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(fixture.TestModelPath);
        cache.InvalidateAll();
        var engine2 = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(fixture.TestModelPath);

        engine1.Should().NotBeSameAs(engine2);
    }
}
