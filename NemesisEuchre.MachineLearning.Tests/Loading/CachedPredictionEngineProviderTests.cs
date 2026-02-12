using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;

namespace NemesisEuchre.MachineLearning.Tests.Loading;

public class CachedPredictionEngineProviderTests
{
    private readonly Mock<IModelLoader> _mockModelLoader = new();
    private readonly Mock<ILogger<CachedPredictionEngineProvider>> _mockLogger = new();
    private readonly IOptions<MachineLearningOptions> _options;
    private readonly CachedPredictionEngineProvider _provider;

    public CachedPredictionEngineProviderTests()
    {
        _options = Microsoft.Extensions.Options.Options.Create(new MachineLearningOptions
        {
            ModelOutputPath = "test-models",
        });

        _provider = new CachedPredictionEngineProvider(
            _mockModelLoader.Object,
            _options,
            _mockLogger.Object);
    }

    [Fact]
    public void TryGetEngine_FirstCall_CallsModelLoader()
    {
        _provider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "gen1");

        _mockModelLoader.Verify(
            x => x.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
                "test-models", "gen1", "CallTrump"),
            Times.Once);
    }

    [Fact]
    public void TryGetEngine_SecondCall_UsesCache()
    {
        _provider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "gen1");
        _provider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "gen1");

        _mockModelLoader.Verify(
            x => x.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
                "test-models", "gen1", "CallTrump"),
            Times.Once);
    }

    [Fact]
    public void TryGetEngine_DifferentDecisionTypes_LoadsSeparateEngines()
    {
        _provider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "gen1");
        _provider.TryGetEngine<DiscardCardTrainingData, DiscardCardRegressionPrediction>("DiscardCard", "gen1");

        _mockModelLoader.Verify(
            x => x.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
                "test-models", "gen1", "CallTrump"),
            Times.Once);

        _mockModelLoader.Verify(
            x => x.LoadModel<DiscardCardTrainingData, DiscardCardRegressionPrediction>(
                "test-models", "gen1", "DiscardCard"),
            Times.Once);
    }

    [Fact]
    public void TryGetEngine_DifferentModelNames_LoadsSeparateEngines()
    {
        _provider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "gen1");
        _provider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "gen2");

        _mockModelLoader.Verify(
            x => x.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
                "test-models", "gen1", "CallTrump"),
            Times.Once);

        _mockModelLoader.Verify(
            x => x.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
                "test-models", "gen2", "CallTrump"),
            Times.Once);
    }

    [Fact]
    public void TryGetEngine_ModelNotFound_ReturnsNullAndLogsWarning()
    {
        _mockModelLoader
            .Setup(x => x.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
                "test-models", "gen1", "CallTrump"))
            .Throws(new FileNotFoundException("Model not found"));

        _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var result = _provider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "gen1");

        result.Should().BeNull();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.Is<EventId>(e => e.Id == 35),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("CallTrump")),
                It.IsAny<FileNotFoundException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void TryGetEngine_LoadFails_ReturnsNullAndLogsError()
    {
        _mockModelLoader
            .Setup(x => x.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
                "test-models", "gen1", "CallTrump"))
            .Throws(new InvalidOperationException("Load failed"));

        _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var result = _provider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "gen1");

        result.Should().BeNull();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.Is<EventId>(e => e.Id == 36),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("CallTrump")),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void TryGetEngine_FailedLoad_CachesNullResult()
    {
        _mockModelLoader
            .Setup(x => x.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
                "test-models", "gen1", "CallTrump"))
            .Throws(new FileNotFoundException("Model not found"));

        var result1 = _provider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "gen1");
        var result2 = _provider.TryGetEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>("CallTrump", "gen1");

        result1.Should().BeNull();
        result2.Should().BeNull();

        _mockModelLoader.Verify(
            x => x.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(
                "test-models", "gen1", "CallTrump"),
            Times.Once);
    }
}
