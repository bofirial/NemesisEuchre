using System.Globalization;

using Humanizer;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Mappers;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace NemesisEuchre.Console.Services;

public interface IGameResultsRenderer
{
    void RenderResults(Game game, bool showDecisions);

    void RenderBatchResults(BatchGameResults results);

    IRenderable BuildLiveResultsTable(BatchProgressSnapshot snapshot, int totalGames, TimeSpan elapsed, string? statusMessage = null);
}

public class GameResultsRenderer(IAnsiConsole ansiConsole, ICallTrumpDecisionMapper callTrumpDecisionMapper, IDecisionRenderer decisionRenderer) : IGameResultsRenderer
{
    private static readonly Color Team1Color = Color.Green;
    private static readonly Color Team2Color = Color.Blue;

    private static readonly Dictionary<Suit, Color> SuitColors = new()
    {
        [Suit.Spades] = Color.Yellow,
        [Suit.Hearts] = Color.Red,
        [Suit.Clubs] = Color.Orange1,
        [Suit.Diamonds] = Color.Pink1,
    };

    private static readonly Dictionary<Suit, string> SuitSymbols = new()
    {
        [Suit.Spades] = ":spade_suit:",
        [Suit.Hearts] = ":heart_suit: ",
        [Suit.Clubs] = ":club_suit:",
        [Suit.Diamonds] = ":diamond_suit:",
    };

    private static readonly Dictionary<Rank, string> RankSymbols = new()
    {
        [Rank.Nine] = "9",
        [Rank.Ten] = "10",
        [Rank.Jack] = "J",
        [Rank.Queen] = "Q",
        [Rank.King] = "K",
        [Rank.Ace] = "A",
        [Rank.LeftBower] = "J",
        [Rank.RightBower] = "J",
    };

    public void RenderResults(Game game, bool showDecisions)
    {
        ansiConsole.Write(new Rule("[bold]Game Results[/]").RuleStyle("grey"));
        ansiConsole.WriteLine();

        RenderGame(game);
        RenderDeals(game, showDecisions);
    }

