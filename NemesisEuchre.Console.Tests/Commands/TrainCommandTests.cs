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

using MicrosoftOptions = Microsoft.Extensions.Options.Options;

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

        var options = MicrosoftOptions.Create(new MachineLearningOptions { ModelOutputPath = string.Empty });

        var command = new TrainCommand(
            mockLogger,
            testConsole,
            mockProgressCoordinator.Object,
            mockRenderer.Object,
            options)
        {
            DecisionType = DecisionType.All,
            ModelName = "gen1",
            Source = "gen1",
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
            It.IsAny<DecisionType>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Spectre.Console.IAnsiConsole>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<IOptions<MachineLearningOptions>?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(trainingResults);

        var options = MicrosoftOptions.Create(new MachineLearningOptions { ModelOutputPath = "models" });

        var command = new TrainCommand(
            mockLogger,
            testConsole,
            mockProgressCoordinator.Object,
            mockRenderer.Object,
            options)
        {
            DecisionType = DecisionType.All,
            ModelName = "gen1",
            Source = "gen1",
        };

        var exitCode = await command.RunAsync();

        exitCode.Should().Be(0);
        mockRenderer.Verify(
            r => r.RenderTrainingResults(
            trainingResults,
            DecisionType.All),
            Times.Once);
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
            It.IsAny<DecisionType>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Spectre.Console.IAnsiConsole>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<IOptions<MachineLearningOptions>?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(trainingResults);

        var options = MicrosoftOptions.Create(new MachineLearningOptions { ModelOutputPath = "models" });

        var command = new TrainCommand(
            mockLogger,
            testConsole,
            mockProgressCoordinator.Object,
            mockRenderer.Object,
            options)
        {
            DecisionType = DecisionType.CallTrump,
            ModelName = "gen1",
            Source = "gen1",
        };

        var exitCode = await command.RunAsync();

        exitCode.Should().Be(2);
    }
}
