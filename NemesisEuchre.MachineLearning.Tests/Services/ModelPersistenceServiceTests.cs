using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

using Moq;

using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.MachineLearning.Tests.Services;

public class ModelPersistenceServiceTests
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
            TestContext.Current.CancellationToken);

        return act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static TrainingResult CreateTrainingResult()
    {
        return new TrainingResult(
            Mock.Of<ITransformer>(),
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
            new RegressionMetricsMetadata(0.5, 1.0, 1.5, 2.0, 0.3),
            "1.0");
    }
}
