using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using Moq;

using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.MachineLearning.Tests.Trainers;

public class RegressionModelTrainerBaseTests
{
    private readonly MLContext _mlContext;
    private readonly Mock<IDataSplitter> _mockDataSplitter;
    private readonly Mock<IModelPersistenceService> _mockPersistenceService;
    private readonly IOptions<MachineLearningOptions> _options;
    private readonly Mock<ILogger> _mockLogger;

    public RegressionModelTrainerBaseTests()
    {
        _mlContext = new MLContext(seed: 42);
        _mockDataSplitter = new Mock<IDataSplitter>();
        _mockPersistenceService = new Mock<IModelPersistenceService>();
        _options = Microsoft.Extensions.Options.Options.Create(new MachineLearningOptions
        {
            RandomSeed = 42,
            ModelOutputPath = "models",
            NumberOfLeaves = 31,
            NumberOfIterations = 200,
            LearningRate = 0.1,
            MinimumExampleCountPerLeaf = 20,
        });
        _mockLogger = new Mock<ILogger>();
    }

    [Fact]
    public void Constructor_WithNullMlContext_ThrowsArgumentNullException()
    {
        var act = () => new TestableTrainer(
            null!,
            _mockDataSplitter.Object,
            _mockPersistenceService.Object,
            _options,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("mlContext");
    }

    [Fact]
    public void Constructor_WithNullDataSplitter_ThrowsArgumentNullException()
    {
        var act = () => new TestableTrainer(
            _mlContext,
            null!,
            _mockPersistenceService.Object,
            _options,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("dataSplitter");
    }

    [Fact]
    public void Constructor_WithNullPersistenceService_ThrowsArgumentNullException()
    {
        var act = () => new TestableTrainer(
            _mlContext,
            _mockDataSplitter.Object,
            null!,
            _options,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("persistenceService");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        var act = () => new TestableTrainer(
            _mlContext,
            _mockDataSplitter.Object,
            _mockPersistenceService.Object,
            null!,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var act = () => new TestableTrainer(
            _mlContext,
            _mockDataSplitter.Object,
            _mockPersistenceService.Object,
            _options,
            null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public Task TrainAsync_WithNullDataView_ThrowsArgumentNullException()
    {
        var trainer = CreateTrainer();

        var act = () => trainer.TrainAsync(null!, cancellationToken: TestContext.Current.CancellationToken);

        return act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("dataView");
    }

    [Fact]
    public Task EvaluateAsync_ViaInterface_ThrowsNotSupportedException()
    {
        IModelTrainer<CallTrumpTrainingData> trainer = CreateTrainer();

        var dataView = _mlContext.Data.LoadFromEnumerable(new List<CallTrumpTrainingData>());
        var act = () => trainer.EvaluateAsync(dataView, TestContext.Current.CancellationToken);

        return act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*EvaluateRegressionAsync*");
    }

    [Fact]
    public Task EvaluateAsync_WithNullTestData_ThrowsArgumentNullException()
    {
        var trainer = CreateTrainer();

        var act = () => trainer.EvaluateAsync(null!, TestContext.Current.CancellationToken);

        return act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("testData");
    }

    [Fact]
    public Task EvaluateAsync_WithoutTrainedModel_ThrowsInvalidOperationException()
    {
        var trainer = CreateTrainer();
        var dataView = _mlContext.Data.LoadFromEnumerable(new List<CallTrumpTrainingData>());

        var act = () => trainer.EvaluateAsync(dataView, TestContext.Current.CancellationToken);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*TrainAsync*");
    }

    [Fact]
    public Task SaveModelAsync_WithoutTrainedModel_ThrowsInvalidOperationException()
    {
        var trainer = CreateTrainer();
        var trainingResult = new TrainingResult(
            Mock.Of<ITransformer>(),
            new RegressionEvaluationMetrics(0.5, 1.0, 1.5, 2.0, 0.3),
            700,
            150,
            150);

        var act = () => trainer.SaveModelAsync(
            "models",
            "test",
            trainingResult,
            TestContext.Current.CancellationToken);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*TrainAsync*");
    }

    private TestableTrainer CreateTrainer()
    {
        return new TestableTrainer(
            _mlContext,
            _mockDataSplitter.Object,
            _mockPersistenceService.Object,
            _options,
            _mockLogger.Object);
    }

    private sealed class TestableTrainer(
        MLContext mlContext,
        IDataSplitter dataSplitter,
        IModelPersistenceService persistenceService,
        IOptions<MachineLearningOptions> options,
        ILogger logger)
        : RegressionModelTrainerBase<CallTrumpTrainingData>(mlContext, dataSplitter, persistenceService, options, logger)
    {
        protected override IEstimator<ITransformer> BuildPipeline(IDataView trainingData)
        {
            return MlContext.Transforms.Concatenate(
                    "Features",
                    nameof(CallTrumpTrainingData.Card1Rank),
                    nameof(CallTrumpTrainingData.Card1Suit))
                .Append(MlContext.Regression.Trainers.Sdca(
                    labelColumnName: "Label",
                    featureColumnName: "Features"));
        }

        protected override string GetModelType()
        {
            return "TestModel";
        }
    }
}
