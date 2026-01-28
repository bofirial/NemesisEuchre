using DotMake.CommandLine;

using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation;
using NemesisEuchre.GameEngine;

using Spectre.Console;

namespace NemesisEuchre.Console.Commands;

[CliCommand(Description = "Nemesis Euchre")]
public class DefaultCommand(
    ILogger<DefaultCommand> logger,
    IAnsiConsole ansiConsole,
    IApplicationBanner applicationBanner,
    IGameOrchestrator gameOrchestrator,
    IBatchGameOrchestrator batchGameOrchestrator,
    IGameRepository gameRepository,
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

    private async Task RunSingleGameAsync()
    {
        ansiConsole.MarkupLine("[dim]Playing a game between 4 ChaosBots...[/]");
        ansiConsole.WriteLine();

        var game = await ansiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Playing game...", async _ => await gameOrchestrator.OrchestrateGameAsync());

        try
        {
            await gameRepository.SaveCompletedGameAsync(game);
        }
        catch (Exception ex)
        {
            LoggerMessages.LogGamePersistenceFailed(logger, ex);
        }

        gameResultsRenderer.RenderResults(game);
    }

    private async Task RunBatchGamesAsync()
    {
        ansiConsole.MarkupLine($"[dim]Playing {Count} games between 4 ChaosBots...[/]");
        ansiConsole.WriteLine();

        var results = await ansiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask($"[green]Playing {Count} games...[/]", maxValue: Count);

                var progress = new Progress<int>(completed => task.Value = completed);

                return await batchGameOrchestrator.RunBatchAsync(Count, progress: progress);
            });

        RenderBatchResults(results);
    }

    private void RenderBatchResults(BatchGameResults results)
    {
        ansiConsole.WriteLine();
        ansiConsole.MarkupLine("[bold green]Batch Game Results[/]");
        ansiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[bold]Metric[/]").Centered())
            .AddColumn(new TableColumn("[bold]Value[/]").Centered());

        table.AddRow("Total Games", results.TotalGames.ToString(System.Globalization.CultureInfo.InvariantCulture));
        table.AddRow("Team 1 Wins", $"{results.Team1Wins} ([green]{results.Team1WinRate:P1}[/])");
        table.AddRow("Team 2 Wins", $"{results.Team2Wins} ([green]{results.Team2WinRate:P1}[/])");
        table.AddRow("Failed Games", results.FailedGames.ToString(System.Globalization.CultureInfo.InvariantCulture));
        table.AddRow("Total Deals Played", results.TotalDeals.ToString(System.Globalization.CultureInfo.InvariantCulture));
        table.AddRow("Elapsed Time", $"{results.ElapsedTime.TotalSeconds:F2}s");

        ansiConsole.Write(table);
        ansiConsole.WriteLine();
    }
}
