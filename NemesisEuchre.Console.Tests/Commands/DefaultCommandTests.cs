using System.Diagnostics.CodeAnalysis;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Commands;

using Spectre.Console.Testing;

namespace NemesisEuchre.Console.Tests.Commands;

public class DefaultCommandTests
{
    [Fact]
    public async Task RunAsyncShouldOutputNemesisEuchreTitleAsync()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var command = new DefaultCommand(mockLogger, testConsole);
        await command.RunAsync(null!);

        VerifyOutputContainsNemesisEuchreLogo(testConsole);
    }

    [Fact]
    public async Task RunAsyncShouldOutputWelcomeMessageAsync()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var command = new DefaultCommand(mockLogger, testConsole);
        await command.RunAsync(null!);

        testConsole.Output.Should().Contain("Welcome to NemesisEuchre - AI-Powered Euchre Strategy");
    }

    [Fact]
    public async Task RunAsyncShouldReturnZeroAsync()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var command = new DefaultCommand(mockLogger, testConsole);
        var result = await command.RunAsync(null!);

        result.Should().Be(0);
    }

    [SuppressMessage(
        "Roslynator",
        "RCS1192:Unnecessary usage of verbatim string literal",
        Justification = "Verbatim string is not necessary for each line but removing it ruins the ascii art and makes the source code less readable")]
    private static void VerifyOutputContainsNemesisEuchreLogo(TestConsole testConsole)
    {
        testConsole.Output.Should().Contain(@" _   _                                   _       ");
        testConsole.Output.Should().Contain(@"| \ | |   ___   _ __ ___     ___   ___  (_)  ___ ");
        testConsole.Output.Should().Contain(@"|  \| |  / _ \ | '_ ` _ \   / _ \ / __| | | / __|");
        testConsole.Output.Should().Contain(@"| |\  | |  __/ | | | | | | |  __/ \__ \ | | \__ \");
        testConsole.Output.Should().Contain(@"|_| \_|  \___| |_| |_| |_|  \___| |___/ |_| |___/");

        testConsole.Output.Should().Contain(@" _____                  _");
        testConsole.Output.Should().Contain(@"| ____|  _   _    ___  | |__    _ __    ___");
        testConsole.Output.Should().Contain(@"|  _|   | | | |  / __| | '_ \  | '__|  / _ \");
        testConsole.Output.Should().Contain(@"| |___  | |_| | | (__  | | | | | |    |  __/");
        testConsole.Output.Should().Contain(@"|_____|  \__,_|  \___| |_| |_| |_|     \___|");
    }
}
