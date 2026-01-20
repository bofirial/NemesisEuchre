using System.CommandLine;

using FluentAssertions;

using NemesisEuchre.Console.CommandActions;

namespace NemesisEuchre.Console.Tests.CommandActions;

public sealed class VersionCommandActionTests
{
    [Fact]
    public void ExecuteShouldNotThrow()
    {
        var command = new VersionCommandAction();

        var rootCommand = new RootCommand();
        var parseResult = rootCommand.Parse(string.Empty);

        command.Invoking(c => c.Invoke(parseResult))
            .Should().NotThrow();
    }
}
