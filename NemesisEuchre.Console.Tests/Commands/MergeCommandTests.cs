using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Commands;
using NemesisEuchre.Console.Services;

using Spectre.Console.Testing;

namespace NemesisEuchre.Console.Tests.Commands;

public class MergeCommandTests
{
    [Fact]
    public async Task RunAsync_WhenSourceCountIsOne_ReturnsErrorExitCode()
    {
        var testConsole = new TestConsole();
        var mockMergeService = new Mock<IIdvMergeService>();

        var command = new MergeCommand(
            Mock.Of<ILogger<MergeCommand>>(),
            testConsole,
            mockMergeService.Object)
        {
            Source = ["source1"],
            Output = "merged",
        };

        var exitCode = await command.RunAsync();

        exitCode.Should().Be(1);
        testConsole.Output.Should().Contain("At least 2 source generation names are required");
        mockMergeService.Verify(
            s => s.MergeAsync(
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<Action<string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RunAsync_WhenMergeSucceeds_ReturnsZero()
    {
        var testConsole = new TestConsole();
        var mockMergeService = new Mock<IIdvMergeService>();

        mockMergeService
            .Setup(s => s.MergeAsync(
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<Action<string>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new MergeCommand(
            Mock.Of<ILogger<MergeCommand>>(),
            testConsole,
            mockMergeService.Object)
        {
            Source = ["source1", "source2"],
            Output = "merged",
        };

        var exitCode = await command.RunAsync();

        exitCode.Should().Be(0);
        testConsole.Output.Should().Contain("Successfully merged");
    }

    [Fact]
    public async Task RunAsync_WhenMergeServiceThrowsFileNotFound_ReturnsErrorExitCode()
    {
        var testConsole = new TestConsole();
        var mockMergeService = new Mock<IIdvMergeService>();

        mockMergeService
            .Setup(s => s.MergeAsync(
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<Action<string>?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("Source IDV file not found: /data/gen1_PlayCard.idv"));

        var command = new MergeCommand(
            Mock.Of<ILogger<MergeCommand>>(),
            testConsole,
            mockMergeService.Object)
        {
            Source = ["source1", "source2"],
            Output = "merged",
        };

        var exitCode = await command.RunAsync();

        exitCode.Should().Be(1);
        testConsole.Output.Should().Contain("Error");
    }
}
