using FluentAssertions;

using Moq;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.DataAccess.Tests.Mappers;

public class EntityToDealMapperTests
{
    private readonly Mock<IEntityToTrickMapper> _mockTrickMapper;
    private readonly EntityToDealMapper _mapper;

    public EntityToDealMapperTests()
    {
        _mockTrickMapper = new Mock<IEntityToTrickMapper>();
        _mapper = new EntityToDealMapper(_mockTrickMapper.Object);
    }

    [Fact]
    public void Map_WithBasicDealEntity_MapsCoreFields()
    {
        var entity = CreateTestDealEntity();
        var players = CreateTestGamePlayers();

        var deal = _mapper.Map(entity, players, includeDecisions: false);

        deal.DealNumber.Should().Be(1);
        deal.DealStatus.Should().Be(DealStatus.Complete);
        deal.DealerPosition.Should().Be(PlayerPosition.North);
        deal.Trump.Should().Be(Suit.Hearts);
        deal.CallingPlayer.Should().Be(PlayerPosition.East);
        deal.CallingPlayerIsGoingAlone.Should().BeFalse();
        deal.ChosenDecision.Should().Be(CallTrumpDecision.OrderItUp);
        deal.DealResult.Should().Be(DealResult.WonStandardBid);
        deal.WinningTeam.Should().Be(Team.Team1);
        deal.Team1Score.Should().Be(1);
        deal.Team2Score.Should().Be(0);
    }

    [Fact]
    public void Map_WithUpCard_MapsCardCorrectly()
    {
        var entity = CreateTestDealEntity();
        entity.UpCardId = CardIdHelper.ToCardId(new Card(Suit.Hearts, Rank.Ace));
        var players = CreateTestGamePlayers();

        var deal = _mapper.Map(entity, players, includeDecisions: false);

        deal.UpCard.Should().Be(new Card(Suit.Hearts, Rank.Ace));
    }

    [Fact]
    public void Map_WithDiscardedCard_MapsCardCorrectly()
    {
        var entity = CreateTestDealEntity();
        entity.DiscardedCardId = CardIdHelper.ToCardId(new Card(Suit.Clubs, Rank.Nine));
        var players = CreateTestGamePlayers();

        var deal = _mapper.Map(entity, players, includeDecisions: false);

        deal.DiscardedCard.Should().Be(new Card(Suit.Clubs, Rank.Nine));
    }

    [Fact]
    public void Map_WithDealPlayers_MapsStartingHands()
    {
        var entity = CreateTestDealEntity();
        entity.DealPlayers =
        [
            new DealPlayerEntity
            {
                PlayerPositionId = (int)PlayerPosition.North,
                ActorTypeId = (int)ActorType.Chaos,
                StartingHandCards =
                [
                    new DealPlayerStartingHandCard { CardId = CardIdHelper.ToCardId(new Card(Suit.Hearts, Rank.Ace)), SortOrder = 0 },
                    new DealPlayerStartingHandCard { CardId = CardIdHelper.ToCardId(new Card(Suit.Spades, Rank.King)), SortOrder = 1 },
                ],
            },
        ];
        var players = CreateTestGamePlayers();

        var deal = _mapper.Map(entity, players, includeDecisions: false);

        deal.Players.Should().ContainKey(PlayerPosition.North);
        deal.Players[PlayerPosition.North].StartingHand.Should().HaveCount(2);
        deal.Players[PlayerPosition.North].StartingHand[0].Should().Be(new Card(Suit.Hearts, Rank.Ace));
        deal.Players[PlayerPosition.North].StartingHand[1].Should().Be(new Card(Suit.Spades, Rank.King));
    }

    [Fact]
    public void Map_WithDeckCards_MapsInSortOrder()
    {
        var entity = CreateTestDealEntity();
        entity.DealDeckCards =
        [
            new DealDeckCard { CardId = CardIdHelper.ToCardId(new Card(Suit.Clubs, Rank.Ten)), SortOrder = 1 },
            new DealDeckCard { CardId = CardIdHelper.ToCardId(new Card(Suit.Hearts, Rank.Nine)), SortOrder = 0 },
        ];
        var players = CreateTestGamePlayers();

        var deal = _mapper.Map(entity, players, includeDecisions: false);

        deal.Deck.Should().HaveCount(2);
        deal.Deck[0].Should().Be(new Card(Suit.Hearts, Rank.Nine));
        deal.Deck[1].Should().Be(new Card(Suit.Clubs, Rank.Ten));
    }

