using System.Globalization;

using Humanizer;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

using Spectre.Console;

namespace NemesisEuchre.Console.Services;

public interface ICardDisplayRenderer
{
    string GetDisplayCard(Card card, Suit? trump = null);

    string GetDisplaySuit(Suit suit);

    string GetDisplayPlayer(PlayerPosition position, Deal? deal = null);

    string GetDisplayTeam(Team team);

    string GetPlayCardDecisionCardDisplay(Card card, PlayCardDecisionRecord playCardDecision, Suit trump);
}

public sealed class CardDisplayRenderer : ICardDisplayRenderer
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

    public string GetDisplayTeam(Team team)
    {
        var teamColor = team == Team.Team1 ? Team1Color : Team2Color;

        return $"[{teamColor}]{team.Humanize()}[/]";
    }

    public string GetDisplayPlayer(PlayerPosition position, Deal? deal = null)
    {
        var teamColor = position.GetTeam() == Team.Team1 ? Team1Color : Team2Color;
        var dealerIcon = deal?.DealerPosition == position ? ":flower_playing_cards: " : string.Empty;
        var callerIcon = deal?.CallingPlayer == position ? ":loudspeaker: " : string.Empty;

        return $"[{teamColor}]{dealerIcon}{callerIcon}{position}[/]";
    }

    public string GetDisplayCard(Card card, Suit? trump = null)
    {
        var rankSymbol = RankSymbols.GetValueOrDefault(card.Rank, "?");
        var suitSymbol = SuitSymbols.GetValueOrDefault(card.Suit, "?");
        var suitColor = SuitColors.GetValueOrDefault(card.Suit, Color.Grey);
        var backgroundColor = trump != null && card.IsTrump(trump!.Value) ? " on grey30" : string.Empty;

        return $"[{suitColor}{backgroundColor}]{rankSymbol}{suitSymbol}[/]";
    }

    public string GetDisplaySuit(Suit suit)
    {
        var suitSymbol = SuitSymbols.GetValueOrDefault(suit, "?");
        var suitColor = SuitColors.GetValueOrDefault(suit, Color.Grey);
        return $"[{suitColor}]{suit} {suitSymbol}[/]";
    }

    public string GetPlayCardDecisionCardDisplay(Card card, PlayCardDecisionRecord playCardDecision, Suit trump)
    {
        var decisionPredictedPoints = playCardDecision.DecisionPredictedPoints.First(p => p.Key == card).Value;

        var cardDisplay = $"{GetDisplayCard(card, trump)} {decisionPredictedPoints.ToString("F3", CultureInfo.InvariantCulture)}";

        return card == playCardDecision.ChosenCard ? $":diamond_with_a_dot: {cardDisplay} :diamond_with_a_dot:" : cardDisplay;
    }
}
