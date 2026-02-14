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

public sealed class DecisionRenderer(ICardDisplayRenderer cardDisplayRenderer) : IDecisionRenderer
{
    private static readonly (Suit suit, CallTrumpDecision callDecision, CallTrumpDecision aloneDecision)[] SuitDecisionMap =
    [
        (suit: Suit.Clubs, callDecision: CallTrumpDecision.CallClubs, aloneDecision: CallTrumpDecision.CallClubsAndGoAlone),
        (suit: Suit.Diamonds, callDecision: CallTrumpDecision.CallDiamonds, aloneDecision: CallTrumpDecision.CallDiamondsAndGoAlone),
        (suit: Suit.Hearts, callDecision: CallTrumpDecision.CallHearts, aloneDecision: CallTrumpDecision.CallHeartsAndGoAlone),
        (suit: Suit.Spades, callDecision: CallTrumpDecision.CallSpades, aloneDecision: CallTrumpDecision.CallSpadesAndGoAlone),
    ];

    public List<IRenderable> RenderDecisions(Deal deal)
    {
        var renderables = new List<IRenderable>();

        renderables.AddRange(RenderCallTrumpDecisions(deal));
        renderables.AddRange(RenderDiscardDecisions(deal));
        renderables.AddRange(RenderPlayCardDecisions(deal));

        return renderables;
    }

    private static Markup FormatDecisionScore(float score, bool isChosen)
    {
        var scoreText = score.ToString("F3", CultureInfo.InvariantCulture);
        return isChosen
            ? new Markup($":diamond_with_a_dot: {scoreText} :diamond_with_a_dot:")
            : new Markup(scoreText);
    }

    private List<IRenderable> RenderCallTrumpDecisions(Deal deal)
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

