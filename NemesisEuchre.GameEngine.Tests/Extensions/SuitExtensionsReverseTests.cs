using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;

namespace NemesisEuchre.GameEngine.Tests.Extensions;

public class SuitExtensionsReverseTests
{
    [Theory]
    [InlineData(RelativeSuit.Trump, Suit.Spades, Suit.Spades)]
    [InlineData(RelativeSuit.NonTrumpSameColor, Suit.Spades, Suit.Clubs)]
    [InlineData(RelativeSuit.NonTrumpOppositeColor1, Suit.Spades, Suit.Hearts)]
    [InlineData(RelativeSuit.NonTrumpOppositeColor2, Suit.Spades, Suit.Diamonds)]
    [InlineData(RelativeSuit.Trump, Suit.Hearts, Suit.Hearts)]
    [InlineData(RelativeSuit.NonTrumpSameColor, Suit.Hearts, Suit.Diamonds)]
    [InlineData(RelativeSuit.NonTrumpOppositeColor1, Suit.Hearts, Suit.Spades)]
    [InlineData(RelativeSuit.NonTrumpOppositeColor2, Suit.Hearts, Suit.Clubs)]
    [InlineData(RelativeSuit.Trump, Suit.Clubs, Suit.Clubs)]
    [InlineData(RelativeSuit.NonTrumpSameColor, Suit.Clubs, Suit.Spades)]
    [InlineData(RelativeSuit.NonTrumpOppositeColor1, Suit.Clubs, Suit.Hearts)]
    [InlineData(RelativeSuit.NonTrumpOppositeColor2, Suit.Clubs, Suit.Diamonds)]
    [InlineData(RelativeSuit.Trump, Suit.Diamonds, Suit.Diamonds)]
    [InlineData(RelativeSuit.NonTrumpSameColor, Suit.Diamonds, Suit.Hearts)]
    [InlineData(RelativeSuit.NonTrumpOppositeColor1, Suit.Diamonds, Suit.Spades)]
    [InlineData(RelativeSuit.NonTrumpOppositeColor2, Suit.Diamonds, Suit.Clubs)]
    public void ToAbsoluteSuit_WithRelativeSuitAndTrump_ReturnsCorrectSuit(
        RelativeSuit relativeSuit, Suit trump, Suit expected)
    {
        var result = relativeSuit.ToAbsoluteSuit(trump);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Suit.Spades)]
    [InlineData(Suit.Hearts)]
    [InlineData(Suit.Clubs)]
    [InlineData(Suit.Diamonds)]
    public void ToAbsoluteSuit_RoundTrip_WithAllSuitsAndTrumps_IsReversible(Suit trump)
    {
        foreach (var suit in new[] { Suit.Spades, Suit.Hearts, Suit.Clubs, Suit.Diamonds })
        {
            var relativeSuit = suit.ToRelativeSuit(trump, Rank.King);
            var backToAbsolute = relativeSuit.ToAbsoluteSuit(trump);

            backToAbsolute.Should().Be(
                suit,
                "round-trip should preserve suit {0} with trump {1}",
                suit,
                trump);
        }
    }
}
