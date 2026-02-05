using FluentAssertions;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Services;

namespace NemesisEuchre.GameEngine.Tests.Services;

public class VoidDetectorTests
{
    private readonly VoidDetector _detector = new();

    [Fact]
    public void TryDetectVoid_WhenLeadSuitIsNull_ReturnsFalse()
    {
        var deal = new Deal();
        var chosenCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };

        var result = _detector.TryDetectVoid(
            deal,
            chosenCard,
            leadSuit: null,
            trump: Suit.Spades,
            playerPosition: PlayerPosition.North,
            out var voidSuit);

        result.Should().BeFalse();
        voidSuit.Should().Be(default);
    }

    [Fact]
    public void TryDetectVoid_WhenPlayerFollowsSuit_ReturnsFalse()
    {
        var deal = new Deal();
        var chosenCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };

        var result = _detector.TryDetectVoid(
            deal,
            chosenCard,
            leadSuit: Suit.Hearts,
            trump: Suit.Spades,
            playerPosition: PlayerPosition.North,
            out var voidSuit);

        result.Should().BeFalse();
        voidSuit.Should().Be(default);
    }

    [Fact]
    public void TryDetectVoid_WhenPlayerDoesNotFollowSuit_ReturnsTrueAndIdentifiesVoidSuit()
    {
        var deal = new Deal();
        var chosenCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };

        var result = _detector.TryDetectVoid(
            deal,
            chosenCard,
            leadSuit: Suit.Clubs,
            trump: Suit.Spades,
            playerPosition: PlayerPosition.North,
            out var voidSuit);

        result.Should().BeTrue();
        voidSuit.Should().Be(Suit.Clubs);
    }

    [Fact]
    public void TryDetectVoid_WhenLeftBowerPlayed_UsesEffectiveSuit()
    {
        var deal = new Deal();
        var leftBower = new Card { Suit = Suit.Clubs, Rank = Rank.Jack };

        var result = _detector.TryDetectVoid(
            deal,
            leftBower,
            leadSuit: Suit.Hearts,
            trump: Suit.Spades,
            playerPosition: PlayerPosition.North,
            out var voidSuit);

        result.Should().BeTrue();
        voidSuit.Should().Be(Suit.Hearts);
    }

    [Fact]
    public void TryDetectVoid_WhenLeftBowerPlayedAndTrumpIsLead_PlayerFollowsSuit()
    {
        var deal = new Deal();
        var leftBower = new Card { Suit = Suit.Clubs, Rank = Rank.Jack };

        var result = _detector.TryDetectVoid(
            deal,
            leftBower,
            leadSuit: Suit.Spades,
            trump: Suit.Spades,
            playerPosition: PlayerPosition.North,
            out var voidSuit);

        result.Should().BeFalse();
        voidSuit.Should().Be(default);
    }

    [Fact]
    public void TryDetectVoid_WhenVoidAlreadyKnown_ReturnsFalse()
    {
        var deal = new Deal
        {
            KnownPlayerSuitVoids =
            [
                (PlayerPosition.North, Suit.Clubs)
            ],
        };
        var chosenCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };

        var result = _detector.TryDetectVoid(
            deal,
            chosenCard,
            leadSuit: Suit.Clubs,
            trump: Suit.Spades,
            playerPosition: PlayerPosition.North,
            out var voidSuit);

        result.Should().BeFalse();
        voidSuit.Should().Be(default);
    }

    [Fact]
    public void TryDetectVoid_WhenDifferentPlayerHasSameVoid_ReturnsTrue()
    {
        var deal = new Deal
        {
            KnownPlayerSuitVoids =
            [
                (PlayerPosition.South, Suit.Clubs)
            ],
        };
        var chosenCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };

        var result = _detector.TryDetectVoid(
            deal,
            chosenCard,
            leadSuit: Suit.Clubs,
            trump: Suit.Spades,
            playerPosition: PlayerPosition.North,
            out var voidSuit);

        result.Should().BeTrue();
        voidSuit.Should().Be(Suit.Clubs);
    }

    [Fact]
    public void TryDetectVoid_WhenSamePlayerHasDifferentVoid_ReturnsTrue()
    {
        var deal = new Deal
        {
            KnownPlayerSuitVoids =
            [
                (PlayerPosition.North, Suit.Diamonds)
            ],
        };
        var chosenCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };

        var result = _detector.TryDetectVoid(
            deal,
            chosenCard,
            leadSuit: Suit.Clubs,
            trump: Suit.Spades,
            playerPosition: PlayerPosition.North,
            out var voidSuit);

        result.Should().BeTrue();
        voidSuit.Should().Be(Suit.Clubs);
    }
}
