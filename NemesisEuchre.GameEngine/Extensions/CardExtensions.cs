using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Extensions;

public static class CardExtensions
{
    /// <summary>
    /// Trump value for the right bower (Jack of trump suit).
    /// Highest trump card in Euchre.
    /// </summary>
    private const int RightBowerValue = 16;

    /// <summary>
    /// Trump value for the left bower (Jack of same color as trump).
    /// Second-highest trump card in Euchre.
    /// </summary>
    private const int LeftBowerValue = 15;

    /// <summary>
    /// Return value indicating a card is not trump.
    /// </summary>
    private const int InvalidTrumpValue = -1;

    public static bool IsRightBower(this Card card, Suit trump)
    {
        return card.Rank == Rank.Jack && card.Suit == trump;
    }

    public static bool IsLeftBower(this Card card, Suit trump)
    {
        return card.Rank == Rank.Jack && card.Suit == trump.GetSameColorSuit() && card.Suit != trump;
    }

    public static Suit GetEffectiveSuit(this Card card, Suit trump)
    {
        return card.IsLeftBower(trump) ? trump : card.Suit;
    }

    public static bool IsTrump(this Card card, Suit trump)
    {
        return card.GetEffectiveSuit(trump) == trump;
    }

    public static int GetTrumpValue(this Card card, Suit trump)
    {
        if (card.IsRightBower(trump))
        {
            return RightBowerValue;
        }

        if (card.IsLeftBower(trump))
        {
            return LeftBowerValue;
        }

        if (!card.IsTrump(trump))
        {
            return InvalidTrumpValue;
        }

        return (int)card.Rank;
    }

    public static string ToDisplayString(this Card card)
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
            Suit.Spades => "♠ ",
            Suit.Hearts => "♥ ",
            Suit.Clubs => "♣ ",
            Suit.Diamonds => "♦ ",
            _ => "?"
        };

        return rankSymbol + suitSymbol;
    }

    public static RelativeCard ToRelative(this Card card, Suit trump)
    {
        var rank = card.Rank;

        if (card.IsRightBower(trump))
        {
            rank = Rank.RightBower;
        }

        if (card.IsLeftBower(trump))
        {
            rank = Rank.LeftBower;
        }

        return new RelativeCard
        {
            Rank = rank,
            Suit = card.Suit.ToRelativeSuit(trump),
            Card = card,
        };
    }
}
