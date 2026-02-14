using System.Globalization;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace NemesisEuchre.Console.Services;

public interface IGameResultsRenderer
{
    void RenderResults(Game game, bool showDecisions);

    void RenderBatchResults(BatchGameResults results);

    IRenderable BuildLiveResultsTable(BatchProgressSnapshot snapshot, int totalGames, TimeSpan elapsed, string? statusMessage = null);
}

public sealed class GameResultsRenderer(
    IAnsiConsole ansiConsole,
    ICardDisplayRenderer cardDisplayRenderer,
    IBatchProgressRenderer batchProgressRenderer,
    ITrickTableRenderer trickTableRenderer,
    IDecisionRenderer decisionRenderer) : IGameResultsRenderer
{
    public void RenderResults(Game game, bool showDecisions)
    {
        ansiConsole.Write(new Rule("[bold]Game Results[/]").RuleStyle("grey"));
        ansiConsole.WriteLine();

        RenderGame(game);
        RenderDeals(game, showDecisions);
    }

    public void RenderBatchResults(BatchGameResults results)
    {
        batchProgressRenderer.RenderBatchResults(results);
    }

    public IRenderable BuildLiveResultsTable(BatchProgressSnapshot snapshot, int totalGames, TimeSpan elapsed, string? statusMessage = null)
    {
        return batchProgressRenderer.BuildLiveResultsTable(snapshot, totalGames, elapsed, statusMessage);
    }

    private void RenderGame(Game game)
    {
        var winnerColor = game.WinningTeam == Team.Team1 ? Color.Green : Color.Blue;

        var scoreTable = new Table()
            .AddColumn(cardDisplayRenderer.GetDisplayTeam(Team.Team1), c => c.Centered())
            .AddColumn(cardDisplayRenderer.GetDisplayTeam(Team.Team2), c => c.Centered())
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

    private void RenderDeals(Game game, bool showDecisions)
    {
        foreach (var deal in game.CompletedDeals)
        {
            RenderDeal(deal, showDecisions);
        }
    }

    private void RenderDeal(Deal deal, bool showDecisions)
    {
        var rows = new List<IRenderable>()
        {
            new Markup($"[bold]Deal #{deal.DealNumber}[/]"),
            trickTableRenderer.RenderDealInformationTable(deal),
            new Markup("[bold]Players[/]"),
            trickTableRenderer.RenderPlayersTable(deal),
            new Markup("[bold]Tricks[/]"),
            trickTableRenderer.RenderTricksTable(deal),
        };

        if (showDecisions)
        {
            rows.AddRange(decisionRenderer.RenderDecisions(deal));
        }

        ansiConsole.Write(Align.Center(new Rows(rows)));
        ansiConsole.WriteLine();
        ansiConsole.WriteLine();
    }
}
