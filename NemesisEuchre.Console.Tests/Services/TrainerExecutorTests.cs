using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services.TrainerExecutors;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Tests.Services;

public class TrainerExecutorTests
{
    [Fact]
    public void ModelType_ReturnsCorrectValue()
    {
        var mockTrainer = new Mock<IModelTrainer<CallTrumpTrainingData>>();
        var mockLogger = Mock.Of<ILogger<CallTrumpRegressionTrainerExecutor>>();

        var executor = new CallTrumpRegressionTrainerExecutor(
            mockTrainer.Object,
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
                mockIdvFileService.Object,
                Mock.Of<ILogger<CallTrumpRegressionTrainerExecutor>>());

            await executor.ExecuteAsync(
                "./models",
                1000,
                "test-model",
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
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentIdvFilePath_ReturnsFailureResult()
    {
        var mockTrainer = new Mock<IModelTrainer<CallTrumpTrainingData>>();
        var mockIdvFileService = new Mock<IIdvFileService>();

        var executor = new CallTrumpRegressionTrainerExecutor(
            mockTrainer.Object,
            mockIdvFileService.Object,
            Mock.Of<ILogger<CallTrumpRegressionTrainerExecutor>>());

        var result = await executor.ExecuteAsync(
            "./models",
            1000,
            "test-model",
            new Progress<TrainingProgress>(),
            idvFilePath: "/nonexistent/path/file.idv",
            cancellationToken: TestContext.Current.CancellationToken);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("IDV file not found");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullIdvFilePath_ReturnsFailureResult()
    {
        var mockTrainer = new Mock<IModelTrainer<CallTrumpTrainingData>>();
        var mockIdvFileService = new Mock<IIdvFileService>();

        var executor = new CallTrumpRegressionTrainerExecutor(
            mockTrainer.Object,
            mockIdvFileService.Object,
            Mock.Of<ILogger<CallTrumpRegressionTrainerExecutor>>());

        var result = await executor.ExecuteAsync(
            "./models",
            1000,
            "test-model",
            new Progress<TrainingProgress>(),
            idvFilePath: null!,
            cancellationToken: TestContext.Current.CancellationToken);

        result.Success.Should().BeFalse();
    }
}
