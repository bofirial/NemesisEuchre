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
                new PlayedCard(new Card(Suit.Hearts, Rank.Ace), PlayerPosition.North),
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
                new PlayedCard(new Card(Suit.Hearts, Rank.Nine), PlayerPosition.North),
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
                new PlayedCard(new Card(Suit.Spades, Rank.Ace), PlayerPosition.North),
                new PlayedCard(new Card(Suit.Spades, Rank.Jack), PlayerPosition.East),
                new PlayedCard(new Card(Suit.Clubs, Rank.Jack), PlayerPosition.South),
                new PlayedCard(new Card(Suit.Spades, Rank.King), PlayerPosition.West),
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
                new PlayedCard(new Card(Suit.Spades, Rank.Ace), PlayerPosition.North),
                new PlayedCard(new Card(Suit.Spades, Rank.King), PlayerPosition.East),
                new PlayedCard(new Card(Suit.Clubs, Rank.Jack), PlayerPosition.South),
                new PlayedCard(new Card(Suit.Spades, Rank.Queen), PlayerPosition.West),
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
                new PlayedCard(new Card(Suit.Hearts, Rank.Ace), PlayerPosition.North),
                new PlayedCard(new Card(Suit.Spades, Rank.Nine), PlayerPosition.East),
                new PlayedCard(new Card(Suit.Hearts, Rank.King), PlayerPosition.South),
                new PlayedCard(new Card(Suit.Diamonds, Rank.Queen), PlayerPosition.West),
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
                new PlayedCard(new Card(Suit.Spades, Rank.Nine), PlayerPosition.North),
                new PlayedCard(new Card(Suit.Spades, Rank.Ace), PlayerPosition.East),
                new PlayedCard(new Card(Suit.Spades, Rank.Ten), PlayerPosition.South),
                new PlayedCard(new Card(Suit.Spades, Rank.King), PlayerPosition.West),
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
                new PlayedCard(new Card(Suit.Hearts, Rank.Nine), PlayerPosition.North),
                new PlayedCard(new Card(Suit.Hearts, Rank.Ace), PlayerPosition.East),
                new PlayedCard(new Card(Suit.Hearts, Rank.Ten), PlayerPosition.South),
                new PlayedCard(new Card(Suit.Hearts, Rank.King), PlayerPosition.West),
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
                new PlayedCard(new Card(Suit.Hearts, Rank.Nine), PlayerPosition.North),
                new PlayedCard(new Card(Suit.Diamonds, Rank.Ace), PlayerPosition.East),
                new PlayedCard(new Card(Suit.Clubs, Rank.Ace), PlayerPosition.South),
                new PlayedCard(new Card(Suit.Hearts, Rank.Ten), PlayerPosition.West),
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
                new PlayedCard(new Card(Suit.Clubs, Rank.Nine), PlayerPosition.North),
                new PlayedCard(new Card(Suit.Clubs, Rank.Jack), PlayerPosition.East),
                new PlayedCard(new Card(Suit.Clubs, Rank.Ace), PlayerPosition.South),
                new PlayedCard(new Card(Suit.Clubs, Rank.Ten), PlayerPosition.West),
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
                new PlayedCard(new Card(Suit.Hearts, Rank.Queen), PlayerPosition.North),
                new PlayedCard(new Card(Suit.Hearts, Rank.Ace), PlayerPosition.East),
                new PlayedCard(new Card(Suit.Diamonds, Rank.Jack), PlayerPosition.South),
                new PlayedCard(new Card(Suit.Hearts, Rank.Jack), PlayerPosition.West),
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
                new PlayedCard(new Card(Suit.Hearts, Rank.Nine), PlayerPosition.North),
                new PlayedCard(new Card(Suit.Hearts, Rank.King), PlayerPosition.East),
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
                new PlayedCard(new Card(Suit.Hearts, Rank.Nine), PlayerPosition.North),
                new PlayedCard(new Card(Suit.Hearts, Rank.King), PlayerPosition.East),
                new PlayedCard(new Card(Suit.Hearts, Rank.Ace), PlayerPosition.South),
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
                new PlayedCard(new Card(Suit.Diamonds, Rank.Ace), PlayerPosition.North),
                new PlayedCard(new Card(trump, Rank.Nine), PlayerPosition.East),
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
                new PlayedCard(new Card(Suit.Diamonds, Rank.Ace), PlayerPosition.North),
                new PlayedCard(new Card(Suit.Hearts, Rank.Ace), PlayerPosition.East),
                new PlayedCard(new Card(Suit.Clubs, Rank.Ten), PlayerPosition.South),
                new PlayedCard(new Card(Suit.Diamonds, Rank.King), PlayerPosition.West),
            },
        };

        var winner = _calculator.CalculateWinner(trick, Suit.Clubs);

        winner.Should().Be(PlayerPosition.South);
    }
}
