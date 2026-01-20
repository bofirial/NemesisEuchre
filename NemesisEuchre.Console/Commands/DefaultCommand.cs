using DotMake.CommandLine;

using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace NemesisEuchre.Console.Commands;

[CliCommand(Description = "Nemesis Euchre")]
public class DefaultCommand(ILogger<DefaultCommand> logger, IAnsiConsole ansiConsole) : ICliRunAsyncWithContextAndReturn
{
    public async Task<int> RunAsync(CliContext cliContext)
    {
        LoggerMessages.LogStartingUp(logger);

        ansiConsole.Write(
            new FigletText("Nemesis Euchre")
                .Centered()
                .Color(Color.Blue));
        ansiConsole.WriteLine();

        ansiConsole.MarkupLine("[green]Welcome to NemesisEuchre - AI-Powered Euchre Strategy[/]");
        ansiConsole.WriteLine();

        return 0;
    }
}
