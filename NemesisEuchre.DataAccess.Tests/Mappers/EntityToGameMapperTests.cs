using FluentAssertions;

using Moq;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Tests.Mappers;

public class EntityToGameMapperTests
{
    private readonly Mock<IEntityToDealMapper> _mockDealMapper;
    private readonly EntityToGameMapper _mapper;

    public EntityToGameMapperTests()
    {
        _mockDealMapper = new Mock<IEntityToDealMapper>();
        _mapper = new EntityToGameMapper(_mockDealMapper.Object);
    }

    [Fact]
    public void Map_WithBasicGameEntity_MapsCoreFields()
    {
        var entity = CreateTestGameEntity();

        var game = _mapper.Map(entity, includeDecisions: false);

        game.GameStatus.Should().Be(GameStatus.Complete);
        game.Team1Score.Should().Be(10);
        game.Team2Score.Should().Be(7);
        game.WinningTeam.Should().Be(Team.Team1);
    }

    [Fact]
    public void Map_WithGamePlayers_MapsAllPositions()
    {
        var entity = CreateTestGameEntity();

        var game = _mapper.Map(entity, includeDecisions: false);

        game.Players.Should().HaveCount(4);
        game.Players.Should().ContainKey(PlayerPosition.North);
        game.Players.Should().ContainKey(PlayerPosition.East);
        game.Players.Should().ContainKey(PlayerPosition.South);
        game.Players.Should().ContainKey(PlayerPosition.West);
        game.Players[PlayerPosition.North].Actor.ActorType.Should().Be(ActorType.Chaos);
    }

    [Fact]
    public void Map_WithDeals_DelegatesMappingInOrder()
    {
        var entity = CreateTestGameEntity();
        entity.Deals =
        [
            new DealEntity { DealId = 2, DealNumber = 2, DealStatusId = (int)DealStatus.Complete, Tricks = [], DealDeckCards = [], DealPlayers = [], DealKnownPlayerSuitVoids = [], CallTrumpDecisions = [], DiscardCardDecisions = [], PlayCardDecisions = [] },
            new DealEntity { DealId = 1, DealNumber = 1, DealStatusId = (int)DealStatus.Complete, Tricks = [], DealDeckCards = [], DealPlayers = [], DealKnownPlayerSuitVoids = [], CallTrumpDecisions = [], DiscardCardDecisions = [], PlayCardDecisions = [] },
        ];

        _mockDealMapper
            .Setup(m => m.Map(It.IsAny<DealEntity>(), It.IsAny<Dictionary<PlayerPosition, Player>>(), false))
            .Returns((DealEntity e, Dictionary<PlayerPosition, Player> _, bool _) => new Deal { DealNumber = (short)e.DealNumber });

        var game = _mapper.Map(entity, includeDecisions: false);

        game.CompletedDeals.Should().HaveCount(2);
        game.CompletedDeals[0].DealNumber.Should().Be(1);
        game.CompletedDeals[1].DealNumber.Should().Be(2);
    }

    [Fact]
    public void Map_WithIncludeDecisions_PassesFlagToDealMapper()
    {
        var entity = CreateTestGameEntity();
        entity.Deals =
        [
            new DealEntity { DealNumber = 1, DealStatusId = (int)DealStatus.Complete, Tricks = [], DealDeckCards = [], DealPlayers = [], DealKnownPlayerSuitVoids = [], CallTrumpDecisions = [], DiscardCardDecisions = [], PlayCardDecisions = [] },
        ];

        _mockDealMapper
            .Setup(m => m.Map(It.IsAny<DealEntity>(), It.IsAny<Dictionary<PlayerPosition, Player>>(), true))
            .Returns(new Deal { DealNumber = 1 });

        _mapper.Map(entity, includeDecisions: true);

        _mockDealMapper.Verify(m => m.Map(It.IsAny<DealEntity>(), It.IsAny<Dictionary<PlayerPosition, Player>>(), true), Times.Once);
    }

    [Fact]
    public void Map_WithNullWinningTeam_MapsToNull()
    {
        var entity = CreateTestGameEntity();
        entity.WinningTeamId = null;

        var game = _mapper.Map(entity, includeDecisions: false);

        game.WinningTeam.Should().BeNull();
    }

    [Fact]
    public void Map_WithPlayerActorType_MapsCorrectly()
    {
        var entity = CreateTestGameEntity();
        entity.GamePlayers = [.. entity.GamePlayers];
        foreach (var gp in entity.GamePlayers)
        {
            if (gp.PlayerPositionId == (int)PlayerPosition.North)
            {
                gp.ActorTypeId = (int)ActorType.Gen1;
            }
        }

        var game = _mapper.Map(entity, includeDecisions: false);

        game.Players[PlayerPosition.North].Actor.ActorType.Should().Be(ActorType.Gen1);
    }

    [Fact]
    public void Map_WithNullActorType_DefaultsToZeroEnumValue()
    {
        var entity = CreateTestGameEntity();
        entity.GamePlayers =
        [
            new GamePlayer { PlayerPositionId = (int)PlayerPosition.North, ActorTypeId = 0 },
            new GamePlayer { PlayerPositionId = (int)PlayerPosition.East, ActorTypeId = 0 },
            new GamePlayer { PlayerPositionId = (int)PlayerPosition.South, ActorTypeId = 0 },
            new GamePlayer { PlayerPositionId = (int)PlayerPosition.West, ActorTypeId = 0 },
        ];

        var game = _mapper.Map(entity, includeDecisions: false);

        game.Players[PlayerPosition.North].Actor.ActorType.Should().Be(ActorType.User);
    }

    private static GameEntity CreateTestGameEntity()
    {
        return new GameEntity
        {
            GameId = 1,
            GameStatusId = (int)GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeamId = (int)Team.Team1,
            CreatedAt = DateTime.UtcNow,
            GamePlayers =
            [
                new GamePlayer { PlayerPositionId = (int)PlayerPosition.North, ActorTypeId = (int)ActorType.Chaos },
                new GamePlayer { PlayerPositionId = (int)PlayerPosition.East, ActorTypeId = (int)ActorType.Chaos },
                new GamePlayer { PlayerPositionId = (int)PlayerPosition.South, ActorTypeId = (int)ActorType.Chaos },
                new GamePlayer { PlayerPositionId = (int)PlayerPosition.West, ActorTypeId = (int)ActorType.Chaos },
            ],
            Deals = [],
        };
    }
}
