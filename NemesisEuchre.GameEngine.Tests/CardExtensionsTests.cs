using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests;

public class CardExtensionsTests
{
    [Theory]
    [InlineData(Suit.Spades, Rank.Jack, Suit.Spades, true)]
    [InlineData(Suit.Hearts, Rank.Jack, Suit.Hearts, true)]
    [InlineData(Suit.Clubs, Rank.Jack, Suit.Clubs, true)]
    [InlineData(Suit.Diamonds, Rank.Jack, Suit.Diamonds, true)]
    [InlineData(Suit.Spades, Rank.Jack, Suit.Hearts, false)]
    [InlineData(Suit.Clubs, Rank.Jack, Suit.Spades, false)]
    [InlineData(Suit.Spades, Rank.Ace, Suit.Spades, false)]
    [InlineData(Suit.Hearts, Rank.Queen, Suit.Hearts, false)]
    public void IsRightBowerShouldReturnTrueWhenCardIsJackOfTrumpSuit(Suit cardSuit, Rank cardRank, Suit trump, bool expected)
    {
        var card = new Card { Suit = cardSuit, Rank = cardRank };

        var result = card.IsRightBower(trump);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Suit.Clubs, Rank.Jack, Suit.Spades, true)]
    [InlineData(Suit.Spades, Rank.Jack, Suit.Clubs, true)]
    [InlineData(Suit.Diamonds, Rank.Jack, Suit.Hearts, true)]
    [InlineData(Suit.Hearts, Rank.Jack, Suit.Diamonds, true)]
    [InlineData(Suit.Spades, Rank.Jack, Suit.Spades, false)]
    [InlineData(Suit.Hearts, Rank.Jack, Suit.Hearts, false)]
    [InlineData(Suit.Clubs, Rank.Ace, Suit.Spades, false)]
    [InlineData(Suit.Diamonds, Rank.Queen, Suit.Hearts, false)]
    public void IsLeftBowerShouldReturnTrueWhenCardIsJackOfSameColorAsTrump(Suit cardSuit, Rank cardRank, Suit trump, bool expected)
    {
        var card = new Card { Suit = cardSuit, Rank = cardRank };

        var result = card.IsLeftBower(trump);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Suit.Clubs, Rank.Jack, Suit.Spades, Suit.Spades)]
    [InlineData(Suit.Spades, Rank.Jack, Suit.Clubs, Suit.Clubs)]
    [InlineData(Suit.Diamonds, Rank.Jack, Suit.Hearts, Suit.Hearts)]
    [InlineData(Suit.Hearts, Rank.Jack, Suit.Diamonds, Suit.Diamonds)]
    [InlineData(Suit.Spades, Rank.Jack, Suit.Spades, Suit.Spades)]
    [InlineData(Suit.Spades, Rank.Ace, Suit.Hearts, Suit.Spades)]
    [InlineData(Suit.Hearts, Rank.Nine, Suit.Clubs, Suit.Hearts)]
    public void GetEffectiveSuitShouldReturnTrumpForLeftBowerOtherwiseOriginalSuit(Suit cardSuit, Rank cardRank, Suit trump, Suit expected)
    {
        var card = new Card { Suit = cardSuit, Rank = cardRank };

        var result = card.GetEffectiveSuit(trump);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Suit.Spades, Rank.Jack, Suit.Spades, true)]
    [InlineData(Suit.Clubs, Rank.Jack, Suit.Spades, true)]
    [InlineData(Suit.Spades, Rank.Ace, Suit.Spades, true)]
    [InlineData(Suit.Hearts, Rank.Jack, Suit.Diamonds, true)]
    [InlineData(Suit.Hearts, Rank.Nine, Suit.Hearts, true)]
    [InlineData(Suit.Spades, Rank.Ace, Suit.Hearts, false)]
    [InlineData(Suit.Clubs, Rank.King, Suit.Diamonds, false)]
    public void IsTrumpShouldReturnTrueWhenCardBelongsToTrumpSuit(Suit cardSuit, Rank cardRank, Suit trump, bool expected)
    {
        var card = new Card { Suit = cardSuit, Rank = cardRank };

        var result = card.IsTrump(trump);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetTrumpValueShouldReturn16ForRightBower()
    {
        var card = new Card { Suit = Suit.Spades, Rank = Rank.Jack };

        var result = card.GetTrumpValue(Suit.Spades);

        result.Should().Be(16);
    }

    [Fact]
    public void GetTrumpValueShouldReturn15ForLeftBower()
    {
        var card = new Card { Suit = Suit.Clubs, Rank = Rank.Jack };

        var result = card.GetTrumpValue(Suit.Spades);

        result.Should().Be(15);
    }

    [Fact]
    public void GetTrumpValueShouldReturnNegative1ForNonTrump()
    {
        var card = new Card { Suit = Suit.Hearts, Rank = Rank.Ace };

        var result = card.GetTrumpValue(Suit.Spades);

        result.Should().Be(-1);
    }

    [Theory]
    [InlineData(Suit.Spades, Rank.Ace, Suit.Spades, 14)]
    [InlineData(Suit.Spades, Rank.King, Suit.Spades, 13)]
    [InlineData(Suit.Spades, Rank.Queen, Suit.Spades, 12)]
    [InlineData(Suit.Spades, Rank.Ten, Suit.Spades, 10)]
    [InlineData(Suit.Spades, Rank.Nine, Suit.Spades, 9)]
    [InlineData(Suit.Hearts, Rank.Ace, Suit.Hearts, 14)]
    public void GetTrumpValueShouldReturnRankValueForTrumpCards(Suit cardSuit, Rank cardRank, Suit trump, int expected)
    {
        var card = new Card { Suit = cardSuit, Rank = cardRank };

        var result = card.GetTrumpValue(trump);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Rank.Nine, Suit.Spades, "9♠")]
    [InlineData(Rank.Ten, Suit.Hearts, "10♥")]
    [InlineData(Rank.Jack, Suit.Clubs, "J♣")]
    [InlineData(Rank.Queen, Suit.Diamonds, "Q♦")]
    [InlineData(Rank.King, Suit.Spades, "K♠")]
    [InlineData(Rank.Ace, Suit.Hearts, "A♥")]
    public void ToDisplayStringShouldReturnCorrectSymbolRepresentation(Rank rank, Suit suit, string expected)
    {
        var card = new Card { Suit = suit, Rank = rank };

        var result = card.ToDisplayString();

        result.Should().Be(expected);
    }

    [Fact]
    public void ToDisplayStringShouldCoverAllRanks()
    {
        var ranks = new[] { Rank.Nine, Rank.Ten, Rank.Jack, Rank.Queen, Rank.King, Rank.Ace };
        var results = ranks.Select(r => new Card { Suit = Suit.Spades, Rank = r }.ToDisplayString()).ToList();

        results.Should().BeEquivalentTo("9♠", "10♠", "J♠", "Q♠", "K♠", "A♠");
    }

    [Fact]
    public void ToDisplayStringShouldCoverAllSuits()
    {
        var suits = new[] { Suit.Spades, Suit.Hearts, Suit.Clubs, Suit.Diamonds };
        var results = suits.Select(s => new Card { Suit = s, Rank = Rank.Ace }.ToDisplayString()).ToList();

        results.Should().BeEquivalentTo("A♠", "A♥", "A♣", "A♦");
    }
}
