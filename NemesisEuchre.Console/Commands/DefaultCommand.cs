using System.Diagnostics;

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
    [CliOption(
        Description = "Number of games to play",
        Alias = "c")]
    public int Count { get; set; } = 1;

    [CliOption(
        Description = "Persist games to SQL database",
        Alias = "sql")]
    public bool PersistToSql { get; set; }

    [CliOption(
        Description = "Persist training data to IDV files with the given generation name (e.g. {gen2}_CallTrump.idv)",
        Required = false,
        Alias = "idv")]
    public string? PersistToIdv { get; set; }

    [CliOption(
        Description = "Show decisions made during the game",
        Alias = "s")]
    public bool ShowDecisions { get; set; }

    [CliOption(
        Description = "ActorType for Team1",
        Alias = "t1")]
    public ActorType? Team1 { get; set; }

    [CliOption(
        Description = "ActorType for Team2",
        Alias = "t2")]
    public ActorType? Team2 { get; set; }

    [CliOption(
        Description = "ModelName for Team1 ModelBots",
        Alias = "t1m")]
    public string? Team1ModelName { get; set; }

    [CliOption(
        Description = "ExplorationTemperature for Team1 ModelTrainerBots",
        Alias = "t1t")]
    public float Team1ExplorationTemperature { get; set; }

    [CliOption(
        Description = "ModelName for Team2 ModelBots",
        Alias = "t2m")]
    public string? Team2ModelName { get; set; }

    [CliOption(
        Description = "ExplorationTemperature for Team2 ModelTrainerBots",
        Alias = "t2t")]
    public float Team2ExplorationTemperature { get; set; }

    [CliOption(
        Description = "Allow overwriting existing IDV files",
        Alias = "o")]
    public bool Overwrite { get; set; }

    public async Task<int> RunAsync()
    {
        Foundation.LoggerMessages.LogStartingUp(logger);

        applicationBanner.Display();

        ansiConsole.MarkupLine("[green]Welcome to NemesisEuchre - AI-Powered Euchre Strategy[/]");

        var persistenceOptions = new GamePersistenceOptions(PersistToSql, PersistToIdv, Overwrite);

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

    private static string GetActorDisplay(Actor? actor)
    {
        return actor?.ActorType switch
        {
            ActorType.User => "the player",
            ActorType.Chaos or ActorType.Chad or ActorType.Beta => $"{actor.ActorType}Bots",
            ActorType.Model => $"{actor.ActorType}Bots ({actor.ModelName})",
            ActorType.ModelTrainer => $"{actor.ActorType}Bots ({actor.ModelName} {actor.ExplorationTemperature})",
            _ => $"{ActorType.Chaos}Bots",
        };
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
        var team1Actors = GetTeamActors(Team.Team1);
        var team2Actors = GetTeamActors(Team.Team2);

        ansiConsole.MarkupLine($"[dim]Playing a game between 2 {GetActorDisplay(team1Actors?[0])} and 2 {GetActorDisplay(team2Actors?[0])}...[/]");
        ansiConsole.WriteLine();

        return ansiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Playing game...", async _ => await singleGameRunner.RunAsync(persistenceOptions: persistenceOptions, team1Actors: team1Actors, team2Actors: team2Actors, showDecisions: ShowDecisions));
    }

    private async Task RunBatchGamesAsync(GamePersistenceOptions persistenceOptions)
    {
        var team1Actors = GetTeamActors(Team.Team1);
        var team2Actors = GetTeamActors(Team.Team2);

        ansiConsole.MarkupLine($"[dim]Playing games between 2 {GetActorDisplay(team1Actors?[0])} and 2 {GetActorDisplay(team2Actors?[0])}...[/]");
        ansiConsole.WriteLine();

        var reporter = new LiveBatchProgressReporter();
        var stopwatch = Stopwatch.StartNew();

        var results = await ansiConsole.Live(new Text(string.Empty))
            .AutoClear(true)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {
                var orchestratorTask = batchGameOrchestrator.RunBatchAsync(
                    Count, reporter, persistenceOptions, team1Actors, team2Actors);

                while (!orchestratorTask.IsCompleted)
                {
                    var snapshot = reporter.LatestSnapshot;
                    if (snapshot != null)
                    {
                        ctx.UpdateTarget(
                            gameResultsRenderer.BuildLiveResultsTable(snapshot, Count, stopwatch.Elapsed));
                    }

                    await Task.WhenAny(orchestratorTask, Task.Delay(250));
                }

                return await orchestratorTask;
            });

        gameResultsRenderer.RenderBatchResults(results);
    }
}
