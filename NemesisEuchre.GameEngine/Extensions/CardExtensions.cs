using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Extensions;

public static class CardExtensions
{
    public static bool IsRightBower(this Card card, Suit trump)
    {
        return card.Rank == Rank.Jack && card.Suit == trump;
    }

    public static bool IsLeftBower(this Card card, Suit trump)
    {
        return card.Rank == Rank.Jack && card.Suit == trump.GetSameSuitColor() && card.Suit != trump;
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
}
