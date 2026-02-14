using FluentAssertions;

using Moq;

using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.DataAccess.Models;
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
    public void Map_WithEmptyKnownPlayerSuitVoids_SetsCollectionEmpty()
    {
        var deal = CreateTestDeal();
        deal.KnownPlayerSuitVoids = [];

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, GameOutcomeContext.None);

        entity.DealKnownPlayerSuitVoids.Should().BeEmpty();
    }

    [Fact]
    public void Map_WithKnownPlayerSuitVoids_MapsCorrectly()
    {
        var deal = CreateTestDeal();
        deal.KnownPlayerSuitVoids =
        [
            new(PlayerPosition.North, Suit.Spades),
            new(PlayerPosition.East, Suit.Hearts),
        ];

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, GameOutcomeContext.None);

        entity.DealKnownPlayerSuitVoids.Should().HaveCount(2);
        entity.DealKnownPlayerSuitVoids.Should().Contain(v =>
            v.PlayerPositionId == (int)PlayerPosition.North && v.SuitId == (int)Suit.Spades);
        entity.DealKnownPlayerSuitVoids.Should().Contain(v =>
            v.PlayerPositionId == (int)PlayerPosition.East && v.SuitId == (int)Suit.Hearts);
    }

    [Fact]
    public void Map_KnownPlayerSuitVoids_PreservesAllEntries()
    {
        var deal = CreateTestDeal();
        deal.KnownPlayerSuitVoids =
        [
            new(PlayerPosition.South, Suit.Clubs),
            new(PlayerPosition.West, Suit.Diamonds),
            new(PlayerPosition.North, Suit.Hearts),
        ];

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, GameOutcomeContext.None);

        entity.DealKnownPlayerSuitVoids.Should().HaveCount(3);
        entity.DealKnownPlayerSuitVoids.Should().Contain(v =>
            v.PlayerPositionId == (int)PlayerPosition.South && v.SuitId == (int)Suit.Clubs);
        entity.DealKnownPlayerSuitVoids.Should().Contain(v =>
            v.PlayerPositionId == (int)PlayerPosition.West && v.SuitId == (int)Suit.Diamonds);
        entity.DealKnownPlayerSuitVoids.Should().Contain(v =>
            v.PlayerPositionId == (int)PlayerPosition.North && v.SuitId == (int)Suit.Hearts);
    }

    [Fact]
    public void Map_ShouldSerializeCallTrumpDecisionPredictedPoints()
    {
        var deal = CreateTestDeal();
        deal.CallTrumpDecisions.Add(new CallTrumpDecisionRecord
        {
            CardsInHand = [new Card(Suit.Hearts, Rank.Nine)],
            PlayerPosition = PlayerPosition.North,
            TeamScore = 0,
            OpponentScore = 0,
            DecisionOrder = 1,
            DealerPosition = PlayerPosition.West,
            UpCard = new Card(Suit.Hearts, Rank.Ace),
            ValidCallTrumpDecisions = [CallTrumpDecision.Pass, CallTrumpDecision.OrderItUp],
            ChosenDecision = CallTrumpDecision.OrderItUp,
            DecisionPredictedPoints = new Dictionary<CallTrumpDecision, float>
            {
                { CallTrumpDecision.Pass, 0.2f },
                { CallTrumpDecision.OrderItUp, 1.8f },
            },
        });

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, GameOutcomeContext.None);

        entity.CallTrumpDecisions.Should().HaveCount(1);
        var decision = entity.CallTrumpDecisions.First();
        decision.PredictedPoints.Should().HaveCount(2);
        decision.PredictedPoints.Should().Contain(p =>
            p.CallTrumpDecisionValueId == (int)CallTrumpDecision.Pass
            && Math.Abs(p.PredictedPoints - 0.2f) < 0.001f);
        decision.PredictedPoints.Should().Contain(p =>
            p.CallTrumpDecisionValueId == (int)CallTrumpDecision.OrderItUp
            && Math.Abs(p.PredictedPoints - 1.8f) < 0.001f);
        decision.CardsInHand.Should().NotBeEmpty();
        decision.ValidDecisions.Should().NotBeEmpty();
        decision.ChosenDecisionValueId.Should().Be((int)CallTrumpDecision.OrderItUp);
    }

    [Fact]
    public void Map_WithEmptyCallTrumpDecisionPredictedPoints_SetsCollectionEmpty()
    {
        var deal = CreateTestDeal();
        deal.CallTrumpDecisions.Add(new CallTrumpDecisionRecord
        {
            CardsInHand = [new Card(Suit.Hearts, Rank.Nine)],
            PlayerPosition = PlayerPosition.North,
            TeamScore = 0,
            OpponentScore = 0,
            DecisionOrder = 1,
            DealerPosition = PlayerPosition.West,
            UpCard = new Card(Suit.Hearts, Rank.Ace),
            ValidCallTrumpDecisions = [CallTrumpDecision.Pass],
            ChosenDecision = CallTrumpDecision.Pass,
            DecisionPredictedPoints = [],
        });

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, GameOutcomeContext.None);

        entity.CallTrumpDecisions.First().PredictedPoints.Should().BeEmpty();
    }

    [Fact]
    public void Map_ShouldSerializeDiscardCardDecisionPredictedPoints()
    {
        var deal = CreateTestDeal();
        var nineOfHearts = new Card(Suit.Hearts, Rank.Nine);
        var tenOfClubs = new Card(Suit.Clubs, Rank.Ten);
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
        var entity = _mapper.Map(deal, 1, gamePlayers, GameOutcomeContext.None);

        entity.DiscardCardDecisions.Should().HaveCount(1);
        var decision = entity.DiscardCardDecisions.First();
        decision.PredictedPoints.Should().HaveCount(2);
        decision.PredictedPoints.Should().Contain(p => Math.Abs(p.PredictedPoints - 1.2f) < 0.001f);
        decision.PredictedPoints.Should().Contain(p => Math.Abs(p.PredictedPoints - 0.5f) < 0.001f);
        decision.CardsInHand.Should().NotBeEmpty();
        decision.ChosenRelativeCardId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Map_WithEmptyDiscardCardDecisionPredictedPoints_SetsCollectionEmpty()
    {
        var deal = CreateTestDeal();
        deal.DiscardCardDecisions.Add(new DiscardCardDecisionRecord
        {
            CardsInHand = [new Card(Suit.Hearts, Rank.Nine)],
            PlayerPosition = PlayerPosition.North,
            TeamScore = 0,
            OpponentScore = 0,
            TrumpSuit = Suit.Hearts,
            CallingPlayer = PlayerPosition.North,
            CallingPlayerGoingAlone = false,
            ValidCardsToDiscard = [new Card(Suit.Hearts, Rank.Nine)],
            ChosenCard = new Card(Suit.Hearts, Rank.Nine),
            DecisionPredictedPoints = [],
        });

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, GameOutcomeContext.None);

        entity.DiscardCardDecisions.First().PredictedPoints.Should().BeEmpty();
    }

    [Fact]
    public void Map_ShouldMapDealDeckCards()
    {
        var deal = CreateTestDeal();
        deal.Deck.AddRange([new Card(Suit.Hearts, Rank.Nine), new Card(Suit.Spades, Rank.Ace)]);

        var gamePlayers = CreateTestGamePlayers();
        var entity = _mapper.Map(deal, 1, gamePlayers, GameOutcomeContext.None);

        entity.DealDeckCards.Should().HaveCount(2);
    }

    private static Deal CreateTestDeal()
    {
        return new Deal
        {
            DealNumber = 1,
            DealStatus = DealStatus.Complete,
            DealerPosition = PlayerPosition.North,
            Deck = [],
            UpCard = new Card(Suit.Hearts, Rank.Nine),
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
                    Actor = new Actor(ActorType.Chaos, null),
                    StartingHand = [],
                    CurrentHand = [],
                },
                [PlayerPosition.East] = new DealPlayer
                {
                    Position = PlayerPosition.East,
                    Actor = new Actor(ActorType.Chaos, null),
                    StartingHand = [],
                    CurrentHand = [],
                },
                [PlayerPosition.South] = new DealPlayer
                {
                    Position = PlayerPosition.South,
                    Actor = new Actor(ActorType.Chaos, null),
                    StartingHand = [],
                    CurrentHand = [],
                },
                [PlayerPosition.West] = new DealPlayer
                {
                    Position = PlayerPosition.West,
                    Actor = new Actor(ActorType.Chaos, null),
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
                Actor = new Actor(ActorType.Chaos, null),
            },
            [PlayerPosition.East] = new Player
            {
                Position = PlayerPosition.East,
                Actor = new Actor(ActorType.Chaos, null),
            },
            [PlayerPosition.South] = new Player
            {
                Position = PlayerPosition.South,
                Actor = new Actor(ActorType.Chaos, null),
            },
            [PlayerPosition.West] = new Player
            {
                Position = PlayerPosition.West,
                Actor = new Actor(ActorType.Chaos, null),
            },
        };
    }
}
