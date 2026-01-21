using NemesisEuchre.GameEngine.Constants;

namespace NemesisEuchre.GameEngine.Extensions;

public static class SuitExtensions
{
    public static Suit GetSameSuitColor(this Suit suit)
    {
        return suit switch
        {
            Suit.Spades => Suit.Clubs,
            Suit.Clubs => Suit.Spades,
            Suit.Hearts => Suit.Diamonds,
            Suit.Diamonds => Suit.Hearts,
            _ => throw new ArgumentOutOfRangeException(nameof(suit))
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
}