    [Fact]
    public void Map_WithKnownVoids_MapsCorrectly()
    {
        var entity = CreateTestDealEntity();
        entity.DealKnownPlayerSuitVoids =
        [
            new DealKnownPlayerSuitVoid { PlayerPositionId = (int)PlayerPosition.East, SuitId = (int)Suit.Spades },
        ];
        var players = CreateTestGamePlayers();

        var deal = _mapper.Map(entity, players, includeDecisions: false);

        deal.KnownPlayerSuitVoids.Should().HaveCount(1);
        deal.KnownPlayerSuitVoids[0].PlayerPosition.Should().Be(PlayerPosition.East);
        deal.KnownPlayerSuitVoids[0].Suit.Should().Be(Suit.Spades);
    }

    [Fact]
    public void Map_WithTricks_DelegatesMapping()
    {
        var entity = CreateTestDealEntity();
        entity.Tricks =
        [
            new TrickEntity { TrickNumber = 1, LeadPlayerPositionId = (int)PlayerPosition.East, TrickCardsPlayed = [], PlayCardDecisions = [] },
            new TrickEntity { TrickNumber = 2, LeadPlayerPositionId = (int)PlayerPosition.South, TrickCardsPlayed = [], PlayCardDecisions = [] },
        ];
        var players = CreateTestGamePlayers();

        _mockTrickMapper
            .Setup(m => m.Map(It.IsAny<TrickEntity>(), It.IsAny<Suit>(), It.IsAny<PlayerPosition>(), false))
            .Returns(new Trick { TrickNumber = 1 });

        var deal = _mapper.Map(entity, players, includeDecisions: false);

        deal.CompletedTricks.Should().HaveCount(2);
        _mockTrickMapper.Verify(m => m.Map(It.IsAny<TrickEntity>(), Suit.Hearts, PlayerPosition.North, false), Times.Exactly(2));
    }

    [Fact]
    public void Map_WithCallTrumpDecisions_MapsWhenIncluded()
    {
        var entity = CreateTestDealEntity();
        entity.CallTrumpDecisions =
        [
            new CallTrumpDecisionEntity
            {
                CallTrumpDecisionId = 1,
                DealId = 1,
                DealerRelativePositionId = (int)RelativePlayerPosition.Self,
                UpCardId = CardIdHelper.ToCardId(new Card(Suit.Hearts, Rank.Ace)),
                TeamScore = 0,
                OpponentScore = 0,
                ChosenDecisionValueId = (int)CallTrumpDecision.OrderItUp,
                DecisionOrder = 1,
                CardsInHand = [new CallTrumpDecisionCardsInHand { CardId = CardIdHelper.ToCardId(new Card(Suit.Hearts, Rank.King)), SortOrder = 0 }],
                ValidDecisions = [new CallTrumpDecisionValidDecision { CallTrumpDecisionValueId = (int)CallTrumpDecision.OrderItUp }],
                PredictedPoints = [new CallTrumpDecisionPredictedPoints { CallTrumpDecisionValueId = (int)CallTrumpDecision.OrderItUp, PredictedPoints = 1.5f }],
            },
        ];
        var players = CreateTestGamePlayers();

        var deal = _mapper.Map(entity, players, includeDecisions: true);

        deal.CallTrumpDecisions.Should().HaveCount(1);
        var decision = deal.CallTrumpDecisions[0];
        decision.PlayerPosition.Should().Be(PlayerPosition.North);
        decision.DealerPosition.Should().Be(PlayerPosition.North);
        decision.ChosenDecision.Should().Be(CallTrumpDecision.OrderItUp);
        decision.CardsInHand.Should().HaveCount(1);
        decision.ValidCallTrumpDecisions.Should().HaveCount(1);
        decision.DecisionPredictedPoints.Should().HaveCount(1);
    }

