using FluentAssertions;

using Moq;

using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Tests.Mappers;

public class GameToEntityMapperTests
{
    [Fact]
    public void Map_ShouldMapGameProperties()
    {
        var mockDealMapper = new Mock<IDealToEntityMapper>();
        var mapper = new GameToEntityMapper(mockDealMapper.Object);

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };

        game.Players.Add(PlayerPosition.North, new Player { Position = PlayerPosition.North, Actor = new Actor(ActorType.Chaos, null) });

        var entity = mapper.Map(game);

        entity.GameStatusId.Should().Be((int)GameStatus.Complete);
        entity.Team1Score.Should().Be(10);
        entity.Team2Score.Should().Be(7);
        entity.WinningTeamId.Should().Be((int)Team.Team1);
        entity.GamePlayers.Should().HaveCount(1);
        entity.GamePlayers.First().PlayerPositionId.Should().Be((int)PlayerPosition.North);
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