    public void RenderBatchResults(BatchGameResults results)
    {
        ansiConsole.WriteLine();
        ansiConsole.MarkupLine("[bold green]Batch Game Results[/]");
        ansiConsole.WriteLine();

        var table = CreateStyledTable()
            .AddColumn(new TableColumn("[bold]Metric[/]").Centered())
            .AddColumn(new TableColumn("[bold]Value[/]").Centered());

        table.AddRow("Total Games", $"{results.TotalGames:N0}");
        table.AddRow("Team 1 Wins", $"{results.Team1Wins:N0} ([{Team1Color}]{results.Team1WinRate:P1}[/])");
        table.AddRow("Team 2 Wins", $"{results.Team2Wins:N0} ([{Team2Color}]{results.Team2WinRate:P1}[/])");
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

    public IRenderable BuildLiveResultsTable(BatchProgressSnapshot snapshot, int totalGames, TimeSpan elapsed, string? statusMessage = null)
    {
        var table = CreateStyledTable()
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
        table.AddRow("Team 1 Wins", $"{snapshot.Team1Wins:N0} ([{Team1Color}]{team1Rate:P1}[/])");
        table.AddRow("Team 2 Wins", $"{snapshot.Team2Wins:N0} ([{Team2Color}]{team2Rate:P1}[/])");
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

    internal static string GetDisplayTeam(Team team)
    {
        var teamColor = team == Team.Team1 ? Team1Color : Team2Color;

        return $"[{teamColor}]{team.Humanize()}[/]";
    }

    internal static string GetDisplayPlayer(PlayerPosition position, Deal? deal = null)
    {
        var teamColor = position.GetTeam() == Team.Team1 ? Team1Color : Team2Color;
        var dealerIcon = deal?.DealerPosition == position ? ":flower_playing_cards: " : string.Empty;
        var callerIcon = deal?.CallingPlayer == position ? ":loudspeaker: " : string.Empty;

        return $"[{teamColor}]{dealerIcon}{callerIcon}{position}[/]";
    }

    internal static string GetDisplayCard(Card card, Suit? trump = null)
    {
        var rankSymbol = RankSymbols.GetValueOrDefault(card.Rank, "?");
        var suitSymbol = SuitSymbols.GetValueOrDefault(card.Suit, "?");
        var suitColor = SuitColors.GetValueOrDefault(card.Suit, Color.Grey);
        var backgroundColor = trump != null && card.IsTrump(trump!.Value) ? " on grey30" : string.Empty;

        return $"[{suitColor}{backgroundColor}]{rankSymbol}{suitSymbol}[/]";
    }

    internal static string GetDisplaySuit(Suit suit)
    {
        var suitSymbol = SuitSymbols.GetValueOrDefault(suit, "?");
        var suitColor = SuitColors.GetValueOrDefault(suit, Color.Grey);
        return $"[{suitColor}]{suit} {suitSymbol}[/]";
    }

    internal static string GetPlayCardDecisionCardDisplay(Card card, PlayCardDecisionRecord playCardDecision, Suit trump)
    {
        var decisionPredictedPoints = playCardDecision.DecisionPredictedPoints.First(p => p.Key == card).Value;

        var cardDisplay = $"{GetDisplayCard(card, trump)} {decisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)}";

        return card == playCardDecision.ChosenCard ? $":diamond_with_a_dot: {cardDisplay} :diamond_with_a_dot:" : cardDisplay;
    }

    private static Table CreateStyledTable(string? title = null)
    {
        return RenderingUtilities.CreateStyledTable(title);
    }

    private static IRenderable[] GetTrickRowPreSpacers(Trick trick)
    {
        return trick.CardsPlayed[0].PlayerPosition switch
        {
            PlayerPosition.East => [new Text(string.Empty)],
            PlayerPosition.South => [new Text(string.Empty), new Text(string.Empty)],
            PlayerPosition.West => [new Text(string.Empty), new Text(string.Empty), new Text(string.Empty)],
            PlayerPosition.North => [],
            _ => [],
        };
    }

    private static IRenderable[] GetTrickRowPostSpacers(Trick trick)
    {
        return trick.CardsPlayed[0].PlayerPosition switch
        {
            PlayerPosition.East => [new Text(string.Empty), new Text(string.Empty)],
            PlayerPosition.South => [new Text(string.Empty)],
            PlayerPosition.West => [],
            PlayerPosition.North => [new Text(string.Empty), new Text(string.Empty), new Text(string.Empty)],
            _ => [],
        };
    }

    private static IRenderable GetCardCell(PlayedCard card, Trick trick, Suit trump)
    {
        var cardMarkup = new Markup(GetDisplayCard(card.Card, trump));

        if (trick.WinningPosition == card.PlayerPosition)
        {
            return new Columns(new Markup(":diamond_with_a_dot: "), cardMarkup, new Markup(" :diamond_with_a_dot:"));
        }

        return cardMarkup;
    }

    private static Table RenderDealInformationTable(Deal deal)
    {
        return new Table()
                .AddColumn("[bold]Dealer[/]", c => c.Centered().Width(10))
                .AddColumn("[bold]Up Card[/]", c => c.Centered())
                .AddColumn("[bold]Caller[/]", c => c.Centered().Width(10))
                .AddColumn("[bold]Decision[/]", c => c.Centered().Width(25))
                .AddColumn("[bold]Trump[/]", c => c.Centered().Width(10))
                .AddColumn("[bold]Deal Result[/]", c => c.Centered().Width(20))
                .AddColumn("[bold]Winning Team[/]", c => c.Centered())
                .AddColumn($"[bold]{GetDisplayTeam(Team.Team1)}[/]", c => c.Centered())
                .AddColumn($"[bold]{GetDisplayTeam(Team.Team2)}[/]", c => c.Centered())
                .AddRow(
                    GetDisplayPlayer(deal.DealerPosition!.Value, deal),
                    GetDisplayCard(deal.UpCard!, deal.Trump!.Value),
                    GetDisplayPlayer(deal.CallingPlayer!.Value, deal),
                    deal.ChosenDecision?.Humanize(LetterCasing.Title) ?? "N/A",
                    GetDisplaySuit(deal.Trump!.Value),
                    deal.DealResult?.Humanize(LetterCasing.Title) ?? "N/A",
                    GetDisplayTeam(deal.WinningTeam!.Value),
                    deal.Team1Score.ToString(CultureInfo.InvariantCulture),
                    deal.Team2Score.ToString(CultureInfo.InvariantCulture));
    }

    private static int GetSkippedPlayerInsertIndex(Trick trick, PlayerPosition skippedPlayerPosition)
    {
        var leadPosition = trick.CardsPlayed[0].PlayerPosition;

        if (leadPosition.GetNextPosition() == skippedPlayerPosition)
        {
            return 1;
        }

        if (leadPosition.GetNextPosition().GetNextPosition() == skippedPlayerPosition)
        {
            return 2;
        }

        return 3;
    }

    private IRenderable[] GetPlayerRow(DealPlayer dealPlayer, Deal deal)
    {
        var playerHand = dealPlayer.StartingHand;

        if (dealPlayer.Position == deal.DealerPosition
            && deal.ChosenDecision is CallTrumpDecision.OrderItUp or CallTrumpDecision.OrderItUpAndGoAlone
            && deal.UpCard != deal.DiscardedCard)
        {
            playerHand = [.. playerHand.Where(c => c != deal.DiscardedCard), deal.UpCard!];
        }

        playerHand = playerHand.SortByTrump(deal.Trump);

        var dimMarkup = string.Empty;

        if (callTrumpDecisionMapper.IsGoingAloneDecision(deal.ChosenDecision!.Value) && deal.CallingPlayer!.Value.GetPartnerPosition() == dealPlayer.Position)
        {
            dimMarkup = "dim";
        }

        return [
            new Markup($"[{dimMarkup}]{GetDisplayPlayer(dealPlayer.Position, deal)}[/]"),
            new Columns(playerHand.Select(c => $"[{dimMarkup}]{GetDisplayCard(c, deal.Trump!.Value)}[/]")),
            new Markup(dealPlayer.Position == deal.DealerPosition && deal.DiscardedCard != null ? $"[{dimMarkup}]{GetDisplayCard(deal.DiscardedCard!, deal.Trump!.Value)}[/]" : string.Empty)
        ];
    }

    private void RenderGame(Game game)
    {
        var winnerColor = game.WinningTeam == Team.Team1 ? Team1Color : Team2Color;

        var scoreTable = new Table()
            .AddColumn(GetDisplayTeam(Team.Team1), c => c.Centered())
            .AddColumn(GetDisplayTeam(Team.Team2), c => c.Centered())
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
            RenderDealInformationTable(deal),
            new Markup("[bold]Players[/]"),
            RenderPlayersTable(deal),
            new Markup("[bold]Tricks[/]"),
            RenderTricksTable(deal),
        };

        if (showDecisions)
        {
            rows.AddRange(decisionRenderer.RenderDecisions(deal));
        }

        ansiConsole.Write(Align.Center(new Rows(rows)));
        ansiConsole.WriteLine();
        ansiConsole.WriteLine();
    }

    private Table RenderPlayersTable(Deal deal)
    {
        return new Table()
                .ShowRowSeparators()
                .AddColumn("[bold]Player[/]", c => c.Centered())
                .AddColumn("[bold]Hand[/]", c => c.Centered())
                .AddColumn("[bold]Discard[/]", c => c.Centered())
                .AddRow(GetPlayerRow(deal.Players[PlayerPosition.North], deal))
                .AddRow(GetPlayerRow(deal.Players[PlayerPosition.East], deal))
                .AddRow(GetPlayerRow(deal.Players[PlayerPosition.South], deal))
                .AddRow(GetPlayerRow(deal.Players[PlayerPosition.West], deal));
    }

    private Table RenderTricksTable(Deal deal)
    {
        return new Table()
                .ShowRowSeparators()
                .AddColumn("[bold]#[/]", c => c.Centered())
                .AddColumn($"[bold]{GetDisplayPlayer(PlayerPosition.North, deal)}[/]", c => c.Centered())
                .AddColumn($"[bold]{GetDisplayPlayer(PlayerPosition.East, deal)}[/]", c => c.Centered())
                .AddColumn($"[bold]{GetDisplayPlayer(PlayerPosition.South, deal)}[/]", c => c.Centered())
                .AddColumn($"[bold]{GetDisplayPlayer(PlayerPosition.West, deal)}[/]", c => c.Centered())
                .AddColumn($"[bold]{GetDisplayPlayer(PlayerPosition.North, deal)}[/]", c => c.Centered())
                .AddColumn($"[bold]{GetDisplayPlayer(PlayerPosition.East, deal)}[/]", c => c.Centered())
                .AddColumn($"[bold]{GetDisplayPlayer(PlayerPosition.South, deal)}[/]", c => c.Centered())
                .AddRow(GetTrickRow(deal.CompletedTricks[0], deal))
                .AddRow(GetTrickRow(deal.CompletedTricks[1], deal))
                .AddRow(GetTrickRow(deal.CompletedTricks[2], deal))
                .AddRow(GetTrickRow(deal.CompletedTricks[3], deal))
                .AddRow(GetTrickRow(deal.CompletedTricks[4], deal));
    }

    private IRenderable[] GetTrickRow(Trick trick, Deal deal)
    {
        var cards = trick.CardsPlayed.ConvertAll(card => GetCardCell(card, trick, deal.Trump!.Value));

        if (callTrumpDecisionMapper.IsGoingAloneDecision(deal.ChosenDecision!.Value))
        {
            var skippedPlayerPosition = deal.CallingPlayer!.Value.GetPartnerPosition();
            var insertIndex = GetSkippedPlayerInsertIndex(trick, skippedPlayerPosition);
            cards.Insert(insertIndex, new Text(string.Empty));
        }

        return [new Text(trick.TrickNumber.ToString(CultureInfo.InvariantCulture)), .. GetTrickRowPreSpacers(trick), .. cards, .. GetTrickRowPostSpacers(trick)];
    }
}
