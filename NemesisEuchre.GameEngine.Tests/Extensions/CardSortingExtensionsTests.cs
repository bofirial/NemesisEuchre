using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests.Extensions;

public class CardSortingExtensionsTests
{
    [Fact]
    public void SortByTrump_WithNoTrump_GroupsBySuitThenRank()
    {
        var cards = new[]
        {
            new Card(Suit.Hearts, Rank.Nine),
            new Card(Suit.Spades, Rank.Ace),
            new Card(Suit.Clubs, Rank.Jack),
            new Card(Suit.Diamonds, Rank.Ten),
            new Card(Suit.Hearts, Rank.King),
            new Card(Suit.Spades, Rank.Nine),
        };

        var sorted = cards.SortByTrump(null);

        sorted.Should().HaveCount(6);
        sorted[0].Suit.Should().Be(Suit.Spades);
        sorted[0].Rank.Should().Be(Rank.Ace);
        sorted[1].Suit.Should().Be(Suit.Spades);
        sorted[1].Rank.Should().Be(Rank.Nine);
        sorted[2].Suit.Should().Be(Suit.Hearts);
        sorted[2].Rank.Should().Be(Rank.King);
        sorted[3].Suit.Should().Be(Suit.Hearts);
        sorted[3].Rank.Should().Be(Rank.Nine);
        sorted[4].Suit.Should().Be(Suit.Clubs);
        sorted[4].Rank.Should().Be(Rank.Jack);
        sorted[5].Suit.Should().Be(Suit.Diamonds);
        sorted[5].Rank.Should().Be(Rank.Ten);
    }

    [Fact]
    public void SortByTrump_WithTrump_PutsTrumpCardsFirst()
    {
        const Suit trump = Suit.Hearts;
        var cards = new[]
        {
            new Card(Suit.Spades, Rank.Nine),
            new Card(Suit.Hearts, Rank.Ace),
            new Card(Suit.Hearts, Rank.Jack),
            new Card(Suit.Diamonds, Rank.Jack),
            new Card(Suit.Clubs, Rank.King),
            new Card(Suit.Hearts, Rank.Nine),
        };

        var sorted = cards.SortByTrump(trump);

        sorted.Should().HaveCount(6);
        sorted[0].Suit.Should().Be(Suit.Hearts);
        sorted[0].Rank.Should().Be(Rank.Jack);
        sorted[1].Suit.Should().Be(Suit.Diamonds);
        sorted[1].Rank.Should().Be(Rank.Jack);
        sorted[2].Suit.Should().Be(Suit.Hearts);
        sorted[2].Rank.Should().Be(Rank.Ace);
        sorted[3].Suit.Should().Be(Suit.Hearts);
        sorted[3].Rank.Should().Be(Rank.Nine);
        sorted[4].Suit.Should().Be(Suit.Spades);
        sorted[4].Rank.Should().Be(Rank.Nine);
        sorted[5].Suit.Should().Be(Suit.Clubs);
        sorted[5].Rank.Should().Be(Rank.King);
    }

    [Fact]
    public void SortByTrump_WithTrump_SortsRightBowerFirst()
    {
        const Suit trump = Suit.Clubs;
        var cards = new[]
        {
            new Card(Suit.Spades, Rank.Jack),
            new Card(Suit.Clubs, Rank.Ace),
            new Card(Suit.Clubs, Rank.Jack),
        };

        var sorted = cards.SortByTrump(trump);

        sorted.Should().HaveCount(3);
        sorted[0].Suit.Should().Be(Suit.Clubs);
        sorted[0].Rank.Should().Be(Rank.Jack);
        sorted[1].Suit.Should().Be(Suit.Spades);
        sorted[1].Rank.Should().Be(Rank.Jack);
        sorted[2].Suit.Should().Be(Suit.Clubs);
        sorted[2].Rank.Should().Be(Rank.Ace);
    }

    [Fact]
    public void SortByTrump_WithTrump_SortsNonTrumpBySuitAndRank()
    {
        const Suit trump = Suit.Hearts;
        var cards = new[]
        {
            new Card(Suit.Spades, Rank.Nine),
            new Card(Suit.Clubs, Rank.King),
            new Card(Suit.Diamonds, Rank.Ten),
            new Card(Suit.Clubs, Rank.Ace),
            new Card(Suit.Spades, Rank.Queen),
        };

        var sorted = cards.SortByTrump(trump);

        sorted.Should().HaveCount(5);
        sorted[0].Suit.Should().Be(Suit.Spades);
        sorted[0].Rank.Should().Be(Rank.Queen);
        sorted[1].Suit.Should().Be(Suit.Spades);
        sorted[1].Rank.Should().Be(Rank.Nine);
        sorted[2].Suit.Should().Be(Suit.Clubs);
        sorted[2].Rank.Should().Be(Rank.Ace);
        sorted[3].Suit.Should().Be(Suit.Clubs);
        sorted[3].Rank.Should().Be(Rank.King);
        sorted[4].Suit.Should().Be(Suit.Diamonds);
        sorted[4].Rank.Should().Be(Rank.Ten);
    }

    [Fact]
    public void SortByTrump_WithEmptyArray_ReturnsEmptyArray()
    {
        var cards = Array.Empty<Card>();

        var sorted = cards.SortByTrump(Suit.Hearts);

        sorted.Should().BeEmpty();
    }

    [Fact]
    public void SortByTrump_WithSingleCard_ReturnsSingleCard()
    {
        var cards = new[] { new Card(Suit.Spades, Rank.Ace) };

        var sorted = cards.SortByTrump(Suit.Hearts);

        sorted.Should().HaveCount(1);
        sorted[0].Suit.Should().Be(Suit.Spades);
        sorted[0].Rank.Should().Be(Rank.Ace);
    }

    [Fact]
    public void SortByTrump_WithAllTrumpCards_SortsByTrumpValue()
    {
        const Suit trump = Suit.Diamonds;
        var cards = new[]
        {
            new Card(Suit.Diamonds, Rank.Nine),
            new Card(Suit.Diamonds, Rank.Jack),
            new Card(Suit.Diamonds, Rank.Ace),
            new Card(Suit.Hearts, Rank.Jack),
            new Card(Suit.Diamonds, Rank.Ten),
            new Card(Suit.Diamonds, Rank.King),
            new Card(Suit.Diamonds, Rank.Queen),
        };

        var sorted = cards.SortByTrump(trump);

        sorted.Should().HaveCount(7);
        sorted[0].Suit.Should().Be(Suit.Diamonds);
        sorted[0].Rank.Should().Be(Rank.Jack);
        sorted[1].Suit.Should().Be(Suit.Hearts);
        sorted[1].Rank.Should().Be(Rank.Jack);
        sorted[2].Suit.Should().Be(Suit.Diamonds);
        sorted[2].Rank.Should().Be(Rank.Ace);
        sorted[3].Suit.Should().Be(Suit.Diamonds);
        sorted[3].Rank.Should().Be(Rank.King);
        sorted[4].Suit.Should().Be(Suit.Diamonds);
        sorted[4].Rank.Should().Be(Rank.Queen);
        sorted[5].Suit.Should().Be(Suit.Diamonds);
        sorted[5].Rank.Should().Be(Rank.Ten);
        sorted[6].Suit.Should().Be(Suit.Diamonds);
        sorted[6].Rank.Should().Be(Rank.Nine);
    }
}
