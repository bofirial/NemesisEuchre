using System.Globalization;

using Humanizer;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace NemesisEuchre.Console.Services;

public interface IDecisionRenderer
{
    List<IRenderable> RenderDecisions(Deal deal);
}

public class DecisionRenderer : IDecisionRenderer
{
    public List<IRenderable> RenderDecisions(Deal deal)
    {
        var renderables = new List<IRenderable>();

        renderables.AddRange(RenderCallTrumpDecisions(deal));
        renderables.AddRange(RenderDiscardDecisions(deal));
        renderables.AddRange(RenderPlayCardDecisions(deal));

        return renderables;
    }

    private static List<IRenderable> RenderCallTrumpDecisions(Deal deal)
    {
        var renderables = new List<IRenderable>
        {
            new Markup("[bold]Call Trump Decisions[/]"),
        };

        var round1Table = new Table()
            .ShowRowSeparators()
            .AddColumn("[bold]Player[/]", c => c.Centered())
            .AddColumn("[bold]Actor[/]", c => c.Centered())
            .AddColumn("[bold]Hand[/]", c => c.Centered())
            .AddColumn("[bold]Up Card[/]", c => c.Centered())
            .AddColumn($"[bold]{CallTrumpDecision.Pass.Humanize()}[/]", c => c.Centered())
            .AddColumn($"[bold]{CallTrumpDecision.OrderItUp.Humanize(LetterCasing.Title)}[/]", c => c.Centered())
            .AddColumn("[bold]Order It Up Alone[/]", c => c.Centered());

        var round2Table = new Table()
            .ShowRowSeparators()
            .AddColumn("[bold]Player[/]", c => c.Centered())
            .AddColumn("[bold]Actor[/]", c => c.Centered())
            .AddColumn("[bold]Hand[/]", c => c.Centered())
            .AddColumn("[bold]Up Card[/]", c => c.Centered())
            .AddColumn($"[bold]{CallTrumpDecision.Pass.Humanize()}[/]", c => c.Centered());

        if (deal.UpCard!.Suit != Suit.Clubs)
        {
            round2Table.AddColumn("[bold]Clubs[/]", c => c.Centered());
            round2Table.AddColumn("[bold]Clubs Alone[/]", c => c.Centered());
        }

        if (deal.UpCard!.Suit != Suit.Diamonds)
        {
            round2Table.AddColumn("[bold]Diamonds[/]", c => c.Centered());
            round2Table.AddColumn("[bold]Diamonds Alone[/]", c => c.Centered());
        }

        if (deal.UpCard!.Suit != Suit.Hearts)
        {
            round2Table.AddColumn("[bold]Hearts[/]", c => c.Centered());
            round2Table.AddColumn("[bold]Hearts Alone[/]", c => c.Centered());
        }

        if (deal.UpCard!.Suit != Suit.Spades)
        {
            round2Table.AddColumn("[bold]Spades[/]", c => c.Centered());
            round2Table.AddColumn("[bold]Spades Alone[/]", c => c.Centered());
        }

        foreach (var callDecision in deal.CallTrumpDecisions)
        {
            if (callDecision.DecisionOrder < 5)
            {
                AddRound1Row(round1Table, callDecision, deal);
            }
            else
            {
                AddRound2Row(round2Table, callDecision, deal);
            }
        }

        renderables.Add(round1Table);
        if (round2Table.Rows.Count > 0)
        {
            renderables.Add(round2Table);
        }

        return renderables;
    }

