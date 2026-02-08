using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.GameEngine.Extensions;

public static class SuitExtensions
{
    public static Suit GetSameColorSuit(this Suit suit)
    {
        return suit switch
        {
            Suit.Spades => Suit.Clubs,
            Suit.Clubs => Suit.Spades,
            Suit.Hearts => Suit.Diamonds,
            Suit.Diamonds => Suit.Hearts,
            _ => throw new ArgumentOutOfRangeException(nameof(suit)),
        };
    }

    public static bool IsRed(this Suit suit)
    {
        return suit is Suit.Hearts or Suit.Diamonds;
    }

    public static bool IsBlack(this Suit suit)
    {
        return suit is Suit.Spades or Suit.Clubs;
    }

    public static RelativeSuit ToRelativeSuit(this Suit suit, Suit trump, Rank? rank = null)
    {
        if (suit == trump)
        {
            return RelativeSuit.Trump;
        }

        var sameColorSuit = trump.GetSameColorSuit();
        if (suit == sameColorSuit)
        {
            if (rank is Rank.Jack or Rank.LeftBower)
            {
                return RelativeSuit.Trump;
            }

            return RelativeSuit.NonTrumpSameColor;
        }

        var oppositeColors = new[] { Suit.Spades, Suit.Hearts, Suit.Clubs, Suit.Diamonds }
            .Where(s => s != trump && s != sameColorSuit)
            .OrderBy(s => (int)s)
            .ToArray();

        return suit == oppositeColors[0]
            ? RelativeSuit.NonTrumpOppositeColor1
            : RelativeSuit.NonTrumpOppositeColor2;
    }
}
