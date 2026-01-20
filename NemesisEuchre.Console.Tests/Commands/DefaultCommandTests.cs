using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Commands;
using NemesisEuchre.Console.Services;

using Spectre.Console.Testing;

namespace NemesisEuchre.Console.Tests.Commands;

public class DefaultCommandTests
{
    [Fact]
    public async Task RunAsyncShouldDisplayApplicationBanner()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = new Mock<IApplicationBanner>();
        var command = new DefaultCommand(mockLogger, testConsole, mockBanner.Object);

        await command.RunAsync(null!);

        mockBanner.Verify(b => b.Display(), Times.Once);
    }

    [Fact]
    public async Task RunAsyncShouldOutputWelcomeMessage()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = new Mock<IApplicationBanner>();
        var command = new DefaultCommand(mockLogger, testConsole, mockBanner.Object);

        await command.RunAsync(null!);

        testConsole.Output.Should().Contain("Welcome to NemesisEuchre - AI-Powered Euchre Strategy");
    }

    [Fact]
    public async Task RunAsyncShouldReturnZero()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var command = new DefaultCommand(mockLogger, testConsole, mockBanner);

        var result = await command.RunAsync(null!);

        result.Should().Be(0);
    }
}
