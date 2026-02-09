using FluentAssertions;

using Moq;

using NemesisEuchre.Console.Commands;
using NemesisEuchre.Console.Services;
using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.DataAccess.Mappers;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

using Spectre.Console.Testing;

namespace NemesisEuchre.Console.Tests.Commands;

public class ShowGameCommandTests
{
    [Fact]
    public async Task RunAsync_WhenGameNotFound_ReturnsOne()
    {
        var testConsole = new TestConsole();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockRepository = new Mock<IGameRepository>();
        var mockMapper = new Mock<IEntityToGameMapper>();
        var mockRenderer = new Mock<IGameResultsRenderer>();

        mockRepository
            .Setup(r => r.GetGameByIdAsync(42, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameEntity?)null);

        var command = new ShowGameCommand(testConsole, mockBanner, mockRepository.Object, mockMapper.Object, mockRenderer.Object)
        {
            GameId = 42,
            ShowDecisions = false,
        };

        var result = await command.RunAsync();

        result.Should().Be(1);
        testConsole.Output.Should().Contain("Game with ID 42 was not found");
    }

    [Fact]
    public async Task RunAsync_WhenGameFound_ReturnsZero()
    {
        var testConsole = new TestConsole();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockRepository = new Mock<IGameRepository>();
        var mockMapper = new Mock<IEntityToGameMapper>();
        var mockRenderer = new Mock<IGameResultsRenderer>();

        var gameEntity = new GameEntity
        {
            GameId = 1,
            GameStatusId = (int)GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeamId = (int)Team.Team1,
            GamePlayers = [],
            Deals = [],
        };
        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };

        mockRepository
            .Setup(r => r.GetGameByIdAsync(1, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameEntity);

        mockMapper
            .Setup(m => m.Map(gameEntity, false))
            .Returns(game);

        var command = new ShowGameCommand(testConsole, mockBanner, mockRepository.Object, mockMapper.Object, mockRenderer.Object)
        {
            GameId = 1,
            ShowDecisions = false,
        };

        var result = await command.RunAsync();

        result.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_WhenGameFound_CallsRenderResults()
    {
        var testConsole = new TestConsole();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockRepository = new Mock<IGameRepository>();
        var mockMapper = new Mock<IEntityToGameMapper>();
        var mockRenderer = new Mock<IGameResultsRenderer>();

        var gameEntity = new GameEntity
        {
            GameId = 1,
            GameStatusId = (int)GameStatus.Complete,
            GamePlayers = [],
            Deals = [],
        };
        var game = new Game { GameStatus = GameStatus.Complete };

        mockRepository
            .Setup(r => r.GetGameByIdAsync(1, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameEntity);

        mockMapper
            .Setup(m => m.Map(gameEntity, false))
            .Returns(game);

        var command = new ShowGameCommand(testConsole, mockBanner, mockRepository.Object, mockMapper.Object, mockRenderer.Object)
        {
            GameId = 1,
            ShowDecisions = false,
        };

        await command.RunAsync();

        mockRenderer.Verify(r => r.RenderResults(game, false), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithShowDecisions_ForwardsFlag()
    {
        var testConsole = new TestConsole();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockRepository = new Mock<IGameRepository>();
        var mockMapper = new Mock<IEntityToGameMapper>();
        var mockRenderer = new Mock<IGameResultsRenderer>();

        var gameEntity = new GameEntity
        {
            GameId = 5,
            GameStatusId = (int)GameStatus.Complete,
            GamePlayers = [],
            Deals = [],
        };
        var game = new Game { GameStatus = GameStatus.Complete };

        mockRepository
            .Setup(r => r.GetGameByIdAsync(5, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameEntity);

        mockMapper
            .Setup(m => m.Map(gameEntity, true))
            .Returns(game);

        var command = new ShowGameCommand(testConsole, mockBanner, mockRepository.Object, mockMapper.Object, mockRenderer.Object)
        {
            GameId = 5,
            ShowDecisions = true,
        };

        await command.RunAsync();

        mockRepository.Verify(r => r.GetGameByIdAsync(5, true, It.IsAny<CancellationToken>()), Times.Once);
        mockMapper.Verify(m => m.Map(gameEntity, true), Times.Once);
        mockRenderer.Verify(r => r.RenderResults(game, true), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenExecuted_DisplaysApplicationBanner()
    {
        var testConsole = new TestConsole();
        var mockBanner = new Mock<IApplicationBanner>();
        var mockRepository = new Mock<IGameRepository>();
        var mockMapper = new Mock<IEntityToGameMapper>();
        var mockRenderer = new Mock<IGameResultsRenderer>();

        mockRepository
            .Setup(r => r.GetGameByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameEntity?)null);

        var command = new ShowGameCommand(testConsole, mockBanner.Object, mockRepository.Object, mockMapper.Object, mockRenderer.Object)
        {
            GameId = 1,
        };

        await command.RunAsync();

        mockBanner.Verify(b => b.Display(), Times.Once);
    }
}
