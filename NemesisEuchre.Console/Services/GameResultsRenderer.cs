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
    private readonly Color _team1Color = Color.Green;
    private readonly Color _team2Color = Color.Blue;

    public void RenderResults(Game game)
    {
        ansiConsole.Write(new Rule("[bold]Game Results[/]").RuleStyle("grey"));
        ansiConsole.WriteLine();

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
        table.AddRow("Team 1 Wins", $"{results.Team1Wins} ([{_team1Color}]{results.Team1WinRate:P1}[/])");
        table.AddRow("Team 2 Wins", $"{results.Team2Wins} ([{_team2Color}]{results.Team2WinRate:P1}[/])");
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

    private void RenderStatistics(Game game)
    {
        var winnerColor = game.WinningTeam == Team.Team1 ? _team1Color : _team2Color;

        var scoreTable = new Table()
            .AddColumn($"[{_team1Color}]Team 1[/]", c => c.Centered())
            .AddColumn($"[{_team2Color}]Team 2[/]", c => c.Centered())
            .AddRow(game.Team1Score.ToString(CultureInfo.InvariantCulture), game.Team2Score.ToString(CultureInfo.InvariantCulture))
            .RoundedBorder();

        var panel = new Panel(scoreTable)
            .Header($" [bold {winnerColor}]{game.WinningTeam} wins![/] ", Justify.Center)
            .Border(BoxBorder.Double)
            .BorderColor(winnerColor)
            .Padding(1, 0);

        ansiConsole.Write(Align.Center(panel));
        ansiConsole.WriteLine();
    }

    private void RenderDealsTable(Game game)
    {
        var table = CreateStyledTable("[bold]Deals Summary[/]").Centered();

        /*
         * Deal Number
         * Dealer Position
         * UpCard
         * Trump
         * Calling Player Position
         * Going Alone Indicator
         * DealResult
         * Winning Team
         * Team 1 Score
         * Team 2 Score
         * North Hand
         * East Hand
         * West Hand
         * South Hand
         * */

        /*
         * Player Position
         * Hand
         * UpCard
         * Possible Decisions w/ EstimatedPoints and Chosen Indicator
         */

        /*
         * Card
         * Estimated Points
         * Chosen Card Indicator
         */

        /*
         * Trick Number
         * First Card & Position
         * Second Card & Position
         * Third Card & Position
         * Fourth Card & Position
         * N E S W N E S columns?
         */

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
