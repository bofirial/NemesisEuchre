using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation.Constants;
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

        var mockLogger = new Mock<ILogger<GameRepository>>();
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

        var mockOptions = new Mock<IOptions<PersistenceOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new PersistenceOptions());

        await using var context = new NemesisEuchreDbContext(options);
        var repository = new GameRepository(context, mockLogger.Object, mockMapper.Object, mockOptions.Object);

        var game = new Game { GameStatus = GameStatus.Complete };
        await repository.SaveCompletedGameAsync(game, TestContext.Current.CancellationToken);

        var savedGame = await context.Games!.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        savedGame.Should().NotBeNull();
        savedGame!.GameStatus.Should().Be(GameStatus.Complete);
    }
}
