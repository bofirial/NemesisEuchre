using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;

namespace NemesisEuchre.GameEngine.Tests;

public class SuitExtensionsTests
{
    [Theory]
    [InlineData(Suit.Spades, Suit.Clubs)]
    [InlineData(Suit.Clubs, Suit.Spades)]
    [InlineData(Suit.Hearts, Suit.Diamonds)]
    [InlineData(Suit.Diamonds, Suit.Hearts)]
    public void GetSameSuitColorShouldReturnMatchingColorSuit(Suit input, Suit expected)
    {
        var result = input.GetSameSuitColor();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Suit.Hearts, true)]
    [InlineData(Suit.Diamonds, true)]
    [InlineData(Suit.Spades, false)]
    [InlineData(Suit.Clubs, false)]
    public void IsRedShouldReturnTrueForRedSuits(Suit suit, bool expected)
    {
        var result = suit.IsRed();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Suit.Spades, true)]
    [InlineData(Suit.Clubs, true)]
    [InlineData(Suit.Hearts, false)]
    [InlineData(Suit.Diamonds, false)]
    public void IsBlackShouldReturnTrueForBlackSuits(Suit suit, bool expected)
    {
        var result = suit.IsBlack();

        result.Should().Be(expected);
    }
}
