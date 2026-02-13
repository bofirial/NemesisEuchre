using FluentAssertions;

using Moq;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;

using NemesisEuchre.Foundation.Constants;

using Spectre.Console;
using Spectre.Console.Testing;

namespace NemesisEuchre.Console.Tests.Services;

public class TrainingProgressCoordinatorTests : IDisposable
{
    private readonly Mock<IModelTrainingOrchestrator> _mockOrchestrator = new();
    private readonly Mock<ITrainingResultsRenderer> _mockRenderer = new();
    private readonly TrainingProgressCoordinator _coordinator;
    private readonly TestConsole _testConsole;
    private bool _disposed;

    public TrainingProgressCoordinatorTests()
    {
        _mockRenderer
            .Setup(x => x.BuildLiveTrainingTable(It.IsAny<TrainingDisplaySnapshot>(), It.IsAny<TimeSpan>()))
            .Returns(new Text(string.Empty));

        _coordinator = new TrainingProgressCoordinator(_mockOrchestrator.Object, _mockRenderer.Object);
        _testConsole = new TestConsole();
    }

    [Fact]
    public async Task CoordinateTrainingWithProgressAsync_CallsOrchestrator()
    {
        var expectedResults = new TrainingResults(1, 0, [], TimeSpan.FromSeconds(1));

        _mockOrchestrator
            .Setup(x => x.TrainModelsAsync(
                DecisionType.CallTrump,
                "models",
                "gen1",
                It.IsAny<IProgress<TrainingProgress>>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        var result = await _coordinator.CoordinateTrainingWithProgressAsync(DecisionType.CallTrump, "models", "gen1", _testConsole, "gen1", cancellationToken: TestContext.Current.CancellationToken);

        result.Should().BeSameAs(expectedResults);

        _mockOrchestrator.Verify(
            x => x.TrainModelsAsync(
                DecisionType.CallTrump,
                "models",
                "gen1",
                It.IsAny<IProgress<TrainingProgress>>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CoordinateTrainingWithProgressAsync_PassesCorrectParameters()
    {
        var expectedResults = new TrainingResults(3, 0, [], TimeSpan.FromSeconds(2));
        DecisionType? capturedDecisionType = null;
        string? capturedOutputPath = null;
        string? capturedModelName = null;

        _mockOrchestrator
            .Setup(x => x.TrainModelsAsync(
                It.IsAny<DecisionType>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<TrainingProgress>>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .Callback<DecisionType, string, string, IProgress<TrainingProgress>, string, bool, CancellationToken>(
                (decision, path, modelName, _, _, _, _) =>
                {
                    capturedDecisionType = decision;
                    capturedOutputPath = path;
                    capturedModelName = modelName;
                })
            .ReturnsAsync(expectedResults);

        await _coordinator.CoordinateTrainingWithProgressAsync(DecisionType.All, "output", "gen2", _testConsole, "gen2", cancellationToken: TestContext.Current.CancellationToken);

        capturedDecisionType.Should().Be(DecisionType.All);
        capturedOutputPath.Should().Be("output");
        capturedModelName.Should().Be("gen2");
    }

    [Fact]
    public async Task CoordinateTrainingWithProgressAsync_RespectsCancellationToken()
    {
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _mockOrchestrator
            .Setup(x => x.TrainModelsAsync(
                It.IsAny<DecisionType>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<TrainingProgress>>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var act = async () => await _coordinator.CoordinateTrainingWithProgressAsync(
            DecisionType.CallTrump,
            "models",
            "gen1",
            _testConsole,
            "gen1",
            cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();

        cts.Dispose();
    }

    [Fact]
    public Task CoordinateTrainingWithProgressAsync_PropagatesExceptions()
    {
        _mockOrchestrator
            .Setup(x => x.TrainModelsAsync(
                It.IsAny<DecisionType>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<TrainingProgress>>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Training failed"));

        var act = async () => await _coordinator.CoordinateTrainingWithProgressAsync(
            DecisionType.CallTrump,
            "models",
            "gen1",
            _testConsole,
            "gen1");

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Training failed");
    }

    [Fact]
    public async Task CoordinateTrainingWithProgressAsync_ReturnsOrchestratorResult()
    {
        var modelResults = new List<ModelTrainingResult>
        {
            new(
                ModelType: "CallTrump",
                Success: true,
                ModelPath: null,
                ErrorMessage: null,
                MeanAbsoluteError: null,
                RSquared: null),
        };

        var expectedResults = new TrainingResults(1, 0, modelResults, TimeSpan.FromSeconds(10));

        _mockOrchestrator
            .Setup(x => x.TrainModelsAsync(
                It.IsAny<DecisionType>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IProgress<TrainingProgress>>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        var result = await _coordinator.CoordinateTrainingWithProgressAsync(DecisionType.CallTrump, "models", "gen1", _testConsole, "gen1", cancellationToken: TestContext.Current.CancellationToken);

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
