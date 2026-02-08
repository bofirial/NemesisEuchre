using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NemesisEuchre.Console.Commands;
using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Options;

using Spectre.Console.Testing;

namespace NemesisEuchre.Console.Tests.Commands;

public class TrainCommandTests
{
    [Fact]
    public async Task RunAsync_WhenOutputPathNotConfigured_ReturnsErrorExitCode()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<TrainCommand>>();
        var mockProgressCoordinator = new Mock<ITrainingProgressCoordinator>();
        var mockRenderer = new Mock<ITrainingResultsRenderer>();

        var options = Options.Create(new MachineLearningOptions { ModelOutputPath = string.Empty });

        var command = new TrainCommand(
            mockLogger,
            testConsole,
            mockProgressCoordinator.Object,
            mockRenderer.Object,
            options)
        {
            ActorType = ActorType.Gen1,
            DecisionType = DecisionType.All,
            OutputPath = null,
        };

        var exitCode = await command.RunAsync();

        exitCode.Should().Be(1);
        testConsole.Output.Should().Contain("Model output path is not configured");
    }

    [Fact]
    public async Task RunAsync_WhenTrainingSucceeds_ReturnsSuccessExitCode()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<TrainCommand>>();
        var mockProgressCoordinator = new Mock<ITrainingProgressCoordinator>();
        var mockRenderer = new Mock<ITrainingResultsRenderer>();

        var trainingResults = new TrainingResults(
            SuccessfulModels: 3,
            FailedModels: 0,
            Results: [],
            TotalDuration: TimeSpan.FromSeconds(10));

        mockProgressCoordinator.Setup(o => o.CoordinateTrainingWithProgressAsync(
            It.IsAny<ActorType>(),
            It.IsAny<DecisionType>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Spectre.Console.IAnsiConsole>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(trainingResults);

        var options = Options.Create(new MachineLearningOptions { ModelOutputPath = "./models" });

        var command = new TrainCommand(
            mockLogger,
            testConsole,
            mockProgressCoordinator.Object,
            mockRenderer.Object,
            options)
        {
            ActorType = ActorType.Gen1,
            DecisionType = DecisionType.All,
            OutputPath = null,
        };

        var exitCode = await command.RunAsync();

        exitCode.Should().Be(0);
        mockRenderer.Verify(
            r => r.RenderTrainingResults(
            trainingResults,
            ActorType.Gen1,
            DecisionType.All), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenTrainingFails_ReturnsFailureExitCode()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<TrainCommand>>();
        var mockProgressCoordinator = new Mock<ITrainingProgressCoordinator>();
        var mockRenderer = new Mock<ITrainingResultsRenderer>();

        var trainingResults = new TrainingResults(
            SuccessfulModels: 2,
            FailedModels: 1,
            Results: [],
            TotalDuration: TimeSpan.FromSeconds(10));

        mockProgressCoordinator.Setup(o => o.CoordinateTrainingWithProgressAsync(
            It.IsAny<ActorType>(),
            It.IsAny<DecisionType>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Spectre.Console.IAnsiConsole>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(trainingResults);

        var options = Options.Create(new MachineLearningOptions { ModelOutputPath = "./models" });

        var command = new TrainCommand(
            mockLogger,
            testConsole,
            mockProgressCoordinator.Object,
            mockRenderer.Object,
            options)
        {
            ActorType = ActorType.Gen1,
            DecisionType = DecisionType.CallTrump,
            OutputPath = "./custom-models",
        };

        var exitCode = await command.RunAsync();

        exitCode.Should().Be(2);
    }

    [Fact]
    public async Task RunAsync_UsesCustomOutputPathWhenProvided()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<TrainCommand>>();
        var mockProgressCoordinator = new Mock<ITrainingProgressCoordinator>();
        var mockRenderer = new Mock<ITrainingResultsRenderer>();

        var trainingResults = new TrainingResults(1, 0, [], TimeSpan.FromSeconds(5));

        mockProgressCoordinator.Setup(o => o.CoordinateTrainingWithProgressAsync(
            ActorType.Gen1,
            DecisionType.CallTrump,
            "./custom-models",
            1000,
            2,
            It.IsAny<Spectre.Console.IAnsiConsole>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(trainingResults);

        var options = Options.Create(new MachineLearningOptions { ModelOutputPath = "./default-models" });

        var command = new TrainCommand(
            mockLogger,
            testConsole,
            mockProgressCoordinator.Object,
            mockRenderer.Object,
            options)
        {
            ActorType = ActorType.Gen1,
            DecisionType = DecisionType.CallTrump,
            OutputPath = "./custom-models",
            SampleLimit = 1000,
            Generation = 2,
        };

        await command.RunAsync();

        mockProgressCoordinator.Verify(
            o => o.CoordinateTrainingWithProgressAsync(
            ActorType.Gen1,
            DecisionType.CallTrump,
            "./custom-models",
            1000,
            2,
            It.IsAny<Spectre.Console.IAnsiConsole>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
