using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services.TrainerExecutors;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Tests.Services;

public class TrainerExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_WhenNoDataAvailable_ReturnsFailureResult()
    {
        var mockTrainer = new Mock<IModelTrainer<CallTrumpTrainingData>>();
        var mockDataLoader = new Mock<ITrainingDataLoader<CallTrumpTrainingData>>();
        mockDataLoader.Setup(l => l.StreamTrainingData(
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Returns([]);

        mockTrainer.Setup(t => t.TrainAsync(
            It.IsAny<IEnumerable<CallTrumpTrainingData>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TrainingResult(null!, new RegressionEvaluationMetrics(0, 0, 0, 0, 0), 0, 0, 0));

        var mockLogger = Mock.Of<ILogger<CallTrumpRegressionTrainerExecutor>>();
        var executor = new CallTrumpRegressionTrainerExecutor(
            mockTrainer.Object,
            mockDataLoader.Object,
            Mock.Of<IIdvFileService>(),
            mockLogger);

        var result = await executor.ExecuteAsync(
            "./models",
            1000,
            1,
            new Progress<TrainingProgress>(),
            cancellationToken: TestContext.Current.CancellationToken);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("No training data available");
    }

    [Fact]
    public void ModelType_ReturnsCorrectValue()
    {
        var mockTrainer = new Mock<IModelTrainer<CallTrumpTrainingData>>();
        var mockDataLoader = new Mock<ITrainingDataLoader<CallTrumpTrainingData>>();
        var mockLogger = Mock.Of<ILogger<CallTrumpRegressionTrainerExecutor>>();

        var executor = new CallTrumpRegressionTrainerExecutor(
            mockTrainer.Object,
            mockDataLoader.Object,
            Mock.Of<IIdvFileService>(),
            mockLogger);

        executor.ModelType.Should().Be("CallTrumpRegression");
        executor.DecisionType.Should().Be(DecisionType.CallTrump);
    }

    [Fact]
    public async Task ExecuteAsync_WithIdvFilePath_LoadsFromIdv()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var mockTrainer = new Mock<IModelTrainer<CallTrumpTrainingData>>();
            var mockDataLoader = new Mock<ITrainingDataLoader<CallTrumpTrainingData>>();
            var mockIdvFileService = new Mock<IIdvFileService>();
            var mockDataView = new Mock<Microsoft.ML.IDataView>();

            mockIdvFileService.Setup(s => s.Load(tempFile)).Returns(mockDataView.Object);

            mockTrainer.Setup(t => t.TrainAsync(
                It.IsAny<Microsoft.ML.IDataView>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TrainingResult(null!, new RegressionEvaluationMetrics(0, 0, 0, 0, 0), 100, 70, 30));

            var executor = new CallTrumpRegressionTrainerExecutor(
                mockTrainer.Object,
                mockDataLoader.Object,
                mockIdvFileService.Object,
                Mock.Of<ILogger<CallTrumpRegressionTrainerExecutor>>());

            await executor.ExecuteAsync(
                "./models",
                1000,
                1,
                new Progress<TrainingProgress>(),
                idvFilePath: tempFile,
                cancellationToken: TestContext.Current.CancellationToken);

            mockIdvFileService.Verify(s => s.Load(tempFile), Times.Once);
            mockTrainer.Verify(
                t => t.TrainAsync(
                    mockDataView.Object,
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            mockDataLoader.Verify(
                l => l.StreamTrainingData(
                    It.IsAny<int>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithNullIdvFilePath_UsesDataLoader()
    {
        var mockTrainer = new Mock<IModelTrainer<CallTrumpTrainingData>>();
        var mockDataLoader = new Mock<ITrainingDataLoader<CallTrumpTrainingData>>();
        var mockIdvFileService = new Mock<IIdvFileService>();

        mockDataLoader.Setup(l => l.StreamTrainingData(
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Returns([]);

        mockTrainer.Setup(t => t.TrainAsync(
            It.IsAny<IEnumerable<CallTrumpTrainingData>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TrainingResult(null!, new RegressionEvaluationMetrics(0, 0, 0, 0, 0), 0, 0, 0));

        var executor = new CallTrumpRegressionTrainerExecutor(
            mockTrainer.Object,
            mockDataLoader.Object,
            mockIdvFileService.Object,
            Mock.Of<ILogger<CallTrumpRegressionTrainerExecutor>>());

        await executor.ExecuteAsync(
            "./models",
            1000,
            1,
            new Progress<TrainingProgress>(),
            idvFilePath: null,
            cancellationToken: TestContext.Current.CancellationToken);

        mockDataLoader.Verify(
            l => l.StreamTrainingData(
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        mockIdvFileService.Verify(s => s.Load(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentIdvFilePath_UsesDataLoader()
    {
        var mockTrainer = new Mock<IModelTrainer<CallTrumpTrainingData>>();
        var mockDataLoader = new Mock<ITrainingDataLoader<CallTrumpTrainingData>>();
        var mockIdvFileService = new Mock<IIdvFileService>();

        mockDataLoader.Setup(l => l.StreamTrainingData(
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .Returns([]);

        mockTrainer.Setup(t => t.TrainAsync(
            It.IsAny<IEnumerable<CallTrumpTrainingData>>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TrainingResult(null!, new RegressionEvaluationMetrics(0, 0, 0, 0, 0), 0, 0, 0));

        var executor = new CallTrumpRegressionTrainerExecutor(
            mockTrainer.Object,
            mockDataLoader.Object,
            mockIdvFileService.Object,
            Mock.Of<ILogger<CallTrumpRegressionTrainerExecutor>>());

        await executor.ExecuteAsync(
            "./models",
            1000,
            1,
            new Progress<TrainingProgress>(),
            idvFilePath: "/nonexistent/path/file.idv",
            cancellationToken: TestContext.Current.CancellationToken);

        mockDataLoader.Verify(
            l => l.StreamTrainingData(
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        mockIdvFileService.Verify(s => s.Load(It.IsAny<string>()), Times.Never);
    }
}
