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
    IGameResultsRenderer gameResultsRenderer,
    IBatchResultsExporter batchResultsExporter) : ICliRunAsyncWithReturn
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
        Description = "ModelName for Team1 ModelBots",
        Required = false,
        Alias = "t1m")]
    public string? Team1ModelName { get; set; }

    [CliOption(
        Description = "ExplorationTemperature for Team1 ModelTrainerBots",
        Alias = "t1t")]
    public float Team1ExplorationTemperature { get; set; }

    [CliOption(
        Description = "DecisionType for Team1 Exploration Temperature",
        Required = false,
        Alias = "t1dt")]
    public DecisionType Team1ExplorationDecisionType { get; set; } = DecisionType.All;

    [CliOption(
        Description = "ModelName for Team1 PlayCard decision (overrides --t1m)",
        Required = false,
        Alias = "t1m-play")]
    public string? Team1PlayCardModelName { get; set; }

    [CliOption(
        Description = "ModelName for Team1 CallTrump decision (overrides --t1m)",
        Required = false,
        Alias = "t1m-call")]
    public string? Team1CallTrumpModelName { get; set; }

    [CliOption(
        Description = "ModelName for Team1 DiscardCard decision (overrides --t1m)",
        Required = false,
        Alias = "t1m-discard")]
    public string? Team1DiscardCardModelName { get; set; }

    [CliOption(
        Description = "ActorType for Team2",
        Alias = "t2")]
    public ActorType? Team2 { get; set; }

    [CliOption(
        Description = "ModelName for Team2 ModelBots",
        Required = false,
        Alias = "t2m")]
    public string? Team2ModelName { get; set; }

    [CliOption(
        Description = "ExplorationTemperature for Team2 ModelTrainerBots",
        Alias = "t2t")]
    public float Team2ExplorationTemperature { get; set; }

    [CliOption(
        Description = "DecisionType for Team2 Exploration Temperature",
        Required = false,
        Alias = "t2dt")]
    public DecisionType Team2ExplorationDecisionType { get; set; } = DecisionType.All;

    [CliOption(
        Description = "ModelName for Team2 PlayCard decision (overrides --t2m)",
        Required = false,
        Alias = "t2m-play")]
    public string? Team2PlayCardModelName { get; set; }

    [CliOption(
        Description = "ModelName for Team2 CallTrump decision (overrides --t2m)",
        Required = false,
        Alias = "t2m-call")]
    public string? Team2CallTrumpModelName { get; set; }

    [CliOption(
        Description = "ModelName for Team2 DiscardCard decision (overrides --t2m)",
        Required = false,
        Alias = "t2m-discard")]
    public string? Team2DiscardCardModelName { get; set; }

    [CliOption(
        Description = "Allow overwriting existing IDV files",
        Alias = "o")]
    public bool Overwrite { get; set; }

    [CliOption(
        Description = "Export batch results to JSON file for automation and analysis",
        Required = false,
        Alias = "json")]
    public string? OutputJson { get; set; }

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

    private static Actor? GetTeamActor(
        ActorType? teamActorType,
        string? teamModelName,
        float teamExplorationTemperature,
        DecisionType teamExplorationDecisionType,
        string? teamPlayCardModelName,
        string? teamCallTrumpModelName,
        string? teamDiscardCardModelName)
    {
        bool hasAnyModel = !string.IsNullOrEmpty(teamModelName)
            || !string.IsNullOrEmpty(teamPlayCardModelName)
            || !string.IsNullOrEmpty(teamCallTrumpModelName)
            || !string.IsNullOrEmpty(teamDiscardCardModelName);

        if (teamExplorationTemperature != default)
        {
            teamActorType = ActorType.ModelTrainer;
        }
        else if (hasAnyModel)
        {
            teamActorType = ActorType.Model;
        }

        if (teamActorType == null)
        {
            return null;
        }

        bool hasPerDecisionTypeModel = !string.IsNullOrEmpty(teamPlayCardModelName)
            || !string.IsNullOrEmpty(teamCallTrumpModelName)
            || !string.IsNullOrEmpty(teamDiscardCardModelName);

        if (hasPerDecisionTypeModel)
        {
            return Actor.WithModels(
                teamActorType.Value,
                teamPlayCardModelName,
                teamCallTrumpModelName,
                teamDiscardCardModelName,
                teamModelName,
                teamExplorationTemperature,
                teamExplorationDecisionType);
        }

        if (!string.IsNullOrEmpty(teamModelName))
        {
            return Actor.WithModel(teamActorType.Value, teamModelName, teamExplorationTemperature, teamExplorationDecisionType);
        }

        return new Actor(teamActorType.Value, null, teamExplorationTemperature, teamExplorationDecisionType);
    }

    private static string GetModelDisplay(Actor? actor)
    {
        if (actor?.ModelNames == null || actor.ModelNames.Count == 0)
        {
            return "no model";
        }

        if (actor.ModelNames.Count == 1 && actor.ModelNames.TryGetValue("default", out var singleModel))
        {
            return singleModel;
        }

        var parts = new List<string>();

        if (actor.ModelNames.TryGetValue("PlayCard", out var playModel))
        {
            parts.Add($"play:{playModel}");
        }

        if (actor.ModelNames.TryGetValue("CallTrump", out var callModel))
        {
            parts.Add($"call:{callModel}");
        }

        if (actor.ModelNames.TryGetValue("DiscardCard", out var discardModel))
        {
            parts.Add($"discard:{discardModel}");
        }

        if (actor.ModelNames.TryGetValue("default", out var defaultModel))
        {
            parts.Add($"default:{defaultModel}");
        }

        return string.Join(", ", parts);
    }

    private static string GetActorDisplay(Actor? actor)
    {
        return actor?.ActorType switch
        {
            ActorType.User => "the player",
            ActorType.Chaos or ActorType.Chad or ActorType.Beta => $"{actor.ActorType}Bots",
            ActorType.Model => $"{actor.ActorType}Bots ({GetModelDisplay(actor)})",
            ActorType.ModelTrainer => $"{actor.ActorType}Bots ({GetModelDisplay(actor)} {actor.ExplorationTemperature})",
            _ => $"{ActorType.Chaos}Bots",
        };
    }

    private Actor[]? GetTeamActors(Team team)
    {
        var teamActor = team switch
        {
            Team.Team1 => GetTeamActor(
                Team1,
                Team1ModelName,
                Team1ExplorationTemperature,
                Team1ExplorationDecisionType,
                Team1PlayCardModelName,
                Team1CallTrumpModelName,
                Team1DiscardCardModelName),
            Team.Team2 => GetTeamActor(
                Team2,
                Team2ModelName,
                Team2ExplorationTemperature,
                Team2ExplorationDecisionType,
                Team2PlayCardModelName,
                Team2CallTrumpModelName,
                Team2DiscardCardModelName),
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
                            gameResultsRenderer.BuildLiveResultsTable(snapshot, Count, stopwatch.Elapsed, reporter.StatusMessage));
                    }

                    await Task.WhenAny(orchestratorTask, Task.Delay(250));
                }

                return await orchestratorTask;
            });

        gameResultsRenderer.RenderBatchResults(results);

        if (!string.IsNullOrWhiteSpace(OutputJson))
        {
            try
            {
                batchResultsExporter.ExportToJson(results, OutputJson, team1Actors, team2Actors);
                var exportedPath = Path.GetFullPath(OutputJson);
                ansiConsole.MarkupLine($"[green]✓ Results exported to: {exportedPath}[/]");
            }
            catch (Exception ex)
            {
                Foundation.LoggerMessages.LogResultsExportFailed(logger, OutputJson, ex);
                ansiConsole.MarkupLine($"[yellow]⚠ Warning: Failed to export results - {ex.Message}[/]");
            }
        }
    }
}
