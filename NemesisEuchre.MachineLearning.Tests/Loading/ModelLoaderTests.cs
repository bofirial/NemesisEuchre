using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.MachineLearning.Caching;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.Tests.Loading;

public class ModelLoaderTests
{
    private readonly Mock<IModelCache> _mockModelCache;
    private readonly ModelLoader _loader;

    public ModelLoaderTests()
    {
        _mockModelCache = new Mock<IModelCache>();
        var mockLogger = new Mock<ILogger<ModelLoader>>();
        _loader = new ModelLoader(_mockModelCache.Object, mockLogger.Object);
    }

    [Fact]
    public void LoadModel_WithNullModelsDirectory_ThrowsArgumentException()
    {
        var act = () => _loader.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(null!, "model", "CallTrump");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void LoadModel_WithEmptyOrWhitespaceModelsDirectory_ThrowsArgumentException(string modelsDirectory)
    {
        var act = () => _loader.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>(modelsDirectory, "model", "CallTrump");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void LoadModel_WithNullModelName_ThrowsArgumentException()
    {
        var act = () => _loader.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>("dir", null!, "CallTrump");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void LoadModel_WithNullDecisionType_ThrowsArgumentException()
    {
        var act = () => _loader.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>("dir", "model", null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void LoadModel_BuildsCorrectFilePath_DelegatesToModelCache()
    {
        var expectedPath = Path.Combine("models", "gen1_calltrump.zip");

        _loader.LoadModel<CallTrumpTrainingData, CallTrumpRegressionPrediction>("models", "gen1", "CallTrump");

        _mockModelCache.Verify(
            c => c.GetOrCreatePredictionEngine<CallTrumpTrainingData, CallTrumpRegressionPrediction>(expectedPath),
            Times.Once);
    }

    [Fact]
    public void LoadModel_LowercasesDecisionType()
    {
        var expectedPath = Path.Combine("dir", "name_playcard.zip");

        _loader.LoadModel<PlayCardTrainingData, PlayCardRegressionPrediction>("dir", "name", "PlayCard");

        _mockModelCache.Verify(
            c => c.GetOrCreatePredictionEngine<PlayCardTrainingData, PlayCardRegressionPrediction>(expectedPath),
            Times.Once);
    }

    [Fact]
    public void InvalidateCache_DelegatesToModelCache()
    {
        _loader.InvalidateCache("some/path.zip");

        _mockModelCache.Verify(c => c.InvalidateCache("some/path.zip"), Times.Once);
    }

    [Fact]
    public void InvalidateAll_DelegatesToModelCache()
    {
        _loader.InvalidateAll();

        _mockModelCache.Verify(c => c.InvalidateAll(), Times.Once);
    }
}
