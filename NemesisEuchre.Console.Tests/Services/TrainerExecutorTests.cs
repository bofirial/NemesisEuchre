using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services.TrainerExecutors;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.Console.Tests.Services;

public class TrainerExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_WhenNoDataAvailable_ReturnsFailureResult()
    {
        var mockTrainer = new Mock<IModelTrainer<CallTrumpTrainingData>>();
        var mockDataLoader = new Mock<ITrainingDataLoader<CallTrumpTrainingData>>();
        mockDataLoader.Setup(l => l.LoadTrainingDataAsync(
            It.IsAny<ActorType>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var mockLogger = Mock.Of<ILogger<CallTrumpRegressionTrainerExecutor>>();
        var executor = new CallTrumpRegressionTrainerExecutor(
            mockTrainer.Object,
            mockDataLoader.Object,
            mockLogger);

        var result = await executor.ExecuteAsync(
            ActorType.Gen1,
            "./models",
            1000,
            1,
            new Progress<TrainingProgress>());

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
            mockLogger);

        executor.ModelType.Should().Be("CallTrumpRegression");
        executor.DecisionType.Should().Be(DecisionType.CallTrump);
    }
}