        foreach (var (suitValue, _, _) in SuitDecisionMap)
        {
            if (deal.UpCard!.Suit != suitValue)
            {
                round2Table.AddColumn($"[bold]{suitValue}[/]", c => c.Centered());
                round2Table.AddColumn($"[bold]{suitValue} Alone[/]", c => c.Centered());
            }
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

    private void AddRound1Row(Table table, CallTrumpDecisionRecord callDecision, Deal deal)
    {
        callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.Pass, out float passDecisionPredictedPoints);
        callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.OrderItUp, out float orderItUpDecisionPredictedPoints);
        callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.OrderItUpAndGoAlone, out float orderItUpAndGoAloneDecisionPredictedPoints);

        var cells = new List<IRenderable>
        {
            new Markup(cardDisplayRenderer.GetDisplayPlayer(callDecision.PlayerPosition, deal)),
            new Markup(deal.Players[callDecision.PlayerPosition].Actor.ActorType.Humanize()),
            new Columns(callDecision.CardsInHand.Select(c => cardDisplayRenderer.GetDisplayCard(c, deal.Trump!.Value))),
            new Markup(cardDisplayRenderer.GetDisplayCard(callDecision.UpCard!)),
            FormatDecisionScore(passDecisionPredictedPoints, callDecision.ChosenDecision == CallTrumpDecision.Pass),
            FormatDecisionScore(orderItUpDecisionPredictedPoints, callDecision.ChosenDecision == CallTrumpDecision.OrderItUp),
            FormatDecisionScore(orderItUpAndGoAloneDecisionPredictedPoints, callDecision.ChosenDecision == CallTrumpDecision.OrderItUpAndGoAlone),
        };

        table.AddRow(cells);
    }

    private void AddRound2Row(Table table, CallTrumpDecisionRecord callDecision, Deal deal)
    {
        var cells = new List<IRenderable>
        {
            new Markup(cardDisplayRenderer.GetDisplayPlayer(callDecision.PlayerPosition, deal)),
            new Markup(deal.Players[callDecision.PlayerPosition].Actor.ActorType.Humanize()),
            new Columns(callDecision.CardsInHand.Select(c => cardDisplayRenderer.GetDisplayCard(c, deal.Trump!.Value))),
            new Markup(cardDisplayRenderer.GetDisplayCard(callDecision.UpCard!)),
        };

        if (callDecision.ValidCallTrumpDecisions.Contains(CallTrumpDecision.Pass))
        {
            callDecision.DecisionPredictedPoints.TryGetValue(CallTrumpDecision.Pass, out float passDecisionPredictedPoints);
            cells.Add(FormatDecisionScore(passDecisionPredictedPoints, callDecision.ChosenDecision == CallTrumpDecision.Pass));
        }
        else
        {
            cells.Add(new Text(string.Empty));
        }

        foreach (var (suitValue, callDecisionType, aloneDecisionType) in SuitDecisionMap)
        {
            if (deal.UpCard!.Suit != suitValue)
            {
                callDecision.DecisionPredictedPoints.TryGetValue(callDecisionType, out float suitPoints);
                callDecision.DecisionPredictedPoints.TryGetValue(aloneDecisionType, out float alonePoints);

                cells.Add(FormatDecisionScore(suitPoints, callDecision.ChosenDecision == callDecisionType));
                cells.Add(FormatDecisionScore(alonePoints, callDecision.ChosenDecision == aloneDecisionType));
            }
        }

        table.AddRow(cells);
    }

    private List<IRenderable> RenderDiscardDecisions(Deal deal)
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
                    cardDisplayRenderer.GetDisplayPlayer(discardCardDecision.PlayerPosition, deal),
                    deal.Players[discardCardDecision.PlayerPosition].Actor.ActorType.Humanize(),
                    cardDisplayRenderer.GetDisplayCard(card, deal.Trump),
                    discardCardDecision.ChosenCard == card ? $":diamond_with_a_dot: {decisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)} :diamond_with_a_dot:" : decisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture));
            }

            renderables.Add(discardDecisionsTable);
        }

        return renderables;
    }

    private List<IRenderable> RenderPlayCardDecisions(Deal deal)
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
                playCardDecision.ValidCardsToPlay.Sort((a, b) => playCardDecision.DecisionPredictedPoints.First(p => p.Key == b).Value.CompareTo(playCardDecision.DecisionPredictedPoints.First(p => p.Key == a).Value));

                playCardDecisionsTable.AddRow(
                    trick.TrickNumber.ToString(CultureInfo.InvariantCulture),
                    cardDisplayRenderer.GetDisplayPlayer(playCardDecision.PlayerPosition, deal),
                    deal.Players[playCardDecision.PlayerPosition].Actor.ActorType.Humanize(),
                    playCardDecision.LeadSuit != null ? cardDisplayRenderer.GetDisplaySuit(playCardDecision.LeadSuit!.Value) : string.Empty,
                    playCardDecision.ValidCardsToPlay.Length > 0 ? cardDisplayRenderer.GetPlayCardDecisionCardDisplay(playCardDecision.ValidCardsToPlay[0], playCardDecision, deal.Trump!.Value) : string.Empty,
                    playCardDecision.ValidCardsToPlay.Length > 1 ? cardDisplayRenderer.GetPlayCardDecisionCardDisplay(playCardDecision.ValidCardsToPlay[1], playCardDecision, deal.Trump!.Value) : string.Empty,
                    playCardDecision.ValidCardsToPlay.Length > 2 ? cardDisplayRenderer.GetPlayCardDecisionCardDisplay(playCardDecision.ValidCardsToPlay[2], playCardDecision, deal.Trump!.Value) : string.Empty,
                    playCardDecision.ValidCardsToPlay.Length > 3 ? cardDisplayRenderer.GetPlayCardDecisionCardDisplay(playCardDecision.ValidCardsToPlay[3], playCardDecision, deal.Trump!.Value) : string.Empty,
                    playCardDecision.ValidCardsToPlay.Length > 4 ? cardDisplayRenderer.GetPlayCardDecisionCardDisplay(playCardDecision.ValidCardsToPlay[4], playCardDecision, deal.Trump!.Value) : string.Empty);
            }
        }

        renderables.Add(playCardDecisionsTable);

        return renderables;
    }
}
