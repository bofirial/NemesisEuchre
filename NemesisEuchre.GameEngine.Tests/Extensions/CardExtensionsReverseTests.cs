using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests.Extensions;

public class CardExtensionsReverseTests
{
    [Theory]
    [InlineData(Suit.Spades)]
    [InlineData(Suit.Hearts)]
    [InlineData(Suit.Clubs)]
    [InlineData(Suit.Diamonds)]
    public void ToAbsolute_RoundTrip_WithAllCardsAndTrumps_IsReversible(Suit trump)
    {
        var allSuits = new[] { Suit.Spades, Suit.Hearts, Suit.Clubs, Suit.Diamonds };
        var allRanks = new[] { Rank.Nine, Rank.Ten, Rank.Jack, Rank.Queen, Rank.King, Rank.Ace };

        foreach (var suit in allSuits)
        {
            foreach (var rank in allRanks)
            {
                var card = new Card(suit, rank);
                var relative = card.ToRelative(trump);
                var backToAbsolute = relative.ToAbsolute(trump);

                backToAbsolute.Should().Be(
                    card,
                    "round-trip should preserve card {0} with trump {1}",
                    card,
                    trump);
            }
        }
    }

    [Fact]
    public void ToAbsolute_WithRightBower_ReturnsJackOfTrump()
    {
        var relative = new Card(Suit.Spades, Rank.Jack).ToRelative(Suit.Spades);
        relative.Rank.Should().Be(Rank.RightBower);

        var absolute = relative.ToAbsolute(Suit.Spades);

        absolute.Should().Be(new Card(Suit.Spades, Rank.Jack));
    }

    [Fact]
    public void ToAbsolute_WithLeftBower_ReturnsJackOfSameColorSuit()
    {
        var relative = new Card(Suit.Clubs, Rank.Jack).ToRelative(Suit.Spades);
        relative.Rank.Should().Be(Rank.LeftBower);

        var absolute = relative.ToAbsolute(Suit.Spades);

        absolute.Should().Be(new Card(Suit.Clubs, Rank.Jack));
    }

    [Theory]
    [InlineData(Suit.Spades, Suit.Clubs)]
    [InlineData(Suit.Clubs, Suit.Spades)]
    [InlineData(Suit.Hearts, Suit.Diamonds)]
    [InlineData(Suit.Diamonds, Suit.Hearts)]
    public void ToAbsolute_WithLeftBower_ForAllTrumps_ReturnsCorrectCard(Suit trump, Suit expectedLeftBowerSuit)
    {
        var leftBowerCard = new Card(expectedLeftBowerSuit, Rank.Jack);
        var relative = leftBowerCard.ToRelative(trump);
        var absolute = relative.ToAbsolute(trump);

        absolute.Should().Be(leftBowerCard);
    }

    [Theory]
    [InlineData(Suit.Spades)]
    [InlineData(Suit.Hearts)]
    [InlineData(Suit.Clubs)]
    [InlineData(Suit.Diamonds)]
    public void ToAbsolute_WithRightBower_ForAllTrumps_ReturnsCorrectCard(Suit trump)
    {
        var rightBowerCard = new Card(trump, Rank.Jack);
        var relative = rightBowerCard.ToRelative(trump);
        var absolute = relative.ToAbsolute(trump);

        absolute.Should().Be(rightBowerCard);
    }

    [Fact]
    public void ToAbsolute_WithNonBowerCard_PreservesRank()
    {
        var card = new Card(Suit.Hearts, Rank.Ace);
        var relative = card.ToRelative(Suit.Spades);
        var absolute = relative.ToAbsolute(Suit.Spades);

        absolute.Rank.Should().Be(Rank.Ace);
        absolute.Suit.Should().Be(Suit.Hearts);
    }
}
