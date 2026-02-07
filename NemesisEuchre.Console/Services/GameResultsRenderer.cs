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
}

public class GameResultsRenderer(IAnsiConsole ansiConsole, ICallTrumpDecisionMapper callTrumpDecisionMapper) : IGameResultsRenderer
{
    private static readonly Color Team1Color = Color.Green;
    private static readonly Color Team2Color = Color.Blue;

    private static readonly Color SpadesColor = Color.Yellow;
    private static readonly Color HeartsColor = Color.Red;
    private static readonly Color ClubsColor = Color.Orange1;
    private static readonly Color DiamondsColor = Color.Pink1;

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

    private static string GetDisplayCard(Card card, Suit? trump = null)
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

        var backgroundColor = trump != null && card.IsTrump(trump!.Value) ? " on grey30" : string.Empty;

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

    private static string GetPlayCardDecisionCardDisplay(Card card, PlayCardDecisionRecord playCardDecision, Suit trump)
    {
        var cardDisplay = $"{GetDisplayCard(card, trump)} 0";

        return card == playCardDecision.ChosenCard ? $":diamond_with_a_dot: {cardDisplay} :diamond_with_a_dot:" : cardDisplay;
    }

    private IRenderable[] GetPlayerRow(DealPlayer dealPlayer, Deal deal)
    {
        var playerHand = dealPlayer.StartingHand;

        if (dealPlayer.Position == deal.DealerPosition
            && deal.ChosenDecision is CallTrumpDecision.OrderItUp or CallTrumpDecision.OrderItUpAndGoAlone
            && (deal.UpCard!.Rank != deal.DiscardedCard!.Rank || deal.UpCard!.Suit != deal.DiscardedCard!.Suit))
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
            rows.Add(new Markup("[bold]Call Trump Decisions[/]"));

            var callTrumpDecisionsRound1Table = new Table()
                .ShowRowSeparators()
                .AddColumn("[bold]Player[/]", c => c.Centered())
                .AddColumn("[bold]Actor Type[/]", c => c.Centered())
                .AddColumn("[bold]Hand[/]", c => c.Centered())
                .AddColumn("[bold]Up Card[/]", c => c.Centered())
                .AddColumn($"[bold]{CallTrumpDecision.Pass.Humanize()}[/]", c => c.Centered())
                .AddColumn($"[bold]{CallTrumpDecision.OrderItUp.Humanize(LetterCasing.Title)}[/]", c => c.Centered())
                .AddColumn("[bold]Order It Up (Alone)[/]", c => c.Centered());

            var callTrumpDecisionsRound2Table = new Table()
                .ShowRowSeparators()
                .AddColumn("[bold]Player[/]", c => c.Centered())
                .AddColumn("[bold]Actor Type[/]", c => c.Centered())
                .AddColumn("[bold]Hand[/]", c => c.Centered())
                .AddColumn("[bold]Up Card[/]", c => c.Centered())
                .AddColumn($"[bold]{CallTrumpDecision.Pass.Humanize()}[/]", c => c.Centered());

            if (deal.UpCard!.Suit != Suit.Clubs)
            {
                callTrumpDecisionsRound2Table.AddColumn("[bold]Clubs[/]", c => c.Centered());
                callTrumpDecisionsRound2Table.AddColumn("[bold]Clubs (Alone)[/]", c => c.Centered());
            }

            if (deal.UpCard!.Suit != Suit.Diamonds)
            {
                callTrumpDecisionsRound2Table.AddColumn("[bold]Diamonds[/]", c => c.Centered());
                callTrumpDecisionsRound2Table.AddColumn("[bold]Diamonds (Alone)[/]", c => c.Centered());
            }

            if (deal.UpCard!.Suit != Suit.Hearts)
            {
                callTrumpDecisionsRound2Table.AddColumn("[bold]Hearts[/]", c => c.Centered());
                callTrumpDecisionsRound2Table.AddColumn("[bold]Hearts (Alone)[/]", c => c.Centered());
            }

            if (deal.UpCard!.Suit != Suit.Spades)
            {
                callTrumpDecisionsRound2Table.AddColumn("[bold]Spades[/]", c => c.Centered());
                callTrumpDecisionsRound2Table.AddColumn("[bold]Spades (Alone)[/]", c => c.Centered());
            }

            foreach (var callDecision in deal.CallTrumpDecisions)
            {
                if (callDecision.DecisionOrder < 5)
                {
                    var cells = new List<IRenderable>
                    {
                        new Markup(GetDisplayPlayer(callDecision.PlayerPosition, deal)),
                        new Markup(deal.Players[callDecision.PlayerPosition].ActorType!.Value.Humanize()),
                        new Columns(callDecision.CardsInHand.Select(c => GetDisplayCard(c, deal.Trump!.Value))),
                        new Markup(GetDisplayCard(callDecision.UpCard)),
                        callDecision.ChosenDecision == CallTrumpDecision.Pass ? new Markup(":diamond_with_a_dot: 0 :diamond_with_a_dot:") : new Markup("0"),
                        callDecision.ChosenDecision == CallTrumpDecision.OrderItUp ? new Markup(":diamond_with_a_dot: 0 :diamond_with_a_dot:") : new Markup("0"),
                        callDecision.ChosenDecision == CallTrumpDecision.OrderItUpAndGoAlone ? new Markup(":diamond_with_a_dot: 0 :diamond_with_a_dot:") : new Markup("0"),
                    };

                    callTrumpDecisionsRound1Table.AddRow(cells);
                }
                else
                {
                    var cells = new List<IRenderable>
                    {
                        new Markup(GetDisplayPlayer(callDecision.PlayerPosition, deal)),
                        new Markup(deal.Players[callDecision.PlayerPosition].ActorType!.Value.Humanize()),
                        new Columns(callDecision.CardsInHand.Select(c => GetDisplayCard(c, deal.Trump!.Value))),
                        new Markup(GetDisplayCard(callDecision.UpCard)),
                    };

                    if (callDecision.ValidCallTrumpDecisions.Contains(CallTrumpDecision.Pass))
                    {
                        cells.Add(callDecision.ChosenDecision == CallTrumpDecision.Pass ? new Markup(":diamond_with_a_dot: 0 :diamond_with_a_dot:") : new Markup("0"));
                    }
                    else
                    {
                        cells.Add(new Text(string.Empty));
                    }

                    if (deal.UpCard!.Suit != Suit.Clubs)
                    {
                        cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallClubs ? new Markup(":diamond_with_a_dot: 0 :diamond_with_a_dot:") : new Markup("0"));
                        cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallClubsAndGoAlone ? new Markup(":diamond_with_a_dot: 0 :diamond_with_a_dot:") : new Markup("0"));
                    }

                    if (deal.UpCard!.Suit != Suit.Diamonds)
                    {
                        cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallDiamonds ? new Markup(":diamond_with_a_dot: 0 :diamond_with_a_dot:") : new Markup("0"));
                        cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallDiamondsAndGoAlone ? new Markup(":diamond_with_a_dot: 0 :diamond_with_a_dot:") : new Markup("0"));
                    }

                    if (deal.UpCard!.Suit != Suit.Hearts)
                    {
                        cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallHearts ? new Markup(":diamond_with_a_dot: 0 :diamond_with_a_dot:") : new Markup("0"));
                        cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallHeartsAndGoAlone ? new Markup(":diamond_with_a_dot: 0 :diamond_with_a_dot:") : new Markup("0"));
                    }

                    if (deal.UpCard!.Suit != Suit.Spades)
                    {
                        cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallSpades ? new Markup(":diamond_with_a_dot: 0 :diamond_with_a_dot:") : new Markup("0"));
                        cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallSpadesAndGoAlone ? new Markup(":diamond_with_a_dot: 0 :diamond_with_a_dot:") : new Markup("0"));
                    }

                    callTrumpDecisionsRound2Table.AddRow(cells);
                }
            }

            rows.Add(callTrumpDecisionsRound1Table);
            if (callTrumpDecisionsRound2Table.Rows.Count > 0)
            {
                rows.Add(callTrumpDecisionsRound2Table);
            }

            if (deal.DiscardCardDecisions.Count > 0)
            {
                rows.Add(new Markup("[bold]Discard Decisions[/]"));

                foreach (var discardCardDecision in deal.DiscardCardDecisions)
                {
                    var discardDecisionsTable = new Table()
                        .ShowRowSeparators()
                        .AddColumn("[bold]Player[/]", c => c.Centered())
                        .AddColumn("[bold]Actor Type[/]", c => c.Centered())
                        .AddColumn("[bold]Card[/]", c => c.Centered())
                        .AddColumn("[bold]Estimated Points[/]", c => c.Centered());

                    foreach (var card in discardCardDecision.CardsInHand)
                    {
                        discardDecisionsTable.AddRow(
                            GetDisplayPlayer(discardCardDecision.PlayerPosition, deal),
                            deal.Players[discardCardDecision.PlayerPosition].ActorType!.Value.Humanize(),
                            GetDisplayCard(card, deal.Trump),
                            discardCardDecision.ChosenCard == card ? ":diamond_with_a_dot: 0 :diamond_with_a_dot:" : "0");
                    }

                    rows.Add(discardDecisionsTable);
                }
            }

            rows.Add(new Markup("[bold]Play Card Decisions[/]"));

            var playCardDecisionsTable = new Table()
                .ShowRowSeparators()
                .AddColumn("[bold]#[/]", c => c.Centered())
                .AddColumn("[bold]Player[/]", c => c.Centered())
                .AddColumn("[bold]Actor Type[/]", c => c.Centered())
                .AddColumn("[bold]Lead Suit[/]", c => c.Centered())
                .AddColumn("[bold]Card 1[/]", c => c.Centered().Width(12))
                .AddColumn("[bold]Card 2[/]", c => c.Centered().Width(12))
                .AddColumn("[bold]Card 3[/]", c => c.Centered().Width(12))
                .AddColumn("[bold]Card 4[/]", c => c.Centered().Width(12))
                .AddColumn("[bold]Card 5[/]", c => c.Centered().Width(12));

            foreach (var trick in deal.CompletedTricks)
            {
                foreach (var playCardDecision in trick.PlayCardDecisions)
                {
                    playCardDecisionsTable.AddRow(
                        trick.TrickNumber.ToString(CultureInfo.InvariantCulture),
                        GetDisplayPlayer(playCardDecision.PlayerPosition, deal),
                        deal.Players[playCardDecision.PlayerPosition].ActorType!.Value.Humanize(),
                        playCardDecision.LeadSuit != null ? GetDisplaySuit(playCardDecision.LeadSuit!.Value) : string.Empty,
                        playCardDecision.ValidCardsToPlay.Length > 0 ? GetPlayCardDecisionCardDisplay(playCardDecision.ValidCardsToPlay[0], playCardDecision, deal.Trump!.Value) : string.Empty,
                        playCardDecision.ValidCardsToPlay.Length > 1 ? GetPlayCardDecisionCardDisplay(playCardDecision.ValidCardsToPlay[1], playCardDecision, deal.Trump!.Value) : string.Empty,
                        playCardDecision.ValidCardsToPlay.Length > 2 ? GetPlayCardDecisionCardDisplay(playCardDecision.ValidCardsToPlay[2], playCardDecision, deal.Trump!.Value) : string.Empty,
                        playCardDecision.ValidCardsToPlay.Length > 3 ? GetPlayCardDecisionCardDisplay(playCardDecision.ValidCardsToPlay[3], playCardDecision, deal.Trump!.Value) : string.Empty,
                        playCardDecision.ValidCardsToPlay.Length > 4 ? GetPlayCardDecisionCardDisplay(playCardDecision.ValidCardsToPlay[4], playCardDecision, deal.Trump!.Value) : string.Empty);
                }
            }

            rows.Add(playCardDecisionsTable);
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "Nested Conditionals are bad")]
    private IRenderable[] GetTrickRow(Trick trick, Deal deal)
    {
        var cards = trick.CardsPlayed.ConvertAll(card => GetCardCell(card, trick, deal.Trump!.Value));

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

            cards.Insert(insertIndex, new Text(string.Empty));
        }

        return [new Text(trick.TrickNumber.ToString(CultureInfo.InvariantCulture)), .. GetTrickRowPreSpacers(trick), .. cards, .. GetTrickRowPostSpacers(trick)];
    }
}
