using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

using Moq;

using NemesisEuchre.MachineLearning.Caching;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.Tests.Caching;

public class ModelCacheTests : IDisposable
{
    private readonly MLContext _mlContext;
    private readonly Mock<ILogger<ModelCache>> _mockLogger;
    private readonly string _tempDirectory;
    private readonly string _testModelPath;

    public ModelCacheTests()
    {
        _mlContext = new MLContext(seed: 42);
        _mockLogger = new Mock<ILogger<ModelCache>>();
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _testModelPath = Path.Combine(_tempDirectory, "test-model.zip");

        CreateTestModel();
    }

    [Fact]
    public void GetOrCreatePredictionEngine_FirstCall_CreatesNewEngine()
    {
        var cache = new ModelCache(_mlContext, _mockLogger.Object);

        var engine = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(_testModelPath);

        engine.Should().NotBeNull();
    }

    [Fact]
    public void GetOrCreatePredictionEngine_SecondCall_ReturnsCachedEngine()
    {
        var cache = new ModelCache(_mlContext, _mockLogger.Object);

        var engine1 = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(_testModelPath);
        var engine2 = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(_testModelPath);

        engine1.Should().BeSameAs(engine2);
    }

    [Fact]
    public void GetOrCreatePredictionEngine_WithConcurrentRequests_CreatesOnlyOnce()
    {
        var cache = new ModelCache(_mlContext, _mockLogger.Object);
        var engines = new List<object>();
        var locker = new object();

        Parallel.For(0, 10, _ =>
        {
            var engine = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(_testModelPath);
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
        var cache = new ModelCache(_mlContext, _mockLogger.Object);
        var invalidPath = Path.Combine(_tempDirectory, "nonexistent-model.zip");

        var act = () => cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(invalidPath);

        act.Should().Throw<FileNotFoundException>()
            .WithMessage($"Model file not found at path: {invalidPath}*");
    }

    [Fact]
    public void InvalidateCache_DisposesEngine_AndAllowsRecreation()
    {
        var cache = new ModelCache(_mlContext, _mockLogger.Object);

        var engine1 = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(_testModelPath);
        cache.InvalidateCache(_testModelPath);
        var engine2 = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(_testModelPath);

        engine1.Should().NotBeSameAs(engine2);
    }

    [Fact]
    public void InvalidateAll_ClearsAllCachedEngines()
    {
        var cache = new ModelCache(_mlContext, _mockLogger.Object);

        var engine1 = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(_testModelPath);
        cache.InvalidateAll();
        var engine2 = cache.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpPrediction>(_testModelPath);

        engine1.Should().NotBeSameAs(engine2);
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
            Directory.Delete(_tempDirectory, true);
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
                DecisionOrder = i % 8,
                Decision0IsValid = 1,
                Decision1IsValid = 1,
                Decision2IsValid = 1,
                Decision3IsValid = 1,
                Decision4IsValid = 1,
                Decision5IsValid = 1,
                Decision6IsValid = 1,
                Decision7IsValid = 1,
                Decision8IsValid = 1,
                Decision9IsValid = 1,
                Decision10IsValid = 1,
                ExpectedDealPoints = (short)((i % 5) - 2),
            });
        }

        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label")
            .Append(_mlContext.Transforms.Concatenate(
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
                nameof(CallTrumpTrainingData.DecisionOrder)))
            .Append(_mlContext.MulticlassClassification.Trainers.LightGbm())
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        var model = pipeline.Fit(dataView);
        _mlContext.Model.Save(model, dataView.Schema, _testModelPath);
    }
}