    [Fact]
    public void Map_WithDiscardCardDecisions_MapsWhenIncluded()
    {
        var entity = CreateTestDealEntity();
        const Suit trump = Suit.Hearts;
        var chosenRelativeCardId = CardIdHelper.ToRelativeCardId(
            new RelativeCard(Rank.Nine, RelativeSuit.NonTrumpSameColor));

        entity.DiscardCardDecisions =
        [
            new DiscardCardDecisionEntity
            {
                DiscardCardDecisionId = 1,
                DealId = 1,
                CallingRelativePlayerPositionId = (int)RelativePlayerPosition.RightHandOpponent,
                CallingPlayerGoingAlone = false,
                TeamScore = 0,
                OpponentScore = 0,
                ChosenRelativeCardId = chosenRelativeCardId,
                CardsInHand = [],
                PredictedPoints = [],
            },
        ];
        var players = CreateTestGamePlayers();

        var deal = _mapper.Map(entity, players, includeDecisions: true);

        deal.DiscardCardDecisions.Should().HaveCount(1);
        var decision = deal.DiscardCardDecisions[0];
        decision.TrumpSuit.Should().Be(trump);
        decision.CallingPlayer.Should().Be(PlayerPosition.East);
    }

    [Fact]
    public void Map_WithoutDecisions_LeavesDecisionCollectionsEmpty()
    {
        var entity = CreateTestDealEntity();
        entity.CallTrumpDecisions =
        [
            new CallTrumpDecisionEntity
            {
                DealerRelativePositionId = (int)RelativePlayerPosition.Self,
                UpCardId = CardIdHelper.ToCardId(new Card(Suit.Hearts, Rank.Ace)),
                ChosenDecisionValueId = (int)CallTrumpDecision.Pass,
                DecisionOrder = 1,
                CardsInHand = [],
                ValidDecisions = [],
                PredictedPoints = [],
            },
        ];
        var players = CreateTestGamePlayers();

        var deal = _mapper.Map(entity, players, includeDecisions: false);

        deal.CallTrumpDecisions.Should().BeEmpty();
        deal.DiscardCardDecisions.Should().BeEmpty();
    }

    [Fact]
    public void Map_WithNullOptionalFields_MapsToNull()
    {
        var entity = CreateTestDealEntity();
        entity.UpCardId = null;
        entity.DiscardedCardId = null;
        entity.TrumpSuitId = null;
        entity.CallingPlayerPositionId = null;
        entity.DealerPositionId = null;
        entity.DealResultId = null;
        entity.WinningTeamId = null;
        entity.ChosenCallTrumpDecisionId = null;
        var players = CreateTestGamePlayers();

        var deal = _mapper.Map(entity, players, includeDecisions: false);

        deal.UpCard.Should().BeNull();
        deal.DiscardedCard.Should().BeNull();
        deal.Trump.Should().BeNull();
        deal.CallingPlayer.Should().BeNull();
        deal.DealerPosition.Should().BeNull();
        deal.DealResult.Should().BeNull();
        deal.WinningTeam.Should().BeNull();
        deal.ChosenDecision.Should().BeNull();
    }

    private static DealEntity CreateTestDealEntity()
    {
        return new DealEntity
        {
            DealId = 1,
            GameId = 1,
            DealNumber = 1,
            DealStatusId = (int)DealStatus.Complete,
            DealerPositionId = (int)PlayerPosition.North,
            UpCardId = CardIdHelper.ToCardId(new Card(Suit.Hearts, Rank.Ace)),
            TrumpSuitId = (int)Suit.Hearts,
            CallingPlayerPositionId = (int)PlayerPosition.East,
            CallingPlayerIsGoingAlone = false,
            ChosenCallTrumpDecisionId = (int)CallTrumpDecision.OrderItUp,
            DealResultId = (int)DealResult.WonStandardBid,
            WinningTeamId = (int)Team.Team1,
            Team1Score = 1,
            Team2Score = 0,
            DealDeckCards = [],
            DealPlayers = [],
            DealKnownPlayerSuitVoids = [],
            Tricks = [],
            CallTrumpDecisions = [],
            DiscardCardDecisions = [],
            PlayCardDecisions = [],
        };
    }

    private static Dictionary<PlayerPosition, Player> CreateTestGamePlayers()
    {
        return new Dictionary<PlayerPosition, Player>
        {
            [PlayerPosition.North] = new Player { Position = PlayerPosition.North, ActorType = ActorType.Chaos },
            [PlayerPosition.East] = new Player { Position = PlayerPosition.East, ActorType = ActorType.Chaos },
            [PlayerPosition.South] = new Player { Position = PlayerPosition.South, ActorType = ActorType.Chaos },
            [PlayerPosition.West] = new Player { Position = PlayerPosition.West, ActorType = ActorType.Chaos },
        };
    }
}
