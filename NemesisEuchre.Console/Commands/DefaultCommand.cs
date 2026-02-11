using DotMake.CommandLine;

using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Models;
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

    [CliOption(Description = "Persist games to SQL database")]
    public bool PersistToSql { get; set; }

    [CliOption(Description = "Persist training data to IDV files with the given generation name (e.g. gen2)", Required = false)]
    public string? PersistToIdv { get; set; }

    [CliOption(Description = "Show decisions made during the game")]
    public bool ShowDecisions { get; set; }

    [CliOption(Description = "ActorType for Team1")]
    public ActorType? Team1 { get; set; }

    [CliOption(Description = "ActorType for Team2")]
    public ActorType? Team2 { get; set; }

    [CliOption(Description = "ModelName for Team1 ModelBots", Alias = "t1m")]
    public string? Team1ModelName { get; set; }

    [CliOption(Description = "ExplorationTemperature for Team1 ModelTrainerBots", Alias = "t1t")]
    public float Team1ExplorationTemperature { get; set; }

    [CliOption(Description = "ModelName for Team2 ModelBots", Alias = "t2m")]
    public string? Team2ModelName { get; set; }

    [CliOption(Description = "ExplorationTemperature for Team2 ModelTrainerBots", Alias = "t2t")]
    public float Team2ExplorationTemperature { get; set; }

    public async Task<int> RunAsync()
    {
        Foundation.LoggerMessages.LogStartingUp(logger);

        applicationBanner.Display();

        ansiConsole.MarkupLine("[green]Welcome to NemesisEuchre - AI-Powered Euchre Strategy[/]");

        var persistenceOptions = new GamePersistenceOptions(PersistToSql, PersistToIdv);

        if (Count == 1)
        {
            await RunSingleGameAsync(persistenceOptions);
        }
        else
        {
            await RunBatchGamesAsync(persistenceOptions);
        }

        return 0;
    }

    private static Actor? GetTeamActor(ActorType? teamActorType, string? teamModelName, float teamExplorationTemperature)
    {
        if (teamExplorationTemperature != default)
        {
            teamActorType = ActorType.ModelTrainer;
        }
        else if (!string.IsNullOrEmpty(teamModelName))
        {
            teamActorType = ActorType.Model;
        }

        return teamActorType != null ? new Actor(teamActorType.Value, teamModelName, teamExplorationTemperature) : null;
    }

    private Actor[]? GetTeamActors(Team team)
    {
        var teamActor = team switch
        {
            Team.Team1 => GetTeamActor(Team1, Team1ModelName, Team1ExplorationTemperature),
            Team.Team2 => GetTeamActor(Team2, Team2ModelName, Team2ExplorationTemperature),
            _ => throw new ArgumentOutOfRangeException(nameof(team), team, $"Invalid Team: {team}"),
        };
        return teamActor != null ? [teamActor, teamActor] : null;
    }

    private Task<Game> RunSingleGameAsync(GamePersistenceOptions persistenceOptions)
    {
        ansiConsole.MarkupLine($"[dim]Playing a game between 2 {Team1 ?? ActorType.Chaos}Bots and 2 {Team2 ?? ActorType.Chaos}Bots...[/]");
        ansiConsole.WriteLine();

        return ansiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Playing game...", async _ => await singleGameRunner.RunAsync(persistenceOptions: persistenceOptions, team1Actors: GetTeamActors(Team.Team1), team2Actors: GetTeamActors(Team.Team2), showDecisions: ShowDecisions));
    }

    private async Task RunBatchGamesAsync(GamePersistenceOptions persistenceOptions)
    {
        ansiConsole.MarkupLine($"[dim]Playing a game between 2 {Team1 ?? ActorType.Chaos}Bots and 2 {Team2 ?? ActorType.Chaos}Bots...[/]");
        ansiConsole.WriteLine();

        var results = await ansiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var playingTask = ctx.AddTask("[green]Playing " + Count + " games...[/]", maxValue: Count);
                var savingTask = ctx.AddTask("[blue]Saving games...[/]", maxValue: Count);

                var progressReporter = new BatchProgressReporter(playingTask, savingTask);

                return await batchGameOrchestrator.RunBatchAsync(Count, progressReporter: progressReporter, persistenceOptions: persistenceOptions, team1Actors: GetTeamActors(Team.Team1), team2Actors: GetTeamActors(Team.Team2));
            });

        gameResultsRenderer.RenderBatchResults(results);
    }
}