    private static void AddRound1Row(Table table, CallTrumpDecisionRecord callDecision, Deal deal)
    {
        callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.Pass, out float passDecisionPredictedPoints);
        callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.OrderItUp, out float orderItUpDecisionPredictedPoints);
        callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.OrderItUpAndGoAlone, out float orderItUpAndGoAloneDecisionPredictedPoints);

        var cells = new List<IRenderable>
        {
            new Markup(GameResultsRenderer.GetDisplayPlayer(callDecision.PlayerPosition, deal)),
            new Markup(deal.Players[callDecision.PlayerPosition].ActorType!.Value.Humanize()),
            new Columns(callDecision.CardsInHand.Select(c => GameResultsRenderer.GetDisplayCard(c, deal.Trump!.Value))),
            new Markup(GameResultsRenderer.GetDisplayCard(callDecision.UpCard!)),
            callDecision.ChosenDecision == CallTrumpDecision.Pass ? new Markup($":diamond_with_a_dot: {passDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)} :diamond_with_a_dot:") : new Markup(passDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)),
            callDecision.ChosenDecision == CallTrumpDecision.OrderItUp ? new Markup($":diamond_with_a_dot: {orderItUpDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)} :diamond_with_a_dot:") : new Markup(orderItUpDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)),
            callDecision.ChosenDecision == CallTrumpDecision.OrderItUpAndGoAlone ? new Markup($":diamond_with_a_dot: {orderItUpAndGoAloneDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)} :diamond_with_a_dot:") : new Markup(orderItUpAndGoAloneDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)),
        };

        table.AddRow(cells);
    }

    private static void AddRound2Row(Table table, CallTrumpDecisionRecord callDecision, Deal deal)
    {
        var cells = new List<IRenderable>
        {
            new Markup(GameResultsRenderer.GetDisplayPlayer(callDecision.PlayerPosition, deal)),
            new Markup(deal.Players[callDecision.PlayerPosition].ActorType!.Value.Humanize()),
            new Columns(callDecision.CardsInHand.Select(c => GameResultsRenderer.GetDisplayCard(c, deal.Trump!.Value))),
            new Markup(GameResultsRenderer.GetDisplayCard(callDecision.UpCard!)),
        };

        if (callDecision.ValidCallTrumpDecisions.Contains(CallTrumpDecision.Pass))
        {
            callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.Pass, out float passDecisionPredictedPoints);

            cells.Add(callDecision.ChosenDecision == CallTrumpDecision.Pass ? new Markup($":diamond_with_a_dot: {passDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)} :diamond_with_a_dot:") : new Markup(passDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)));
        }
        else
        {
            cells.Add(new Text(string.Empty));
        }

        if (deal.UpCard!.Suit != Suit.Clubs)
        {
            callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.CallClubs, out float clubsDecisionPredictedPoints);
            callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.CallClubsAndGoAlone, out float clubsAloneDecisionPredictedPoints);

            cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallClubs ? new Markup($":diamond_with_a_dot: {clubsDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)} :diamond_with_a_dot:") : new Markup(clubsDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)));
            cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallClubsAndGoAlone ? new Markup($":diamond_with_a_dot: {clubsAloneDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)} :diamond_with_a_dot:") : new Markup(clubsAloneDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)));
        }

        if (deal.UpCard!.Suit != Suit.Diamonds)
        {
            callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.CallDiamonds, out float diamondsDecisionPredictedPoints);
            callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.CallDiamondsAndGoAlone, out float diamondsAloneDecisionPredictedPoints);

            cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallDiamonds ? new Markup($":diamond_with_a_dot: {diamondsDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)} :diamond_with_a_dot:") : new Markup(diamondsDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)));
            cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallDiamondsAndGoAlone ? new Markup($":diamond_with_a_dot: {diamondsAloneDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)} :diamond_with_a_dot:") : new Markup(diamondsAloneDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)));
        }

        if (deal.UpCard!.Suit != Suit.Hearts)
        {
            callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.CallHearts, out float heartsDecisionPredictedPoints);
            callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.CallHeartsAndGoAlone, out float heartsAloneDecisionPredictedPoints);

            cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallHearts ? new Markup($":diamond_with_a_dot: {heartsDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)} :diamond_with_a_dot:") : new Markup(heartsDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)));
            cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallHeartsAndGoAlone ? new Markup($":diamond_with_a_dot: {heartsAloneDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)} :diamond_with_a_dot:") : new Markup(heartsAloneDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)));
        }

        if (deal.UpCard!.Suit != Suit.Spades)
        {
            callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.CallSpades, out float spadesDecisionPredictedPoints);
            callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.CallSpadesAndGoAlone, out float spadesAloneDecisionPredictedPoints);

            cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallSpades ? new Markup($":diamond_with_a_dot: {spadesDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)} :diamond_with_a_dot:") : new Markup(spadesDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)));
            cells.Add(callDecision.ChosenDecision == CallTrumpDecision.CallSpadesAndGoAlone ? new Markup($":diamond_with_a_dot: {spadesAloneDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)} :diamond_with_a_dot:") : new Markup(spadesAloneDecisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)));
        }

        table.AddRow(cells);
    }

    private static List<IRenderable> RenderDiscardDecisions(Deal deal)
    {
        var renderables = new List<IRenderable>();

        if (deal.DiscardCardDecisions.Count == 0)
        {
            return renderables;
        }

        renderables.Add(new Markup("[bold]Discard Decisions[/]"));

        foreach (var discardCardDecision in deal.DiscardCardDecisions)
        {
            var discardDecisionsTable = new Table()
                .ShowRowSeparators()
                .AddColumn("[bold]Player[/]", c => c.Centered())
                .AddColumn("[bold]Actor[/]", c => c.Centered())
                .AddColumn("[bold]Card[/]", c => c.Centered())
                .AddColumn("[bold]Estimated Points[/]", c => c.Centered());

            foreach (var card in discardCardDecision.CardsInHand)
            {
                var decisionPredictedPoints = discardCardDecision.DecisionPredictedPoints.First(p => p.Key == card).Value;

                discardDecisionsTable.AddRow(
                    GameResultsRenderer.GetDisplayPlayer(discardCardDecision.PlayerPosition, deal),
                    deal.Players[discardCardDecision.PlayerPosition].ActorType!.Value.Humanize(),
                    GameResultsRenderer.GetDisplayCard(card, deal.Trump),
                    discardCardDecision.ChosenCard == card ? $":diamond_with_a_dot: {decisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)} :diamond_with_a_dot:" : decisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture));
            }

            renderables.Add(discardDecisionsTable);
        }

        return renderables;
    }

    private static List<IRenderable> RenderPlayCardDecisions(Deal deal)
    {
        var renderables = new List<IRenderable>
        {
            new Markup("[bold]Play Card Decisions[/]"),
        };

        var playCardDecisionsTable = new Table()
            .ShowRowSeparators()
            .AddColumn("[bold]#[/]", c => c.Centered())
            .AddColumn("[bold]Player[/]", c => c.Centered())
            .AddColumn("[bold]Actor[/]", c => c.Centered())
            .AddColumn("[bold]Lead Suit[/]", c => c.Centered())
            .AddColumn("[bold]Card 1[/]", c => c.Centered().Width(18))
            .AddColumn("[bold]Card 2[/]", c => c.Centered().Width(18))
            .AddColumn("[bold]Card 3[/]", c => c.Centered().Width(18))
            .AddColumn("[bold]Card 4[/]", c => c.Centered().Width(18))
            .AddColumn("[bold]Card 5[/]", c => c.Centered().Width(18));

        foreach (var trick in deal.CompletedTricks)
        {
            foreach (var playCardDecision in trick.PlayCardDecisions)
            {
                playCardDecisionsTable.AddRow(
                    trick.TrickNumber.ToString(CultureInfo.InvariantCulture),
                    GameResultsRenderer.GetDisplayPlayer(playCardDecision.PlayerPosition, deal),
                    deal.Players[playCardDecision.PlayerPosition].ActorType!.Value.Humanize(),
                    playCardDecision.LeadSuit != null ? GameResultsRenderer.GetDisplaySuit(playCardDecision.LeadSuit!.Value) : string.Empty,
                    playCardDecision.ValidCardsToPlay.Length > 0 ? GameResultsRenderer.GetPlayCardDecisionCardDisplay(playCardDecision.ValidCardsToPlay[0], playCardDecision, deal.Trump!.Value) : string.Empty,
                    playCardDecision.ValidCardsToPlay.Length > 1 ? GameResultsRenderer.GetPlayCardDecisionCardDisplay(playCardDecision.ValidCardsToPlay[1], playCardDecision, deal.Trump!.Value) : string.Empty,
                    playCardDecision.ValidCardsToPlay.Length > 2 ? GameResultsRenderer.GetPlayCardDecisionCardDisplay(playCardDecision.ValidCardsToPlay[2], playCardDecision, deal.Trump!.Value) : string.Empty,
                    playCardDecision.ValidCardsToPlay.Length > 3 ? GameResultsRenderer.GetPlayCardDecisionCardDisplay(playCardDecision.ValidCardsToPlay[3], playCardDecision, deal.Trump!.Value) : string.Empty,
                    playCardDecision.ValidCardsToPlay.Length > 4 ? GameResultsRenderer.GetPlayCardDecisionCardDisplay(playCardDecision.ValidCardsToPlay[4], playCardDecision, deal.Trump!.Value) : string.Empty);
            }
        }

        renderables.Add(playCardDecisionsTable);

        return renderables;
    }
}
