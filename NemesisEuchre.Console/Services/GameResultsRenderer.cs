using System.Globalization;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

using Spectre.Console;

namespace NemesisEuchre.Console.Services;

public interface IGameResultsRenderer
{
    void RenderResults(Game game);

    void RenderBatchResults(BatchGameResults results);
}

public class GameResultsRenderer(IAnsiConsole ansiConsole) : IGameResultsRenderer
{
    public void RenderResults(Game game)
    {
        ansiConsole.Write(new Rule("[bold]Game Results[/]").RuleStyle("grey"));
        ansiConsole.WriteLine();

        RenderWinner(game);
        RenderStatistics(game);
        RenderDealsTable(game);
    }

    public void RenderBatchResults(BatchGameResults results)
    {
        ansiConsole.WriteLine();
        ansiConsole.MarkupLine("[bold green]Batch Game Results[/]");
        ansiConsole.WriteLine();

        var table = CreateStyledTable()
            .AddColumn(new TableColumn("[bold]Metric[/]").Centered())
            .AddColumn(new TableColumn("[bold]Value[/]").Centered());

        table.AddRow("Total Games", results.TotalGames.ToString(CultureInfo.InvariantCulture));
        table.AddRow("Team 1 Wins", $"{results.Team1Wins} ([green]{results.Team1WinRate:P1}[/])");
        table.AddRow("Team 2 Wins", $"{results.Team2Wins} ([blue]{results.Team2WinRate:P1}[/])");
        table.AddRow("Failed Games", results.FailedGames.ToString(CultureInfo.InvariantCulture));
        table.AddRow("Total Deals Played", results.TotalDeals.ToString(CultureInfo.InvariantCulture));
        table.AddRow("Elapsed Time", $"{results.ElapsedTime.TotalSeconds:F2}s");

        ansiConsole.Write(table);
        ansiConsole.WriteLine();
    }

    private static Card[] GetPlayerHand(Deal deal, PlayerPosition position)
    {
        return deal.Players.GetValueOrDefault(position)?.StartingHand ?? [];
    }

    private static Table CreateStyledTable(string? title = null)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);

        if (title is not null)
        {
            table.Title(title);
        }

        return table;
    }

    private static string FormatHandWithColors(Card[] cards, Suit? trump)
    {
        if (cards.Length == 0)
        {
            return "N/A";
        }

        var sortedCards = cards.SortByTrump(trump);

        var formattedCards = sortedCards.Select(card =>
        {
            var displayString = card.ToDisplayString();
            return card.Suit.IsRed()
                ? $"[red]{displayString}[/]"
                : displayString;
        });

        return string.Join(" ", formattedCards);
    }

    private void RenderWinner(Game game)
    {
        var winnerColor = game.WinningTeam == Team.Team1 ? "green" : "yellow";
        var winnerText = $"[bold {winnerColor}]{game.WinningTeam} wins![/]";

        var panel = new Panel(winnerText)
            .Border(BoxBorder.Double)
            .BorderColor(game.WinningTeam == Team.Team1 ? Color.Green : Color.Yellow)
            .Padding(1, 0);

        ansiConsole.Write(panel);
        ansiConsole.WriteLine();
    }

    private void RenderStatistics(Game game)
    {
        var table = CreateStyledTable();

        table.AddColumn("Team 1 Score");
        table.AddColumn("Team 2 Score");
        table.AddColumn("Deals Played");
        table.AddColumn("Winning Team");

        table.AddRow(
            game.Team1Score.ToString(CultureInfo.InvariantCulture),
            game.Team2Score.ToString(CultureInfo.InvariantCulture),
            game.CompletedDeals.Count.ToString(CultureInfo.InvariantCulture),
            game.WinningTeam?.ToString() ?? "N/A");

        ansiConsole.Write(table);
        ansiConsole.WriteLine();
    }

    private void RenderDealsTable(Game game)
    {
        var table = CreateStyledTable("[bold]Deals Summary[/]");

        table.AddColumn("Deal #");
        table.AddColumn("Trump");
        table.AddColumn("Caller");
        table.AddColumn("Went Alone");
        table.AddColumn("Result");
        table.AddColumn("Team 1 Score");
        table.AddColumn("Team 2 Score");
        table.AddColumn("North Hand");
        table.AddColumn("East Hand");
        table.AddColumn("South Hand");
        table.AddColumn("West Hand");

        for (int i = 0; i < game.CompletedDeals.Count; i++)
        {
            var deal = game.CompletedDeals[i];
            table.AddRow(
                (i + 1).ToString(CultureInfo.InvariantCulture),
                deal.Trump?.ToString() ?? "N/A",
                deal.CallingPlayer?.ToString() ?? "N/A",
                deal.CallingPlayerIsGoingAlone ? "Yes" : "No",
                deal.DealResult?.ToString() ?? "N/A",
                deal.Team1Score.ToString(CultureInfo.InvariantCulture),
                deal.Team2Score.ToString(CultureInfo.InvariantCulture),
                FormatHandWithColors(GetPlayerHand(deal, PlayerPosition.North), deal.Trump),
                FormatHandWithColors(GetPlayerHand(deal, PlayerPosition.East), deal.Trump),
                FormatHandWithColors(GetPlayerHand(deal, PlayerPosition.South), deal.Trump),
                FormatHandWithColors(GetPlayerHand(deal, PlayerPosition.West), deal.Trump));
        }

        ansiConsole.Write(table);
        ansiConsole.WriteLine();
    }
}
