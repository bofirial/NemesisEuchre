using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Tests;

public class TrickWinnerCalculatorTests
{
    private readonly TrickWinnerCalculator _calculator = new();

    [Fact]
    public void CalculateWinner_WithNullTrick_ThrowsArgumentNullException()
    {
        var act = () => _calculator.CalculateWinner(null!, Suit.Spades);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CalculateWinner_WithNoCardsPlayed_ThrowsInvalidOperationException()
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.North,
            LeadSuit = Suit.Hearts,
        };

        var act = () => _calculator.CalculateWinner(trick, Suit.Spades);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot calculate winner of a trick with no cards played");
    }

    [Fact]
    public void CalculateWinner_WithNoLeadSuit_ThrowsInvalidOperationException()
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.North,
            CardsPlayed =
            {
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
                    PlayerPosition = PlayerPosition.North,
                },
            },
        };

        var act = () => _calculator.CalculateWinner(trick, Suit.Spades);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot calculate winner of a trick with no lead suit");
    }

    [Fact]
    public void CalculateWinner_WithSingleCard_ReturnsThatPlayerPosition()
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.North,
            LeadSuit = Suit.Hearts,
            CardsPlayed =
            {
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
                    PlayerPosition = PlayerPosition.North,
                },
            },
        };

        var winner = _calculator.CalculateWinner(trick, Suit.Spades);

        winner.Should().Be(PlayerPosition.North);
    }

    [Fact]
    public void CalculateWinner_WithRightBower_ReturnsRightBowerPlayer()
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.North,
            LeadSuit = Suit.Spades,
            CardsPlayed =
            {
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Spades, Rank = Rank.Ace },
                    PlayerPosition = PlayerPosition.North,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Spades, Rank = Rank.Jack },
                    PlayerPosition = PlayerPosition.East,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Clubs, Rank = Rank.Jack },
                    PlayerPosition = PlayerPosition.South,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Spades, Rank = Rank.King },
                    PlayerPosition = PlayerPosition.West,
                },
            },
        };

        var winner = _calculator.CalculateWinner(trick, Suit.Spades);

        winner.Should().Be(PlayerPosition.East);
    }

    [Fact]
    public void CalculateWinner_WithLeftBower_ReturnsLeftBowerPlayerWhenNoRightBower()
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.North,
            LeadSuit = Suit.Spades,
            CardsPlayed =
            {
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Spades, Rank = Rank.Ace },
                    PlayerPosition = PlayerPosition.North,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Spades, Rank = Rank.King },
                    PlayerPosition = PlayerPosition.East,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Clubs, Rank = Rank.Jack },
                    PlayerPosition = PlayerPosition.South,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Spades, Rank = Rank.Queen },
                    PlayerPosition = PlayerPosition.West,
                },
            },
        };

        var winner = _calculator.CalculateWinner(trick, Suit.Spades);

        winner.Should().Be(PlayerPosition.South);
    }

    [Fact]
    public void CalculateWinner_WithTrumpAndNonTrump_ReturnsTrumpPlayer()
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.North,
            LeadSuit = Suit.Hearts,
            CardsPlayed =
            {
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
                    PlayerPosition = PlayerPosition.North,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Spades, Rank = Rank.Nine },
                    PlayerPosition = PlayerPosition.East,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.King },
                    PlayerPosition = PlayerPosition.South,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Diamonds, Rank = Rank.Queen },
                    PlayerPosition = PlayerPosition.West,
                },
            },
        };

        var winner = _calculator.CalculateWinner(trick, Suit.Spades);

        winner.Should().Be(PlayerPosition.East);
    }

    [Fact]
    public void CalculateWinner_WithMultipleTrumpCards_ReturnsHighestTrumpPlayer()
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.North,
            LeadSuit = Suit.Hearts,
            CardsPlayed =
            {
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Spades, Rank = Rank.Nine },
                    PlayerPosition = PlayerPosition.North,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Spades, Rank = Rank.Ace },
                    PlayerPosition = PlayerPosition.East,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Spades, Rank = Rank.Ten },
                    PlayerPosition = PlayerPosition.South,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Spades, Rank = Rank.King },
                    PlayerPosition = PlayerPosition.West,
                },
            },
        };

        var winner = _calculator.CalculateWinner(trick, Suit.Spades);

        winner.Should().Be(PlayerPosition.East);
    }

    [Fact]
    public void CalculateWinner_WithNoTrump_ReturnsHighestLeadSuitPlayer()
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.North,
            LeadSuit = Suit.Hearts,
            CardsPlayed =
            {
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
                    PlayerPosition = PlayerPosition.North,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
                    PlayerPosition = PlayerPosition.East,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Ten },
                    PlayerPosition = PlayerPosition.South,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.King },
                    PlayerPosition = PlayerPosition.West,
                },
            },
        };

        var winner = _calculator.CalculateWinner(trick, Suit.Spades);

        winner.Should().Be(PlayerPosition.East);
    }

    [Fact]
    public void CalculateWinner_WithOffSuitCards_IgnoresThemInFavorOfLeadSuit()
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.North,
            LeadSuit = Suit.Hearts,
            CardsPlayed =
            {
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
                    PlayerPosition = PlayerPosition.North,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Diamonds, Rank = Rank.Ace },
                    PlayerPosition = PlayerPosition.East,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Clubs, Rank = Rank.Ace },
                    PlayerPosition = PlayerPosition.South,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Ten },
                    PlayerPosition = PlayerPosition.West,
                },
            },
        };

        var winner = _calculator.CalculateWinner(trick, Suit.Spades);

        winner.Should().Be(PlayerPosition.West);
    }

    [Fact]
    public void CalculateWinner_WithLeftBowerInLeadSuit_CountsAsOffSuit()
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.North,
            LeadSuit = Suit.Clubs,
            CardsPlayed =
            {
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Clubs, Rank = Rank.Nine },
                    PlayerPosition = PlayerPosition.North,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Clubs, Rank = Rank.Jack },
                    PlayerPosition = PlayerPosition.East,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Clubs, Rank = Rank.Ace },
                    PlayerPosition = PlayerPosition.South,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Clubs, Rank = Rank.Ten },
                    PlayerPosition = PlayerPosition.West,
                },
            },
        };

        var winner = _calculator.CalculateWinner(trick, Suit.Spades);

        winner.Should().Be(PlayerPosition.East);
    }

    [Fact]
    public void CalculateWinner_WithAllRanksOfTrump_ReturnsCorrectOrder()
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.North,
            LeadSuit = Suit.Hearts,
            CardsPlayed =
            {
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Queen },
                    PlayerPosition = PlayerPosition.North,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
                    PlayerPosition = PlayerPosition.East,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Diamonds, Rank = Rank.Jack },
                    PlayerPosition = PlayerPosition.South,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Jack },
                    PlayerPosition = PlayerPosition.West,
                },
            },
        };

        var winner = _calculator.CalculateWinner(trick, Suit.Hearts);

        winner.Should().Be(PlayerPosition.West);
    }

    [Fact]
    public void CalculateWinner_WithTwoPlayers_ReturnsCorrectWinner()
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.North,
            LeadSuit = Suit.Hearts,
            CardsPlayed =
            {
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
                    PlayerPosition = PlayerPosition.North,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.King },
                    PlayerPosition = PlayerPosition.East,
                },
            },
        };

        var winner = _calculator.CalculateWinner(trick, Suit.Spades);

        winner.Should().Be(PlayerPosition.East);
    }

    [Fact]
    public void CalculateWinner_WithThreePlayers_ReturnsCorrectWinner()
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.North,
            LeadSuit = Suit.Hearts,
            CardsPlayed =
            {
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
                    PlayerPosition = PlayerPosition.North,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.King },
                    PlayerPosition = PlayerPosition.East,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
                    PlayerPosition = PlayerPosition.South,
                },
            },
        };

        var winner = _calculator.CalculateWinner(trick, Suit.Spades);

        winner.Should().Be(PlayerPosition.South);
    }

    [Theory]
    [InlineData(Suit.Spades)]
    [InlineData(Suit.Hearts)]
    [InlineData(Suit.Clubs)]
    public void CalculateWinner_WithDifferentTrumpSuits_WorksCorrectly(Suit trump)
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.North,
            LeadSuit = Suit.Diamonds,
            CardsPlayed =
            {
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Diamonds, Rank = Rank.Ace },
                    PlayerPosition = PlayerPosition.North,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = trump, Rank = Rank.Nine },
                    PlayerPosition = PlayerPosition.East,
                },
            },
        };

        var winner = _calculator.CalculateWinner(trick, trump);

        winner.Should().Be(PlayerPosition.East);
    }

    [Fact]
    public void CalculateWinner_ComplexScenario_ReturnsCorrectWinner()
    {
        var trick = new Trick
        {
            LeadPosition = PlayerPosition.North,
            LeadSuit = Suit.Diamonds,
            CardsPlayed =
            {
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Diamonds, Rank = Rank.Ace },
                    PlayerPosition = PlayerPosition.North,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
                    PlayerPosition = PlayerPosition.East,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Clubs, Rank = Rank.Ten },
                    PlayerPosition = PlayerPosition.South,
                },
                new PlayedCard
                {
                    Card = new Card { Suit = Suit.Diamonds, Rank = Rank.King },
                    PlayerPosition = PlayerPosition.West,
                },
            },
        };

        var winner = _calculator.CalculateWinner(trick, Suit.Clubs);

        winner.Should().Be(PlayerPosition.South);
    }
}
