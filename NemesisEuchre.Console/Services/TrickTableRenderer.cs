using System.Globalization;

using Humanizer;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Mappers;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace NemesisEuchre.Console.Services;

public interface ITrickTableRenderer
{
    Table RenderTricksTable(Deal deal);

    Table RenderPlayersTable(Deal deal);

    Table RenderDealInformationTable(Deal deal);
}

public sealed class TrickTableRenderer(
    ICardDisplayRenderer cardDisplayRenderer,
    ICallTrumpDecisionMapper callTrumpDecisionMapper) : ITrickTableRenderer
{
    public Table RenderDealInformationTable(Deal deal)
    {
        return new Table()
                .AddColumn("[bold]Dealer[/]", c => c.Centered().Width(10))
                .AddColumn("[bold]Up Card[/]", c => c.Centered())
                .AddColumn("[bold]Caller[/]", c => c.Centered().Width(10))
                .AddColumn("[bold]Decision[/]", c => c.Centered().Width(25))
                .AddColumn("[bold]Trump[/]", c => c.Centered().Width(10))
                .AddColumn("[bold]Deal Result[/]", c => c.Centered().Width(20))
                .AddColumn("[bold]Winning Team[/]", c => c.Centered())
                .AddColumn($"[bold]{cardDisplayRenderer.GetDisplayTeam(Team.Team1)}[/]", c => c.Centered())
                .AddColumn($"[bold]{cardDisplayRenderer.GetDisplayTeam(Team.Team2)}[/]", c => c.Centered())
                .AddRow(
                    cardDisplayRenderer.GetDisplayPlayer(deal.DealerPosition!.Value, deal),
                    cardDisplayRenderer.GetDisplayCard(deal.UpCard!, deal.Trump!.Value),
                    cardDisplayRenderer.GetDisplayPlayer(deal.CallingPlayer!.Value, deal),
                    deal.ChosenDecision?.Humanize(LetterCasing.Title) ?? "N/A",
                    cardDisplayRenderer.GetDisplaySuit(deal.Trump!.Value),
                    deal.DealResult?.Humanize(LetterCasing.Title) ?? "N/A",
                    cardDisplayRenderer.GetDisplayTeam(deal.WinningTeam!.Value),
                    deal.Team1Score.ToString(CultureInfo.InvariantCulture),
                    deal.Team2Score.ToString(CultureInfo.InvariantCulture));
    }

    public Table RenderPlayersTable(Deal deal)
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

    public Table RenderTricksTable(Deal deal)
    {
        return new Table()
                .ShowRowSeparators()
                .AddColumn("[bold]#[/]", c => c.Centered())
                .AddColumn($"[bold]{cardDisplayRenderer.GetDisplayPlayer(PlayerPosition.North, deal)}[/]", c => c.Centered())
                .AddColumn($"[bold]{cardDisplayRenderer.GetDisplayPlayer(PlayerPosition.East, deal)}[/]", c => c.Centered())
                .AddColumn($"[bold]{cardDisplayRenderer.GetDisplayPlayer(PlayerPosition.South, deal)}[/]", c => c.Centered())
                .AddColumn($"[bold]{cardDisplayRenderer.GetDisplayPlayer(PlayerPosition.West, deal)}[/]", c => c.Centered())
                .AddColumn($"[bold]{cardDisplayRenderer.GetDisplayPlayer(PlayerPosition.North, deal)}[/]", c => c.Centered())
                .AddColumn($"[bold]{cardDisplayRenderer.GetDisplayPlayer(PlayerPosition.East, deal)}[/]", c => c.Centered())
                .AddColumn($"[bold]{cardDisplayRenderer.GetDisplayPlayer(PlayerPosition.South, deal)}[/]", c => c.Centered())
                .AddRow(GetTrickRow(deal.CompletedTricks[0], deal))
                .AddRow(GetTrickRow(deal.CompletedTricks[1], deal))
                .AddRow(GetTrickRow(deal.CompletedTricks[2], deal))
                .AddRow(GetTrickRow(deal.CompletedTricks[3], deal))
                .AddRow(GetTrickRow(deal.CompletedTricks[4], deal));
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

    private IRenderable GetCardCell(PlayedCard card, Trick trick, Suit trump)
    {
        var cardMarkup = new Markup(cardDisplayRenderer.GetDisplayCard(card.Card, trump));

        if (trick.WinningPosition == card.PlayerPosition)
        {
            return new Columns(new Markup(":diamond_with_a_dot: "), cardMarkup, new Markup(" :diamond_with_a_dot:"));
        }

        return cardMarkup;
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
            new Markup($"[{dimMarkup}]{cardDisplayRenderer.GetDisplayPlayer(dealPlayer.Position, deal)}[/]"),
            new Columns(playerHand.Select(c => $"[{dimMarkup}]{cardDisplayRenderer.GetDisplayCard(c, deal.Trump!.Value)}[/]")),
            new Markup(dealPlayer.Position == deal.DealerPosition && deal.DiscardedCard != null ? $"[{dimMarkup}]{cardDisplayRenderer.GetDisplayCard(deal.DiscardedCard!, deal.Trump!.Value)}[/]" : string.Empty)
        ];
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
