using DotMake.CommandLine;

using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Services;
using NemesisEuchre.GameEngine;

using Spectre.Console;

namespace NemesisEuchre.Console.Commands;

[CliCommand(Description = "Nemesis Euchre")]
public class DefaultCommand(
    ILogger<DefaultCommand> logger,
    IAnsiConsole ansiConsole,
    IApplicationBanner applicationBanner,
    IGameOrchestrator gameOrchestrator,
    IGameResultsRenderer gameResultsRenderer) : ICliRunAsyncWithReturn
{
    public async Task<int> RunAsync()
    {
        LoggerMessages.LogStartingUp(logger);

        applicationBanner.Display();

        ansiConsole.MarkupLine("[green]Welcome to NemesisEuchre - AI-Powered Euchre Strategy[/]");
        ansiConsole.MarkupLine("[dim]Playing a game between 4 ChaosBots...[/]");
        ansiConsole.WriteLine();

        var game = await ansiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Playing game...", async _ => await gameOrchestrator.OrchestrateGameAsync());

        gameResultsRenderer.RenderResults(game);

        return 0;
    }
}
