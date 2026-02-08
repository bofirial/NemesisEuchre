using System.Text.Json;

using FluentAssertions;

using Moq;

using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.DataAccess.Tests.Mappers;

public class DealToEntityMapperTests
{
    private readonly Mock<ITrickToEntityMapper> _mockTrickMapper;
    private readonly DealToEntityMapper _mapper;

    public DealToEntityMapperTests()
    {
        _mockTrickMapper = new Mock<ITrickToEntityMapper>();
        _mapper = new DealToEntityMapper(_mockTrickMapper.Object);
    }

    [Fact]
    public void Map_WithEmptyKnownPlayerSuitVoids_SetsJsonToNull()
    {
        var deal = CreateTestDeal();
        deal.KnownPlayerSuitVoids = [];

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, false, false);

        entity.KnownPlayerSuitVoidsJson.Should().BeNull();
    }

    [Fact]
    public void Map_WithKnownPlayerSuitVoids_SerializesCorrectly()
    {
        var deal = CreateTestDeal();
        deal.KnownPlayerSuitVoids =
        [
            new(PlayerPosition.North, Suit.Spades),
            new(PlayerPosition.East, Suit.Hearts),
        ];

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, false, false);

        entity.KnownPlayerSuitVoidsJson.Should().NotBeNullOrEmpty();

        var json = entity.KnownPlayerSuitVoidsJson!;
        json.Should().Contain("North");
        json.Should().Contain("Spades");
        json.Should().Contain("East");
        json.Should().Contain("Hearts");
    }

    [Fact]
    public void Map_KnownPlayerSuitVoidsRoundtrip_PreservesData()
    {
        var deal = CreateTestDeal();
        deal.KnownPlayerSuitVoids =
        [
            new(PlayerPosition.South, Suit.Clubs),
            new(PlayerPosition.West, Suit.Diamonds),
            new(PlayerPosition.North, Suit.Hearts),
        ];

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, false, false);

        var json = entity.KnownPlayerSuitVoidsJson!;
        json.Should().Contain("South");
        json.Should().Contain("Clubs");
        json.Should().Contain("West");
        json.Should().Contain("Diamonds");
        json.Should().Contain("North");
        json.Should().Contain("Hearts");
    }

    [Fact]
    public void Map_ShouldSerializeCallTrumpDecisionPredictedPoints()
    {
        var deal = CreateTestDeal();
        deal.CallTrumpDecisions.Add(new CallTrumpDecisionRecord
        {
            CardsInHand = [new Card { Suit = Suit.Hearts, Rank = Rank.Nine }],
            PlayerPosition = PlayerPosition.North,
            TeamScore = 0,
            OpponentScore = 0,
            DecisionOrder = 1,
            DealerPosition = PlayerPosition.West,
            UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
            ValidCallTrumpDecisions = [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp],
            ChosenDecision = CallTrumpDecision.OrderItUp,
            DecisionPredictedPoints = new Dictionary<CallTrumpDecision, float>
            {
                { CallTrumpDecision.Pass, 0.2f },
                { CallTrumpDecision.OrderItUp, 1.8f },
            },
        });

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, false, false);

        entity.CallTrumpDecisions.Should().HaveCount(1);
        var decision = entity.CallTrumpDecisions.First();
        decision.DecisionPredictedPointsJson.Should().NotBeNull();

        var deserialized = JsonSerializer.Deserialize<Dictionary<CallTrumpDecision, float>>(
            decision.DecisionPredictedPointsJson!, JsonSerializationOptions.Default);
        deserialized.Should().HaveCount(2);
        deserialized![CallTrumpDecision.Pass].Should().BeApproximately(0.2f, 0.001f);
        deserialized[CallTrumpDecision.OrderItUp].Should().BeApproximately(1.8f, 0.001f);
    }

    [Fact]
    public void Map_WithEmptyCallTrumpDecisionPredictedPoints_SetsJsonToNull()
    {
        var deal = CreateTestDeal();
        deal.CallTrumpDecisions.Add(new CallTrumpDecisionRecord
        {
            CardsInHand = [new Card { Suit = Suit.Hearts, Rank = Rank.Nine }],
            PlayerPosition = PlayerPosition.North,
            TeamScore = 0,
            OpponentScore = 0,
            DecisionOrder = 1,
            DealerPosition = PlayerPosition.West,
            UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Ace },
            ValidCallTrumpDecisions = [CallTrumpDecision.Pass],
            ChosenDecision = CallTrumpDecision.Pass,
            DecisionPredictedPoints = [],
        });

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, false, false);

        entity.CallTrumpDecisions.First().DecisionPredictedPointsJson.Should().BeNull();
    }

    [Fact]
    public void Map_ShouldSerializeDiscardCardDecisionPredictedPoints()
    {
        var deal = CreateTestDeal();
        var nineOfHearts = new Card { Suit = Suit.Hearts, Rank = Rank.Nine };
        var tenOfClubs = new Card { Suit = Suit.Clubs, Rank = Rank.Ten };
        deal.DiscardCardDecisions.Add(new DiscardCardDecisionRecord
        {
            CardsInHand = [nineOfHearts, tenOfClubs],
            PlayerPosition = PlayerPosition.North,
            TeamScore = 0,
            OpponentScore = 0,
            TrumpSuit = Suit.Hearts,
            CallingPlayer = PlayerPosition.North,
            CallingPlayerGoingAlone = false,
            ValidCardsToDiscard = [nineOfHearts, tenOfClubs],
            ChosenCard = tenOfClubs,
            DecisionPredictedPoints = new Dictionary<Card, float>
            {
                { nineOfHearts, 1.2f },
                { tenOfClubs, 0.5f },
            },
        });

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, false, false);

        entity.DiscardCardDecisions.Should().HaveCount(1);
        var decision = entity.DiscardCardDecisions.First();
        decision.DecisionPredictedPointsJson.Should().NotBeNull();

        var items = JsonSerializer.Deserialize<List<CardPointsPair>>(
            decision.DecisionPredictedPointsJson!, JsonSerializationOptions.Default);
        items.Should().HaveCount(2);
        items.Should().Contain(i => Math.Abs(i.Points - 1.2f) < 0.001f);
        items.Should().Contain(i => Math.Abs(i.Points - 0.5f) < 0.001f);
    }

    [Fact]
    public void Map_WithEmptyDiscardCardDecisionPredictedPoints_SetsJsonToNull()
    {
        var deal = CreateTestDeal();
        deal.DiscardCardDecisions.Add(new DiscardCardDecisionRecord
        {
            CardsInHand = [new Card { Suit = Suit.Hearts, Rank = Rank.Nine }],
            PlayerPosition = PlayerPosition.North,
            TeamScore = 0,
            OpponentScore = 0,
            TrumpSuit = Suit.Hearts,
            CallingPlayer = PlayerPosition.North,
            CallingPlayerGoingAlone = false,
            ValidCardsToDiscard = [new Card { Suit = Suit.Hearts, Rank = Rank.Nine }],
            ChosenCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
            DecisionPredictedPoints = [],
        });

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, false, false);

        entity.DiscardCardDecisions.First().DecisionPredictedPointsJson.Should().BeNull();
    }

    private static Deal CreateTestDeal()
    {
        return new Deal
        {
            DealNumber = 1,
            DealStatus = DealStatus.Complete,
            DealerPosition = PlayerPosition.North,
            Deck = [],
            UpCard = new Card { Suit = Suit.Hearts, Rank = Rank.Nine },
            Trump = Suit.Hearts,
            CallingPlayer = PlayerPosition.North,
            CallingPlayerIsGoingAlone = false,
            ChosenDecision = CallTrumpDecision.OrderItUp,
            DealResult = DealResult.WonStandardBid,
            WinningTeam = Team.Team1,
            Team1Score = 1,
            Team2Score = 0,
            Players = new Dictionary<PlayerPosition, DealPlayer>
            {
                [PlayerPosition.North] = new DealPlayer
                {
                    Position = PlayerPosition.North,
                    ActorType = ActorType.Chaos,
                    StartingHand = [],
                    CurrentHand = [],
                },
                [PlayerPosition.East] = new DealPlayer
                {
                    Position = PlayerPosition.East,
                    ActorType = ActorType.Chaos,
                    StartingHand = [],
                    CurrentHand = [],
                },
                [PlayerPosition.South] = new DealPlayer
                {
                    Position = PlayerPosition.South,
                    ActorType = ActorType.Chaos,
                    StartingHand = [],
                    CurrentHand = [],
                },
                [PlayerPosition.West] = new DealPlayer
                {
                    Position = PlayerPosition.West,
                    ActorType = ActorType.Chaos,
                    StartingHand = [],
                    CurrentHand = [],
                },
            },
        };
    }

    private static Dictionary<PlayerPosition, Player> CreateTestGamePlayers()
    {
        return new Dictionary<PlayerPosition, Player>
        {
            [PlayerPosition.North] = new Player
            {
                Position = PlayerPosition.North,
                ActorType = ActorType.Chaos,
            },
            [PlayerPosition.East] = new Player
            {
                Position = PlayerPosition.East,
                ActorType = ActorType.Chaos,
            },
            [PlayerPosition.South] = new Player
            {
                Position = PlayerPosition.South,
                ActorType = ActorType.Chaos,
            },
            [PlayerPosition.West] = new Player
            {
                Position = PlayerPosition.West,
                ActorType = ActorType.Chaos,
            },
        };
    }

#pragma warning disable SA1313, CA1852
    private record CardPointsPair(RelativeCard Card, float Points);
#pragma warning restore SA1313, CA1852
}
