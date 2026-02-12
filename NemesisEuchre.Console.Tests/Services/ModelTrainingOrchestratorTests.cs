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

public class ModelTrainingOrchestratorTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), $"ModelTrainingOrchestratorTests_{Guid.NewGuid()}");
    private bool _disposed;

    [Fact]
    public async Task TrainModelsAsync_WhenNoTrainersFound_ReturnsEmptyResults()
    {
        var mockFactory = new Mock<ITrainerFactory>();
        mockFactory.Setup(f => f.GetTrainers(It.IsAny<DecisionType>()))
            .Returns([]);

        var mockLogger = Mock.Of<ILogger<ModelTrainingOrchestrator>>();
        var orchestrator = new ModelTrainingOrchestrator(mockFactory.Object, Options.Create(new PersistenceOptions()), mockLogger);

        var results = await orchestrator.TrainModelsAsync(
            DecisionType.CallTrump,
            "./models",
            "gen1",
            new Progress<TrainingProgress>(),
            "gen1",
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
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IProgress<TrainingProgress>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        var mockFactory = new Mock<ITrainerFactory>();
        mockFactory.Setup(f => f.GetTrainers(DecisionType.CallTrump))
            .Returns([mockTrainer.Object]);

        var mockLogger = Mock.Of<ILogger<ModelTrainingOrchestrator>>();
        var orchestrator = new ModelTrainingOrchestrator(mockFactory.Object, Options.Create(new PersistenceOptions()), mockLogger);

        var results = await orchestrator.TrainModelsAsync(
            DecisionType.CallTrump,
            "./models",
            "gen1",
            new Progress<TrainingProgress>(),
            "gen1",
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
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IProgress<TrainingProgress>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        var mockFactory = new Mock<ITrainerFactory>();
        mockFactory.Setup(f => f.GetTrainers(DecisionType.CallTrump))
            .Returns([mockTrainer.Object]);

        var mockLogger = Mock.Of<ILogger<ModelTrainingOrchestrator>>();
        var orchestrator = new ModelTrainingOrchestrator(mockFactory.Object, Options.Create(new PersistenceOptions()), mockLogger);

        var results = await orchestrator.TrainModelsAsync(
            DecisionType.CallTrump,
            "./models",
            "gen1",
            new Progress<TrainingProgress>(),
            "gen1",
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
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IProgress<TrainingProgress>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        var mockTrainer2 = new Mock<ITrainerExecutor>();
        mockTrainer2.Setup(t => t.ModelType).Returns("Model2");
        mockTrainer2.Setup(t => t.ExecuteAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IProgress<TrainingProgress>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        var mockFactory = new Mock<ITrainerFactory>();
        mockFactory.Setup(f => f.GetTrainers(DecisionType.All))
            .Returns([mockTrainer1.Object, mockTrainer2.Object]);

        var mockLogger = Mock.Of<ILogger<ModelTrainingOrchestrator>>();
        var orchestrator = new ModelTrainingOrchestrator(mockFactory.Object, Options.Create(new PersistenceOptions()), mockLogger);

        var results = await orchestrator.TrainModelsAsync(
            DecisionType.All,
            "./models",
            "gen1",
            new Progress<TrainingProgress>(),
            "gen1",
            cancellationToken: TestContext.Current.CancellationToken);

        results.SuccessfulModels.Should().Be(1);
        results.FailedModels.Should().Be(1);
        results.Results.Should().HaveCount(2);
    }

    [Fact]
    public async Task TrainModelsAsync_ThrowsInvalidOperationException_WhenModelFileExists()
    {
        Directory.CreateDirectory(_tempDirectory);
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "gen1_testmodel.zip"), "existing", TestContext.Current.CancellationToken);

        var orchestrator = CreateOrchestratorWithTrainer("TestModel", DecisionType.CallTrump);

        var act = () => orchestrator.TrainModelsAsync(
            DecisionType.CallTrump,
            _tempDirectory,
            "gen1",
            new Progress<TrainingProgress>(),
            "gen1",
            cancellationToken: TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Model files already exist*--overwrite*");
    }

    [Fact]
    public async Task TrainModelsAsync_ThrowsInvalidOperationException_WhenMetadataFileExists()
    {
        Directory.CreateDirectory(_tempDirectory);
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "gen1_testmodel.json"), "existing", TestContext.Current.CancellationToken);

        var orchestrator = CreateOrchestratorWithTrainer("TestModel", DecisionType.CallTrump);

        var act = () => orchestrator.TrainModelsAsync(
            DecisionType.CallTrump,
            _tempDirectory,
            "gen1",
            new Progress<TrainingProgress>(),
            "gen1",
            cancellationToken: TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Model files already exist*--overwrite*");
    }

    [Fact]
    public async Task TrainModelsAsync_DoesNotCallExecuteAsync_WhenGuardFails()
    {
        Directory.CreateDirectory(_tempDirectory);
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "gen1_testmodel.zip"), "existing", TestContext.Current.CancellationToken);

        var mockTrainer = new Mock<ITrainerExecutor>();
        mockTrainer.Setup(t => t.ModelType).Returns("TestModel");
        mockTrainer.Setup(t => t.DecisionType).Returns(DecisionType.CallTrump);

        var mockFactory = new Mock<ITrainerFactory>();
        mockFactory.Setup(f => f.GetTrainers(DecisionType.CallTrump))
            .Returns([mockTrainer.Object]);

        var orchestrator = new ModelTrainingOrchestrator(
            mockFactory.Object, Options.Create(new PersistenceOptions()), Mock.Of<ILogger<ModelTrainingOrchestrator>>());

        var act = () => orchestrator.TrainModelsAsync(
            DecisionType.CallTrump,
            _tempDirectory,
            "gen1",
            new Progress<TrainingProgress>(),
            "gen1",
            cancellationToken: TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
        mockTrainer.Verify(
            t => t.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<TrainingProgress>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TrainModelsAsync_Succeeds_WhenFilesExistAndAllowOverwriteIsTrue()
    {
        Directory.CreateDirectory(_tempDirectory);
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "gen1_testmodel.zip"), "existing", TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "gen1_testmodel.json"), "existing", TestContext.Current.CancellationToken);

        var successResult = new ModelTrainingResult("TestModel", true, ModelPath: "path.zip");
        var mockTrainer = new Mock<ITrainerExecutor>();
        mockTrainer.Setup(t => t.ModelType).Returns("TestModel");
        mockTrainer.Setup(t => t.DecisionType).Returns(DecisionType.CallTrump);
        mockTrainer.Setup(t => t.ExecuteAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IProgress<TrainingProgress>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        var mockFactory = new Mock<ITrainerFactory>();
        mockFactory.Setup(f => f.GetTrainers(DecisionType.CallTrump))
            .Returns([mockTrainer.Object]);

        var orchestrator = new ModelTrainingOrchestrator(
            mockFactory.Object, Options.Create(new PersistenceOptions()), Mock.Of<ILogger<ModelTrainingOrchestrator>>());

        var results = await orchestrator.TrainModelsAsync(
            DecisionType.CallTrump,
            _tempDirectory,
            "gen1",
            new Progress<TrainingProgress>(),
            "gen1",
            allowOverwrite: true,
            cancellationToken: TestContext.Current.CancellationToken);

        results.SuccessfulModels.Should().Be(1);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && Directory.Exists(_tempDirectory))
            {
                try
                {
                    Directory.Delete(_tempDirectory, true);
                }
#pragma warning disable S108
                catch
                {
                }
#pragma warning restore S108
            }

            _disposed = true;
        }
    }

    private static ModelTrainingOrchestrator CreateOrchestratorWithTrainer(string modelType, DecisionType decisionType)
    {
        var mockTrainer = new Mock<ITrainerExecutor>();
        mockTrainer.Setup(t => t.ModelType).Returns(modelType);
        mockTrainer.Setup(t => t.DecisionType).Returns(decisionType);

        var mockFactory = new Mock<ITrainerFactory>();
        mockFactory.Setup(f => f.GetTrainers(decisionType))
            .Returns([mockTrainer.Object]);

        return new ModelTrainingOrchestrator(
            mockFactory.Object, Options.Create(new PersistenceOptions()), Mock.Of<ILogger<ModelTrainingOrchestrator>>());
    }
}
