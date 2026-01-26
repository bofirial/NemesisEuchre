using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;

namespace NemesisEuchre.GameEngine.Tests.Extensions;

public class SuitExtensionsTests
{
    [Theory]
    [InlineData(Suit.Spades, Suit.Clubs)]
    [InlineData(Suit.Clubs, Suit.Spades)]
    [InlineData(Suit.Hearts, Suit.Diamonds)]
    [InlineData(Suit.Diamonds, Suit.Hearts)]
    public void GetSameColorSuit_WithAnySuit_ReturnsMatchingColorSuit(Suit input, Suit expected)
    {
        var result = input.GetSameColorSuit();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Suit.Hearts, true)]
    [InlineData(Suit.Diamonds, true)]
    [InlineData(Suit.Spades, false)]
    [InlineData(Suit.Clubs, false)]
    public void IsRed_WithAnySuit_ReturnsTrueForRedSuits(Suit suit, bool expected)
    {
        var result = suit.IsRed();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Suit.Spades, true)]
    [InlineData(Suit.Clubs, true)]
    [InlineData(Suit.Hearts, false)]
    [InlineData(Suit.Diamonds, false)]
    public void IsBlack_WithAnySuit_ReturnsTrueForBlackSuits(Suit suit, bool expected)
    {
        var result = suit.IsBlack();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Suit.Spades, Suit.Spades, RelativeSuit.Trump)]
    [InlineData(Suit.Spades, Suit.Clubs, RelativeSuit.NonTrumpSameColor)]
    [InlineData(Suit.Spades, Suit.Hearts, RelativeSuit.NonTrumpOppositeColor1)]
    [InlineData(Suit.Spades, Suit.Diamonds, RelativeSuit.NonTrumpOppositeColor2)]
    [InlineData(Suit.Hearts, Suit.Hearts, RelativeSuit.Trump)]
    [InlineData(Suit.Hearts, Suit.Diamonds, RelativeSuit.NonTrumpSameColor)]
    [InlineData(Suit.Hearts, Suit.Spades, RelativeSuit.NonTrumpOppositeColor1)]
    [InlineData(Suit.Hearts, Suit.Clubs, RelativeSuit.NonTrumpOppositeColor2)]
    [InlineData(Suit.Clubs, Suit.Clubs, RelativeSuit.Trump)]
    [InlineData(Suit.Clubs, Suit.Spades, RelativeSuit.NonTrumpSameColor)]
    [InlineData(Suit.Clubs, Suit.Hearts, RelativeSuit.NonTrumpOppositeColor1)]
    [InlineData(Suit.Clubs, Suit.Diamonds, RelativeSuit.NonTrumpOppositeColor2)]
    [InlineData(Suit.Diamonds, Suit.Diamonds, RelativeSuit.Trump)]
    [InlineData(Suit.Diamonds, Suit.Hearts, RelativeSuit.NonTrumpSameColor)]
    [InlineData(Suit.Diamonds, Suit.Spades, RelativeSuit.NonTrumpOppositeColor1)]
    [InlineData(Suit.Diamonds, Suit.Clubs, RelativeSuit.NonTrumpOppositeColor2)]
    public void ToRelativeSuit_WithTrumpAndSuit_ConvertsCorrectly(Suit trump, Suit suit, RelativeSuit expected)
    {
        var result = suit.ToRelativeSuit(trump);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Suit.Spades)]
    [InlineData(Suit.Hearts)]
    [InlineData(Suit.Clubs)]
    [InlineData(Suit.Diamonds)]
    public void ToRelativeSuit_WithAnyTrump_ProducesOneOfEachRelativeSuit(Suit trump)
    {
        var allSuits = new[] { Suit.Spades, Suit.Hearts, Suit.Clubs, Suit.Diamonds };
        var relativeSuits = allSuits.Select(s => s.ToRelativeSuit(trump)).ToList();

        relativeSuits.Should().Contain(RelativeSuit.Trump);
        relativeSuits.Should().Contain(RelativeSuit.NonTrumpSameColor);
        relativeSuits.Should().Contain(RelativeSuit.NonTrumpOppositeColor1);
        relativeSuits.Should().Contain(RelativeSuit.NonTrumpOppositeColor2);
        relativeSuits.Should().HaveCount(4);
    }
}
