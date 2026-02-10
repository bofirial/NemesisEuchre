using FluentAssertions;

using Moq;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;

using NemesisEuchre.Foundation.Constants;

using Spectre.Console.Testing;

namespace NemesisEuchre.Console.Tests.Services;

public class TrainingProgressCoordinatorTests : IDisposable
{
    private readonly Mock<IModelTrainingOrchestrator> _mockOrchestrator = new();
    private readonly TrainingProgressCoordinator _coordinator;
    private readonly TestConsole _testConsole;
    private bool _disposed;

    public TrainingProgressCoordinatorTests()
    {
        _coordinator = new TrainingProgressCoordinator(_mockOrchestrator.Object);
        _testConsole = new TestConsole();
    }

    [Fact]
    public async Task CoordinateTrainingWithProgressAsync_CallsOrchestrator()
    {
        var expectedResults = new TrainingResults(1, 0, [], TimeSpan.FromSeconds(1));

        _mockOrchestrator
            .Setup(x => x.TrainModelsAsync(
                ActorType.Chaos,
                DecisionType.CallTrump,
                "./models",
                1000,
                1,
                It.IsAny<IProgress<TrainingProgress>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        var result = await _coordinator.CoordinateTrainingWithProgressAsync(ActorType.Chaos, DecisionType.CallTrump, "./models", 1000, 1, _testConsole, cancellationToken: TestContext.Current.CancellationToken);

        result.Should().BeSameAs(expectedResults);

        _mockOrchestrator.Verify(
            x => x.TrainModelsAsync(
                ActorType.Chaos,
                DecisionType.CallTrump,
                "./models",
                1000,
                1,
                It.IsAny<IProgress<TrainingProgress>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CoordinateTrainingWithProgressAsync_PassesCorrectParameters()
    {
        var expectedResults = new TrainingResults(3, 0, [], TimeSpan.FromSeconds(2));
        ActorType? capturedActorType = null;
        DecisionType? capturedDecisionType = null;
        string? capturedOutputPath = null;
        int? capturedSampleLimit = null;
        int? capturedGeneration = null;

        _mockOrchestrator
            .Setup(x => x.TrainModelsAsync(
                It.IsAny<ActorType>(),
                It.IsAny<DecisionType>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<IProgress<TrainingProgress>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Callback<ActorType, DecisionType, string, int, int, IProgress<TrainingProgress>, string?, CancellationToken>(
                (actor, decision, path, limit, gen, _, _, _) =>
                {
                    capturedActorType = actor;
                    capturedDecisionType = decision;
                    capturedOutputPath = path;
                    capturedSampleLimit = limit;
                    capturedGeneration = gen;
                })
            .ReturnsAsync(expectedResults);

        await _coordinator.CoordinateTrainingWithProgressAsync(ActorType.Beta, DecisionType.All, "./output", 5000, 2, _testConsole, cancellationToken: TestContext.Current.CancellationToken);

        capturedActorType.Should().Be(ActorType.Beta);
        capturedDecisionType.Should().Be(DecisionType.All);
        capturedOutputPath.Should().Be("./output");
        capturedSampleLimit.Should().Be(5000);
        capturedGeneration.Should().Be(2);
    }

    [Fact]
    public async Task CoordinateTrainingWithProgressAsync_RespectsCancellationToken()
    {
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _mockOrchestrator
            .Setup(x => x.TrainModelsAsync(
                It.IsAny<ActorType>(),
                It.IsAny<DecisionType>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<IProgress<TrainingProgress>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var act = async () => await _coordinator.CoordinateTrainingWithProgressAsync(
            ActorType.Chaos,
            DecisionType.CallTrump,
            "./models",
            1000,
            1,
            _testConsole,
            cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();

        cts.Dispose();
    }

    [Fact]
    public Task CoordinateTrainingWithProgressAsync_PropagatesExceptions()
    {
        _mockOrchestrator
            .Setup(x => x.TrainModelsAsync(
                It.IsAny<ActorType>(),
                It.IsAny<DecisionType>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<IProgress<TrainingProgress>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Training failed"));

        var act = async () => await _coordinator.CoordinateTrainingWithProgressAsync(
            ActorType.Chaos,
            DecisionType.CallTrump,
            "./models",
            1000,
            1,
            _testConsole);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Training failed");
    }

    [Fact]
    public async Task CoordinateTrainingWithProgressAsync_ReturnsOrchestratorResult()
    {
        var modelResults = new List<ModelTrainingResult>
        {
            new(
                ModelType: "CallTrumpRegression",
                Success: true,
                ModelPath: null,
                ErrorMessage: null,
                MeanAbsoluteError: null,
                RSquared: null),
        };

        var expectedResults = new TrainingResults(1, 0, modelResults, TimeSpan.FromSeconds(10));

        _mockOrchestrator
            .Setup(x => x.TrainModelsAsync(
                It.IsAny<ActorType>(),
                It.IsAny<DecisionType>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<IProgress<TrainingProgress>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        var result = await _coordinator.CoordinateTrainingWithProgressAsync(ActorType.Chaos, DecisionType.CallTrump, "./models", 0, 1, _testConsole, cancellationToken: TestContext.Current.CancellationToken);

        result.Should().BeSameAs(expectedResults);
        result.SuccessfulModels.Should().Be(1);
        result.FailedModels.Should().Be(0);
        result.Results.Should().ContainSingle();
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
            if (disposing)
            {
                _testConsole.Dispose();
            }

            _disposed = true;
        }
    }
}
