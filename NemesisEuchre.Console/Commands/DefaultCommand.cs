using DotMake.CommandLine;

using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Services;

using Spectre.Console;

namespace NemesisEuchre.Console.Commands;

[CliCommand(Description = "Nemesis Euchre")]
public class DefaultCommand(ILogger<DefaultCommand> logger, IAnsiConsole ansiConsole, IApplicationBanner applicationBanner) : ICliRunAsyncWithContextAndReturn
{
    public async Task<int> RunAsync(CliContext cliContext)
    {
        LoggerMessages.LogStartingUp(logger);

        applicationBanner.Display();

        ansiConsole.MarkupLine("[green]Welcome to NemesisEuchre - AI-Powered Euchre Strategy[/]");
        ansiConsole.WriteLine();

        return 0;
    }
}
