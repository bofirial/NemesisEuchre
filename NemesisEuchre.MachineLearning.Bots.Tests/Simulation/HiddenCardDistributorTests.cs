using FluentAssertions;

using Moq;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Utilities;
using NemesisEuchre.MachineLearning.Bots.Simulation;

using Xunit;

namespace NemesisEuchre.MachineLearning.Bots.Tests.Simulation;

public class HiddenCardDistributorTests
{
    private readonly HiddenCardDistributor _distributor = new();
    private readonly Mock<IRandomNumberGenerator> _randomMock = new();

    public HiddenCardDistributorTests()
    {
        var random = new Random(42);
        _randomMock.Setup(r => r.NextInt(It.IsAny<int>())).Returns((int max) => random.Next(max));
    }

    [Fact]
    public void DistributeHiddenCards_AssignsCorrectNumberOfCardsPerPlayer()
    {
        var myHand = new Card[]
        {
            new(Suit.Spades, Rank.Ace),
            new(Suit.Spades, Rank.King),
            new(Suit.Hearts, Rank.Ace),
            new(Suit.Hearts, Rank.King),
            new(Suit.Clubs, Rank.Ace),
        };

        var upCard = new Card(Suit.Diamonds, Rank.Nine);
        Card[] accountedFor = [.. myHand, upCard];

        var result = _distributor.DistributeHiddenCards(
            accountedFor,
            PlayerPosition.South,
            5,
            Suit.Spades,
            [],
            sittingOutPlayer: null,
            _randomMock.Object);

        result.Should().HaveCount(3);
        result.Should().ContainKey(PlayerPosition.North);
        result.Should().ContainKey(PlayerPosition.East);
        result.Should().ContainKey(PlayerPosition.West);
        result.Should().NotContainKey(PlayerPosition.South);

        result[PlayerPosition.North].Should().HaveCount(5);
        result[PlayerPosition.East].Should().HaveCount(5);
        result[PlayerPosition.West].Should().HaveCount(5);
    }

    [Fact]
    public void DistributeHiddenCards_AllCardsAccountedFor()
    {
        var myHand = new Card[]
        {
            new(Suit.Spades, Rank.Ace),
            new(Suit.Spades, Rank.King),
            new(Suit.Hearts, Rank.Ace),
            new(Suit.Hearts, Rank.King),
            new(Suit.Clubs, Rank.Ace),
        };

        Card[] accountedFor = [.. myHand];

        var result = _distributor.DistributeHiddenCards(
            accountedFor,
            PlayerPosition.South,
            5,
            Suit.Spades,
            [],
            sittingOutPlayer: null,
            _randomMock.Object);

        var allDistributed = result.Values.SelectMany(h => h).ToList();
        allDistributed.AddRange(myHand);

        allDistributed.Should().HaveCount(20);
        allDistributed.Distinct().Should().HaveCount(20);
        allDistributed.Should().OnlyContain(c => !myHand.Contains(c) || myHand.Contains(c));
    }

    [Fact]
    public void DistributeHiddenCards_RespectsKnownVoids()
    {
        var myHand = new Card[]
        {
            new(Suit.Spades, Rank.Ace),
            new(Suit.Spades, Rank.King),
            new(Suit.Hearts, Rank.Ace),
            new(Suit.Hearts, Rank.King),
            new(Suit.Clubs, Rank.Ace),
        };

        Card[] accountedFor = [.. myHand];

        var voids = new List<PlayerSuitVoid>
        {
            new(PlayerPosition.North, Suit.Hearts),
        };

        var result = _distributor.DistributeHiddenCards(
            accountedFor,
            PlayerPosition.South,
            5,
            Suit.Diamonds,
            voids,
            sittingOutPlayer: null,
            _randomMock.Object);

        result[PlayerPosition.North].Should().NotContain(c => c.Suit == Suit.Hearts);
    }

    [Fact]
    public void DistributeHiddenCards_SkipsSittingOutPlayer()
    {
        var myHand = new Card[]
        {
            new(Suit.Spades, Rank.Ace),
            new(Suit.Spades, Rank.King),
            new(Suit.Hearts, Rank.Ace),
            new(Suit.Hearts, Rank.King),
            new(Suit.Clubs, Rank.Ace),
        };

        Card[] accountedFor = [.. myHand];

        var result = _distributor.DistributeHiddenCards(
            accountedFor,
            PlayerPosition.South,
            5,
            Suit.Spades,
            [],
            sittingOutPlayer: PlayerPosition.North,
            _randomMock.Object);

        result.Should().HaveCount(2);
        result.Should().ContainKey(PlayerPosition.East);
        result.Should().ContainKey(PlayerPosition.West);
        result.Should().NotContainKey(PlayerPosition.North);
    }

    [Fact]
    public void DistributeHiddenCards_HandlesFewerCardsLaterInDeal()
    {
        var myHand = new Card[]
        {
            new(Suit.Spades, Rank.Ace),
            new(Suit.Hearts, Rank.Ace),
            new(Suit.Clubs, Rank.Ace),
        };

        var playedCards = new Card[]
        {
            new(Suit.Spades, Rank.King),
            new(Suit.Hearts, Rank.King),
            new(Suit.Clubs, Rank.King),
            new(Suit.Diamonds, Rank.King),
            new(Suit.Spades, Rank.Queen),
            new(Suit.Hearts, Rank.Queen),
            new(Suit.Clubs, Rank.Queen),
            new(Suit.Diamonds, Rank.Queen),
        };

        Card[] accountedFor = [.. myHand, .. playedCards];

        var result = _distributor.DistributeHiddenCards(
            accountedFor,
            PlayerPosition.South,
            3,
            Suit.Spades,
            [],
            sittingOutPlayer: null,
            _randomMock.Object);

        result[PlayerPosition.North].Should().HaveCount(3);
        result[PlayerPosition.East].Should().HaveCount(3);
        result[PlayerPosition.West].Should().HaveCount(3);
    }

    [Fact]
    public void DistributeHiddenCards_HandlesLeftBowerVoidCorrectly()
    {
        var myHand = new Card[]
        {
            new(Suit.Spades, Rank.Ace),
            new(Suit.Spades, Rank.King),
            new(Suit.Spades, Rank.Queen),
            new(Suit.Spades, Rank.Jack),
            new(Suit.Hearts, Rank.Ace),
        };

        Card[] accountedFor = [.. myHand];

        var voids = new List<PlayerSuitVoid>
        {
            new(PlayerPosition.East, Suit.Spades),
        };

        var result = _distributor.DistributeHiddenCards(
            accountedFor,
            PlayerPosition.South,
            5,
            Suit.Spades,
            voids,
            sittingOutPlayer: null,
            _randomMock.Object);

        var jackOfClubs = new Card(Suit.Clubs, Rank.Jack);
        result[PlayerPosition.East].Should().NotContain(jackOfClubs, "left bower of clubs is effectively trump (spades) and East is void in spades");
    }
}
