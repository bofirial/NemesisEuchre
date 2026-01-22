using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Extensions;

public static class CardExtensions
{
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
            return 16;
        }
        else if (card.IsLeftBower(trump))
        {
            return 15;
        }
        else if (!card.IsTrump(trump))
        {
            return -1;
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
            Suit.Spades => "♠",
            Suit.Hearts => "♥",
            Suit.Clubs => "♣",
            Suit.Diamonds => "♦",
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
        };
    }
}
