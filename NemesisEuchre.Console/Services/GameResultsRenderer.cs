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
    void RenderResults(Game game);

    void RenderBatchResults(BatchGameResults results);
}

public class GameResultsRenderer(IAnsiConsole ansiConsole, ICallTrumpDecisionMapper callTrumpDecisionMapper) : IGameResultsRenderer
{
    private static readonly Color Team1Color = Color.Green;
    private static readonly Color Team2Color = Color.Blue;

    private static readonly Color SpadesColor = Color.Yellow;
    private static readonly Color HeartsColor = Color.Red;
    private static readonly Color ClubsColor = Color.Orange1;
    private static readonly Color DiamondsColor = Color.Pink1;

    public void RenderResults(Game game)
    {
        ansiConsole.Write(new Rule("[bold]Game Results[/]").RuleStyle("grey"));
        ansiConsole.WriteLine();

        RenderGame(game);
        RenderDeals(game);
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
        table.AddRow("Team 1 Wins", $"{results.Team1Wins} ([{Team1Color}]{results.Team1WinRate:P1}[/])");
        table.AddRow("Team 2 Wins", $"{results.Team2Wins} ([{Team2Color}]{results.Team2WinRate:P1}[/])");
        table.AddRow("Failed Games", results.FailedGames.ToString(CultureInfo.InvariantCulture));
        table.AddRow("Total Deals Played", results.TotalDeals.ToString(CultureInfo.InvariantCulture));
        table.AddRow("Elapsed Time", $"{results.ElapsedTime.TotalSeconds:F2}s");

        ansiConsole.Write(table);
        ansiConsole.WriteLine();
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

    private static string GetDisplayTeam(Team team)
    {
        var teamColor = team == Team.Team1 ? Team1Color : Team2Color;

        return $"[{teamColor}]{team.Humanize()}[/]";
    }

    private static string GetDisplayPlayer(PlayerPosition position, Deal? deal = null)
    {
        var teamColor = position.GetTeam() == Team.Team1 ? Team1Color : Team2Color;
        var dealerIcon = deal?.DealerPosition == position ? ":flower_playing_cards: " : string.Empty;
        var callerIcon = deal?.CallingPlayer == position ? ":loudspeaker: " : string.Empty;

        return $"[{teamColor}]{dealerIcon}{callerIcon}{position}[/]";
    }

    private static string GetDisplayCard(Card card, Suit trump)
    {
        var rankSymbol = card.Rank switch
        {
            Rank.Nine => "9",
            Rank.Ten => "10",
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            Rank.Ace => "A",
            Rank.LeftBower => "J",
            Rank.RightBower => "J",
            _ => "?"
        };

        var suitSymbol = card.Suit switch
        {
            Suit.Spades => ":spade_suit:",
            Suit.Hearts => ":heart_suit: ",
            Suit.Clubs => ":club_suit:",
            Suit.Diamonds => ":diamond_suit:",
            _ => "?"
        };

        var suitColor = card.Suit switch
        {
            Suit.Spades => SpadesColor,
            Suit.Hearts => HeartsColor,
            Suit.Clubs => ClubsColor,
            Suit.Diamonds => DiamondsColor,
            _ => Color.Grey
        };

        var backgroundColor = card.IsTrump(trump) ? " on grey30" : string.Empty;

        return $"[{suitColor}{backgroundColor}]{rankSymbol}{suitSymbol}[/]";
    }

    private static string GetDisplaySuit(Suit suit)
    {
        var suitSymbol = suit switch
        {
            Suit.Spades => ":spade_suit:",
            Suit.Hearts => ":heart_suit: ",
            Suit.Clubs => ":club_suit:",
            Suit.Diamonds => ":diamond_suit:",
            _ => "?"
        };
        var suitColor = suit switch
        {
            Suit.Spades => SpadesColor,
            Suit.Hearts => HeartsColor,
            Suit.Clubs => ClubsColor,
            Suit.Diamonds => DiamondsColor,
            _ => Color.Grey
        };
        return $"[{suitColor}]{suit} {suitSymbol}[/]";
    }

    private static string[] GetTrickRowPreSpacers(Trick trick)
    {
        return trick.CardsPlayed[0].PlayerPosition switch
        {
            PlayerPosition.East => [string.Empty],
            PlayerPosition.South => [string.Empty, string.Empty],
            PlayerPosition.West => [string.Empty, string.Empty, string.Empty],
            PlayerPosition.North => [],
            _ => [],
        };
    }

    private static string[] GetTrickRowPostSpacers(Trick trick)
    {
        string[] trickRowPostSpacers = trick.CardsPlayed[0].PlayerPosition switch
        {
            PlayerPosition.East => [string.Empty, string.Empty],
            PlayerPosition.South => [string.Empty],
            PlayerPosition.West => [],
            PlayerPosition.North => [string.Empty, string.Empty, string.Empty],
            _ => [],
        };

        return [.. trickRowPostSpacers];
    }

    private IRenderable[] GetPlayerRow(DealPlayer dealPlayer, Deal deal)
    {
        var playerHand = dealPlayer.StartingHand;

        if (dealPlayer.Position == deal.DealerPosition && deal.ChosenDecision is CallTrumpDecision.OrderItUp or CallTrumpDecision.OrderItUpAndGoAlone)
        {
            playerHand = [.. playerHand.Where(c => c.Rank != deal.DiscardedCard!.Rank || c.Suit != deal.DiscardedCard!.Suit), deal.UpCard!];
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

    private void RenderDeals(Game game)
    {
        foreach (var deal in game.CompletedDeals)
        {
            RenderDeal(deal);
        }

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
    }

    private void RenderDeal(Deal deal)
    {
        var rows = new Rows(
            new Markup($"[bold]Deal #{deal.DealNumber}[/]"),
            new Table()
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
                    deal.Team2Score.ToString(CultureInfo.InvariantCulture)),
            new Table()
                .ShowRowSeparators()
                .AddColumn("[bold]Player[/]", c => c.Centered())
                .AddColumn("[bold]Hand[/]", c => c.Centered())
                .AddColumn("[bold]Discard[/]", c => c.Centered())
                .AddRow(GetPlayerRow(deal.Players[PlayerPosition.North], deal))
                .AddRow(GetPlayerRow(deal.Players[PlayerPosition.East], deal))
                .AddRow(GetPlayerRow(deal.Players[PlayerPosition.South], deal))
                .AddRow(GetPlayerRow(deal.Players[PlayerPosition.West], deal)),
            RenderTricksTable(deal));

        ansiConsole.Write(Align.Center(rows));
        ansiConsole.WriteLine();
        ansiConsole.WriteLine();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "Nested Conditionals are bad")]
    private string[] GetTrickRow(Trick trick, Deal deal)
    {
        var cards = trick.CardsPlayed.ConvertAll(c => (trick.WinningPosition == c.PlayerPosition ? ":crown: " : string.Empty) + GetDisplayCard(c.Card, deal.Trump!.Value) + (trick.WinningPosition == c.PlayerPosition ? " :crown:" : string.Empty));

        if (callTrumpDecisionMapper.IsGoingAloneDecision(deal.ChosenDecision!.Value))
        {
            var skippedPlayerPosition = deal.CallingPlayer!.Value.GetPartnerPosition();

            int insertIndex;
            if (trick.CardsPlayed[0].PlayerPosition.GetNextPosition() == skippedPlayerPosition)
            {
                insertIndex = 1;
            }
            else if (trick.CardsPlayed[0].PlayerPosition.GetNextPosition().GetNextPosition() == skippedPlayerPosition)
            {
                insertIndex = 2;
            }
            else
            {
                insertIndex = 3;
            }

            cards.Insert(insertIndex, string.Empty);
        }

        return [trick.TrickNumber.ToString(CultureInfo.InvariantCulture), .. GetTrickRowPreSpacers(trick), .. cards, .. GetTrickRowPostSpacers(trick)];
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
}
