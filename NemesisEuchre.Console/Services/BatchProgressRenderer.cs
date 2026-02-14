using Humanizer;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Foundation.Constants;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace NemesisEuchre.Console.Services;

public interface IBatchProgressRenderer
{
    void RenderBatchResults(BatchGameResults results);

    IRenderable BuildLiveResultsTable(
        BatchProgressSnapshot snapshot,
        int totalGames,
        TimeSpan elapsed,
        string? statusMessage = null);
}

public sealed class BatchProgressRenderer(IAnsiConsole ansiConsole, ICardDisplayRenderer cardDisplayRenderer) : IBatchProgressRenderer
{
    public void RenderBatchResults(BatchGameResults results)
    {
        ansiConsole.WriteLine();
        ansiConsole.MarkupLine("[bold green]Batch Game Results[/]");
        ansiConsole.WriteLine();

        var table = RenderingUtilities.CreateStyledTable()
            .AddColumn(new TableColumn("[bold]Metric[/]").Centered())
            .AddColumn(new TableColumn("[bold]Value[/]").Centered());

        table.AddRow("Total Games", $"{results.TotalGames:N0}");
        table.AddRow("Team 1 Wins", $"{results.Team1Wins:N0} ({cardDisplayRenderer.GetDisplayTeam(Team.Team1)} {results.Team1WinRate:P1})");
        table.AddRow("Team 2 Wins", $"{results.Team2Wins:N0} ({cardDisplayRenderer.GetDisplayTeam(Team.Team2)} {results.Team2WinRate:P1})");
        table.AddRow("Failed Games", $"{results.FailedGames:N0}");
        table.AddRow("Total Deals", $"{results.TotalDeals:N0}");
        table.AddRow("Total Tricks", $"{results.TotalTricks:N0}");
        table.AddRow("Call Trump Decisions", $"{results.TotalCallTrumpDecisions:N0}");
        table.AddRow("Discard Card Decisions", $"{results.TotalDiscardCardDecisions:N0}");
        table.AddRow("Play Card Decisions", $"{results.TotalPlayCardDecisions:N0}");
        table.AddRow("Elapsed Time", results.ElapsedTime.Humanize(2, countEmptyUnits: true, minUnit: TimeUnit.Second));

        if (results.PlayingDuration.HasValue)
        {
            table.AddRow("[dim]  Playing[/]", $"[dim]{results.PlayingDuration?.Humanize(2, countEmptyUnits: true, minUnit: TimeUnit.Second)}[/]");
        }

        if (results.PersistenceDuration.HasValue)
        {
            table.AddRow("[dim]  Persistence[/]", $"[dim]{results.PersistenceDuration?.Humanize(2, countEmptyUnits: true, minUnit: TimeUnit.Second)}[/]");
        }

        if (results.IdvSaveDuration.HasValue)
        {
            table.AddRow("[dim]  IDV Save[/]", $"[dim]{results.IdvSaveDuration?.Humanize(2, countEmptyUnits: true, minUnit: TimeUnit.Second)}[/]");
        }

        if (results.ElapsedTime.TotalSeconds > 0)
        {
            var throughput = results.TotalGames / results.ElapsedTime.TotalSeconds;
            table.AddRow("Throughput", $"{throughput:F0} games/sec");
        }

        ansiConsole.Write(table);
        ansiConsole.WriteLine();
    }

    public IRenderable BuildLiveResultsTable(
        BatchProgressSnapshot snapshot,
        int totalGames,
        TimeSpan elapsed,
        string? statusMessage = null)
    {
        var table = RenderingUtilities.CreateStyledTable()
            .AddColumn(new TableColumn("[bold]Metric[/]").Centered())
            .AddColumn(new TableColumn("[bold]Value[/]").Centered());

        var percentage = totalGames > 0 ? (double)snapshot.CompletedGames / totalGames * 100 : 0;
        table.AddRow("Progress", $"{snapshot.CompletedGames:N0} / {totalGames:N0} ({percentage:F1}%)");

        if (statusMessage != null)
        {
            table.AddRow("[yellow]Status[/]", $"[yellow]{statusMessage}[/]");
        }

        var completedNonFailed = snapshot.Team1Wins + snapshot.Team2Wins;
        var team1Rate = completedNonFailed > 0 ? (double)snapshot.Team1Wins / completedNonFailed : 0;
        var team2Rate = completedNonFailed > 0 ? (double)snapshot.Team2Wins / completedNonFailed : 0;
        table.AddRow("Team 1 Wins", $"{snapshot.Team1Wins:N0} ({cardDisplayRenderer.GetDisplayTeam(Team.Team1)} {team1Rate:P1})");
        table.AddRow("Team 2 Wins", $"{snapshot.Team2Wins:N0} ({cardDisplayRenderer.GetDisplayTeam(Team.Team2)} {team2Rate:P1})");
        table.AddRow("Failed Games", $"{snapshot.FailedGames:N0}");
        table.AddRow("Total Deals", $"{snapshot.TotalDeals:N0}");
        table.AddRow("Total Tricks", $"{snapshot.TotalTricks:N0}");
        table.AddRow("Call Trump Decisions", $"{snapshot.TotalCallTrumpDecisions:N0}");
        table.AddRow("Discard Card Decisions", $"{snapshot.TotalDiscardCardDecisions:N0}");
        table.AddRow("Play Card Decisions", $"{snapshot.TotalPlayCardDecisions:N0}");
        table.AddRow("Elapsed Time", elapsed.Humanize(2, countEmptyUnits: true, minUnit: TimeUnit.Second));

        if (snapshot.CompletedGames > 0 && elapsed.TotalSeconds > 0)
        {
            var throughput = snapshot.CompletedGames / elapsed.TotalSeconds;
            table.AddRow("Throughput", $"{throughput:F0} games/sec");

            var remaining = totalGames - snapshot.CompletedGames;
            if (remaining > 0)
            {
                var estimatedRemaining = TimeSpan.FromSeconds(remaining / throughput);
                table.AddRow("Estimated Remaining", estimatedRemaining.Humanize(2, countEmptyUnits: true, minUnit: TimeUnit.Second));
            }
        }

        return new Rows(new Markup("[bold yellow]Batch Game Results (Live)[/]"), new Text(string.Empty), table);
    }
}
