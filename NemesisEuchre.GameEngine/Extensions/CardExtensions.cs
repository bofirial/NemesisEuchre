using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Extensions;

public static class CardExtensions
{
    private const int RightBowerValue = 16;

    private const int LeftBowerValue = 15;

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
            _ => "?",
        };

        var suitSymbol = card.Suit switch
        {
            Suit.Spades => "♠ ",
            Suit.Hearts => "♥ ",
            Suit.Clubs => "♣ ",
            Suit.Diamonds => "♦ ",
            _ => "?",
        };

        return rankSymbol + suitSymbol;
    }

    public static Card ToAbsolute(this RelativeCard relativeCard, Suit trump)
    {
        if (relativeCard.Rank == Rank.RightBower)
        {
            return new Card(trump, Rank.Jack);
        }

        if (relativeCard.Rank == Rank.LeftBower)
        {
            return new Card(trump.GetSameColorSuit(), Rank.Jack);
        }

        var absoluteSuit = relativeCard.Suit.ToAbsoluteSuit(trump);
        return new Card(absoluteSuit, relativeCard.Rank);
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

        return new RelativeCard(rank, card.Suit.ToRelativeSuit(trump, rank))
        {
            Card = card,
        };
    }
}
