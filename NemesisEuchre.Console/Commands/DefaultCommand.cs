using DotMake.CommandLine;

using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Services;
using NemesisEuchre.Foundation.Constants;
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

    [CliOption(Description = "Skip saving games to the database")]
    public bool DoNotPersist { get; set; }

    [CliOption(Description = "ActorType for Team1")]
    public ActorType? Team1 { get; set; }

    [CliOption(Description = "ActorType for Team2")]
    public ActorType? Team2 { get; set; }

    public async Task<int> RunAsync()
    {
        Foundation.LoggerMessages.LogStartingUp(logger);

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
        ansiConsole.MarkupLine($"[dim]Playing a game between 2 {Team1 ?? ActorType.Chaos}Bots and 2 {Team2 ?? ActorType.Chaos}Bots...[/]");
        ansiConsole.WriteLine();

        var team1ActorTypes = Team1.HasValue ? new[] { Team1.Value, Team1.Value } : null;
        var team2ActorTypes = Team2.HasValue ? new[] { Team2.Value, Team2.Value } : null;

        return ansiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Playing game...", async _ => await singleGameRunner.RunAsync(doNotPersist: DoNotPersist, team1ActorTypes: team1ActorTypes, team2ActorTypes: team2ActorTypes));
    }

    private async Task RunBatchGamesAsync()
    {
        ansiConsole.MarkupLine($"[dim]Playing a game between 2 {Team1 ?? ActorType.Chaos}Bots and 2 {Team2 ?? ActorType.Chaos}Bots...[/]");
        ansiConsole.WriteLine();

        var results = await ansiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var playingTask = ctx.AddTask("[green]Playing " + Count + " games...[/]", maxValue: Count);
                var savingTask = ctx.AddTask("[blue]Saving games...[/]", maxValue: Count);

                var progressReporter = new BatchProgressReporter(playingTask, savingTask);

                var team1ActorTypes = Team1.HasValue ? new[] { Team1.Value, Team1.Value } : null;
                var team2ActorTypes = Team2.HasValue ? new[] { Team2.Value, Team2.Value } : null;

                return await batchGameOrchestrator.RunBatchAsync(Count, progressReporter: progressReporter, doNotPersist: DoNotPersist, team1ActorTypes: team1ActorTypes, team2ActorTypes: team2ActorTypes);
            });

        gameResultsRenderer.RenderBatchResults(results);
    }
}
