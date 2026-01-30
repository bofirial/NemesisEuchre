using DotMake.CommandLine;

using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Services;
using NemesisEuchre.Foundation;
using NemesisEuchre.GameEngine.Models;

using Spectre.Console;

namespace NemesisEuchre.Console.Commands;

[CliCommand(Description = "Nemesis Euchre")]
public class DefaultCommand(
    ILogger<DefaultCommand> logger,
    IAnsiConsole ansiConsole,
    IApplicationBanner applicationBanner,
    ISingleGameRunner singleGameRunner,
    IBatchGameOrchestrator batchGameOrchestrator,
    IGameResultsRenderer gameResultsRenderer) : ICliRunAsyncWithReturn
{
    [CliOption(Description = "Number of games to play")]
    public int Count { get; set; } = 1;

    public async Task<int> RunAsync()
    {
        LoggerMessages.LogStartingUp(logger);

        applicationBanner.Display();

        ansiConsole.MarkupLine("[green]Welcome to NemesisEuchre - AI-Powered Euchre Strategy[/]");

        if (Count == 1)
        {
            await RunSingleGameAsync();
        }
        else
        {
            await RunBatchGamesAsync();
        }

        return 0;
    }

    private Task<Game> RunSingleGameAsync()
    {
        ansiConsole.MarkupLine("[dim]Playing a game between 4 ChaosBots...[/]");
        ansiConsole.WriteLine();

        return ansiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Playing game...", async _ => await singleGameRunner.RunAsync());
    }

    private async Task RunBatchGamesAsync()
    {
        ansiConsole.MarkupLine($"[dim]Playing {Count} games between 4 ChaosBots...[/]");
        ansiConsole.WriteLine();

        var results = await ansiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var playingTask = ctx.AddTask("[green]Playing " + Count + " games...[/]", maxValue: Count);
                var savingTask = ctx.AddTask("[blue]Saving games...[/]", maxValue: Count);

                var progressReporter = new BatchProgressReporter(playingTask, savingTask);

                return await batchGameOrchestrator.RunBatchAsync(Count, progressReporter: progressReporter);
            });

        gameResultsRenderer.RenderBatchResults(results);
    }
}
