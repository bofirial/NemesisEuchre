using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;
using NemesisEuchre.Console.Services.TrainerExecutors;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.Console.Tests.Services;

public class ModelTrainingOrchestratorTests
{
    [Fact]
    public async Task TrainModelsAsync_WhenNoTrainersFound_ReturnsEmptyResults()
    {
        var mockFactory = new Mock<ITrainerFactory>();
        mockFactory.Setup(f => f.GetTrainers(It.IsAny<DecisionType>()))
            .Returns([]);

        var mockLogger = Mock.Of<ILogger<ModelTrainingOrchestrator>>();
        var orchestrator = new ModelTrainingOrchestrator(mockFactory.Object, Options.Create(new PersistenceOptions()), mockLogger);

        var results = await orchestrator.TrainModelsAsync(
            ActorType.Gen1,
            DecisionType.CallTrump,
            "./models",
            0,
            1,
            new Progress<TrainingProgress>(),
            cancellationToken: TestContext.Current.CancellationToken);

        results.SuccessfulModels.Should().Be(0);
        results.FailedModels.Should().Be(0);
        results.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task TrainModelsAsync_WhenTrainerSucceeds_ReturnsSuccessResult()
    {
        var successResult = new ModelTrainingResult(
            "TestModel",
            true,
            ModelPath: "./models/Gen1_TestModel_Gen1.zip",
            MeanAbsoluteError: 0.5,
            RSquared: 0.85);

        var mockTrainer = new Mock<ITrainerExecutor>();
        mockTrainer.Setup(t => t.ModelType).Returns("TestModel");
        mockTrainer.Setup(t => t.DecisionType).Returns(DecisionType.CallTrump);
        mockTrainer.Setup(t => t.ExecuteAsync(
            It.IsAny<ActorType>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<IProgress<TrainingProgress>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        var mockFactory = new Mock<ITrainerFactory>();
        mockFactory.Setup(f => f.GetTrainers(DecisionType.CallTrump))
            .Returns([mockTrainer.Object]);

        var mockLogger = Mock.Of<ILogger<ModelTrainingOrchestrator>>();
        var orchestrator = new ModelTrainingOrchestrator(mockFactory.Object, Options.Create(new PersistenceOptions()), mockLogger);

        var results = await orchestrator.TrainModelsAsync(
            ActorType.Gen1,
            DecisionType.CallTrump,
            "./models",
            1000,
            1,
            new Progress<TrainingProgress>(),
            cancellationToken: TestContext.Current.CancellationToken);

        results.SuccessfulModels.Should().Be(1);
        results.FailedModels.Should().Be(0);
        results.Results.Should().HaveCount(1);
        results.Results[0].Success.Should().BeTrue();
        results.Results[0].ModelType.Should().Be("TestModel");
    }

    [Fact]
    public async Task TrainModelsAsync_WhenTrainerFails_ReturnsFailureResult()
    {
        var failureResult = new ModelTrainingResult(
            "TestModel",
            false,
            ErrorMessage: "Training failed");

        var mockTrainer = new Mock<ITrainerExecutor>();
        mockTrainer.Setup(t => t.ModelType).Returns("TestModel");
        mockTrainer.Setup(t => t.DecisionType).Returns(DecisionType.CallTrump);
        mockTrainer.Setup(t => t.ExecuteAsync(
            It.IsAny<ActorType>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<IProgress<TrainingProgress>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        var mockFactory = new Mock<ITrainerFactory>();
        mockFactory.Setup(f => f.GetTrainers(DecisionType.CallTrump))
            .Returns([mockTrainer.Object]);

        var mockLogger = Mock.Of<ILogger<ModelTrainingOrchestrator>>();
        var orchestrator = new ModelTrainingOrchestrator(mockFactory.Object, Options.Create(new PersistenceOptions()), mockLogger);

        var results = await orchestrator.TrainModelsAsync(
            ActorType.Gen1,
            DecisionType.CallTrump,
            "./models",
            1000,
            1,
            new Progress<TrainingProgress>(),
            cancellationToken: TestContext.Current.CancellationToken);

        results.SuccessfulModels.Should().Be(0);
        results.FailedModels.Should().Be(1);
        results.Results.Should().HaveCount(1);
        results.Results[0].Success.Should().BeFalse();
        results.Results[0].ErrorMessage.Should().Be("Training failed");
    }

    [Fact]
    public async Task TrainModelsAsync_WhenMultipleTrainers_ReturnsAggregatedResults()
    {
        var successResult = new ModelTrainingResult("Model1", true, ModelPath: "./models/Model1.zip");
        var failureResult = new ModelTrainingResult("Model2", false, ErrorMessage: "Failed");

        var mockTrainer1 = new Mock<ITrainerExecutor>();
        mockTrainer1.Setup(t => t.ModelType).Returns("Model1");
        mockTrainer1.Setup(t => t.ExecuteAsync(
            It.IsAny<ActorType>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<IProgress<TrainingProgress>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        var mockTrainer2 = new Mock<ITrainerExecutor>();
        mockTrainer2.Setup(t => t.ModelType).Returns("Model2");
        mockTrainer2.Setup(t => t.ExecuteAsync(
            It.IsAny<ActorType>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<IProgress<TrainingProgress>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        var mockFactory = new Mock<ITrainerFactory>();
        mockFactory.Setup(f => f.GetTrainers(DecisionType.All))
            .Returns([mockTrainer1.Object, mockTrainer2.Object]);

        var mockLogger = Mock.Of<ILogger<ModelTrainingOrchestrator>>();
        var orchestrator = new ModelTrainingOrchestrator(mockFactory.Object, Options.Create(new PersistenceOptions()), mockLogger);

        var results = await orchestrator.TrainModelsAsync(
            ActorType.Gen1,
            DecisionType.All,
            "./models",
            1000,
            1,
            new Progress<TrainingProgress>(),
            cancellationToken: TestContext.Current.CancellationToken);

        results.SuccessfulModels.Should().Be(1);
        results.FailedModels.Should().Be(1);
        results.Results.Should().HaveCount(2);
    }
}
