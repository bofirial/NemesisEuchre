using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Services;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Tests.Services;

public class SingleGameRunnerTests
{
    [Fact]
    public async Task RunAsync_ExecutesGameOrchestrator()
    {
        var mockOrchestrator = new Mock<IGameOrchestrator>();
        var mockRepository = new Mock<IGameRepository>();
        var mockRenderer = new Mock<IGameResultsRenderer>();
        var mockLogger = Mock.Of<ILogger<SingleGameRunner>>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var runner = new SingleGameRunner(mockOrchestrator.Object, mockRepository.Object, mockRenderer.Object, mockLogger);

        await runner.RunAsync(cancellationToken: TestContext.Current.CancellationToken);

        mockOrchestrator.Verify(o => o.OrchestrateGameAsync(), Times.Once);
    }

    [Fact]
    public async Task RunAsync_PersistsCompletedGame()
    {
        var mockOrchestrator = new Mock<IGameOrchestrator>();
        var mockRepository = new Mock<IGameRepository>();
        var mockRenderer = new Mock<IGameResultsRenderer>();
        var mockLogger = Mock.Of<ILogger<SingleGameRunner>>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var runner = new SingleGameRunner(mockOrchestrator.Object, mockRepository.Object, mockRenderer.Object, mockLogger);

        await runner.RunAsync(cancellationToken: TestContext.Current.CancellationToken);

        mockRepository.Verify(
            r => r.SaveCompletedGameAsync(
                It.Is<Game>(g => g.GameStatus == GameStatus.Complete && g.WinningTeam == Team.Team1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_RendersGameResults()
    {
        var mockOrchestrator = new Mock<IGameOrchestrator>();
        var mockRepository = new Mock<IGameRepository>();
        var mockRenderer = new Mock<IGameResultsRenderer>();
        var mockLogger = Mock.Of<ILogger<SingleGameRunner>>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var runner = new SingleGameRunner(mockOrchestrator.Object, mockRepository.Object, mockRenderer.Object, mockLogger);

        await runner.RunAsync(cancellationToken: TestContext.Current.CancellationToken);

        mockRenderer.Verify(r => r.RenderResults(game, false), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ReturnsCompletedGame()
    {
        var mockOrchestrator = new Mock<IGameOrchestrator>();
        var mockRepository = new Mock<IGameRepository>();
        var mockRenderer = new Mock<IGameResultsRenderer>();
        var mockLogger = Mock.Of<ILogger<SingleGameRunner>>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var runner = new SingleGameRunner(mockOrchestrator.Object, mockRepository.Object, mockRenderer.Object, mockLogger);

        var result = await runner.RunAsync(cancellationToken: TestContext.Current.CancellationToken);

        result.Should().BeSameAs(game);
    }

    [Fact]
    public async Task RunAsync_WhenPersistenceFails_StillRendersResults()
    {
        var mockOrchestrator = new Mock<IGameOrchestrator>();
        var mockRepository = new Mock<IGameRepository>();
        var mockRenderer = new Mock<IGameResultsRenderer>();
        var mockLogger = new Mock<ILogger<SingleGameRunner>>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var runner = new SingleGameRunner(mockOrchestrator.Object, mockRepository.Object, mockRenderer.Object, mockLogger.Object);

        await runner.RunAsync(cancellationToken: TestContext.Current.CancellationToken);

        mockRenderer.Verify(r => r.RenderResults(game, false), Times.Once, "Results should be rendered even when persistence fails");
    }

    [Fact]
    public async Task RunAsync_WhenPersistenceFails_LogsError()
    {
        var mockOrchestrator = new Mock<IGameOrchestrator>();
        var mockRepository = new Mock<IGameRepository>();
        var mockRenderer = new Mock<IGameResultsRenderer>();
        var mockLogger = new Mock<ILogger<SingleGameRunner>>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        var expectedException = new InvalidOperationException("Database connection failed");
        mockRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var runner = new SingleGameRunner(mockOrchestrator.Object, mockRepository.Object, mockRenderer.Object, mockLogger.Object);

        await runner.RunAsync(cancellationToken: TestContext.Current.CancellationToken);

        mockLogger.Verify(
            x => x.IsEnabled(LogLevel.Error),
            Times.AtLeastOnce(),
            "Error logging should be attempted when persistence fails");
    }

    [Fact]
    public async Task RunAsync_WhenPersistenceFails_ReturnsGame()
    {
        var mockOrchestrator = new Mock<IGameOrchestrator>();
        var mockRepository = new Mock<IGameRepository>();
        var mockRenderer = new Mock<IGameResultsRenderer>();
        var mockLogger = Mock.Of<ILogger<SingleGameRunner>>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var runner = new SingleGameRunner(mockOrchestrator.Object, mockRepository.Object, mockRenderer.Object, mockLogger);

        var result = await runner.RunAsync(cancellationToken: TestContext.Current.CancellationToken);

        result.Should().BeSameAs(game, "Game should still be returned even when persistence fails");
    }

    [Fact]
    public async Task RunAsync_PassesCancellationTokenToRepository()
    {
        var mockOrchestrator = new Mock<IGameOrchestrator>();
        var mockRepository = new Mock<IGameRepository>();
        var mockRenderer = new Mock<IGameResultsRenderer>();
        var mockLogger = Mock.Of<ILogger<SingleGameRunner>>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var runner = new SingleGameRunner(mockOrchestrator.Object, mockRepository.Object, mockRenderer.Object, mockLogger);
        using var cts = new CancellationTokenSource();

        await runner.RunAsync(cancellationToken: cts.Token);

        mockRepository.Verify(
            r => r.SaveCompletedGameAsync(It.IsAny<Game>(), cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_ExecutesInCorrectOrder()
    {
        var mockOrchestrator = new Mock<IGameOrchestrator>();
        var mockRepository = new Mock<IGameRepository>();
        var mockRenderer = new Mock<IGameResultsRenderer>();
        var mockLogger = Mock.Of<ILogger<SingleGameRunner>>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };

        var callOrder = new List<string>();

        mockOrchestrator.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game)
            .Callback(() => callOrder.Add("Orchestrate"));

        mockRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback(() => callOrder.Add("Persist"));

        mockRenderer.Setup(x => x.RenderResults(It.IsAny<Game>(), false))
            .Callback(() => callOrder.Add("Render"));

        var runner = new SingleGameRunner(mockOrchestrator.Object, mockRepository.Object, mockRenderer.Object, mockLogger);

        await runner.RunAsync(cancellationToken: TestContext.Current.CancellationToken);

        callOrder.Should().ContainInOrder("Orchestrate", "Persist", "Render");
    }

    [Fact]
    public async Task RunAsync_WithDoNotPersist_SkipsPersistence()
    {
        var mockOrchestrator = new Mock<IGameOrchestrator>();
        var mockRepository = new Mock<IGameRepository>();
        var mockRenderer = new Mock<IGameResultsRenderer>();
        var mockLogger = new Mock<ILogger<SingleGameRunner>>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);

        var runner = new SingleGameRunner(mockOrchestrator.Object, mockRepository.Object, mockRenderer.Object, mockLogger.Object);

        await runner.RunAsync(doNotPersist: true, cancellationToken: TestContext.Current.CancellationToken);

        mockRepository.Verify(
            r => r.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Repository should not be called when doNotPersist is true");
    }

    [Fact]
    public async Task RunAsync_WithDoNotPersist_StillRendersResults()
    {
        var mockOrchestrator = new Mock<IGameOrchestrator>();
        var mockRepository = new Mock<IGameRepository>();
        var mockRenderer = new Mock<IGameResultsRenderer>();
        var mockLogger = Mock.Of<ILogger<SingleGameRunner>>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);

        var runner = new SingleGameRunner(mockOrchestrator.Object, mockRepository.Object, mockRenderer.Object, mockLogger);

        await runner.RunAsync(doNotPersist: true, cancellationToken: TestContext.Current.CancellationToken);

        mockRenderer.Verify(r => r.RenderResults(game, false), Times.Once, "Results should still be rendered");
    }

    [Fact]
    public async Task RunAsync_WithDoNotPersist_LogsSkippedMessage()
    {
        var mockOrchestrator = new Mock<IGameOrchestrator>();
        var mockRepository = new Mock<IGameRepository>();
        var mockRenderer = new Mock<IGameResultsRenderer>();
        var mockLogger = new Mock<ILogger<SingleGameRunner>>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);

        var runner = new SingleGameRunner(mockOrchestrator.Object, mockRepository.Object, mockRenderer.Object, mockLogger.Object);

        await runner.RunAsync(doNotPersist: true, cancellationToken: TestContext.Current.CancellationToken);

        mockLogger.Verify(
            x => x.IsEnabled(LogLevel.Information),
            Times.AtLeastOnce(),
            "Info logging should be attempted for skipped persistence");
    }
}
