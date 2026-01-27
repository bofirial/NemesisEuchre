using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Tests.Repositories;

public class GameRepositoryTests
{
    [Fact]
    public async Task SaveCompleteGameAsync_ShouldSaveGame()
    {
        var options = new DbContextOptionsBuilder<NemesisEuchreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockMapper = new Mock<IGameToEntityMapper>();
        mockMapper.Setup(m => m.Map(It.IsAny<Game>()))
            .Returns(new GameEntity
            {
                GameStatus = GameStatus.Complete,
                PlayersJson = "{}",
                Team1Score = 10,
                Team2Score = 0,
                WinningTeam = Team.Team1,
                CreatedAt = DateTime.UtcNow,
            });

        await using var context = new NemesisEuchreDbContext(options);
        var repository = new GameRepository(context, mockMapper.Object);

        var game = new Game { GameStatus = GameStatus.Complete };
        var gameId = await repository.SaveCompletedGameAsync(game);

        gameId.Should().BeGreaterThan(0);
        var savedGame = await context.Games!.FindAsync(gameId);
        savedGame.Should().NotBeNull();
        savedGame!.GameStatus.Should().Be(GameStatus.Complete);
    }
}
