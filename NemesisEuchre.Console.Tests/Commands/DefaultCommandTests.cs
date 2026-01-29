using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Commands;
using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine;
using NemesisEuchre.GameEngine.Models;

using Spectre.Console.Testing;

namespace NemesisEuchre.Console.Tests.Commands;

public class DefaultCommandTests
{
    [Fact]
    public async Task RunAsync_WhenExecuted_DisplaysApplicationBanner()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = new Mock<IApplicationBanner>();
        var mockGameOrchestrator = new Mock<IGameOrchestrator>();
        var mockGameRepository = new Mock<IGameRepository>();
        var mockGameResultsRenderer = new Mock<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockGameRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var command = new DefaultCommand(mockLogger, testConsole, mockBanner.Object, mockGameOrchestrator.Object, mockBatchGameOrchestrator.Object, mockGameRepository.Object, mockGameResultsRenderer.Object);

        await command.RunAsync();

        mockBanner.Verify(b => b.Display(), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenExecuted_OutputsWelcomeMessage()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = new Mock<IApplicationBanner>();
        var mockGameOrchestrator = new Mock<IGameOrchestrator>();
        var mockGameRepository = new Mock<IGameRepository>();
        var mockGameResultsRenderer = new Mock<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockGameRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var command = new DefaultCommand(mockLogger, testConsole, mockBanner.Object, mockGameOrchestrator.Object, mockBatchGameOrchestrator.Object, mockGameRepository.Object, mockGameResultsRenderer.Object);

        await command.RunAsync();

        testConsole.Output.Should().Contain("Welcome to NemesisEuchre - AI-Powered Euchre Strategy");
    }

    [Fact]
    public async Task RunAsync_WhenExecuted_ReturnsZero()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockGameOrchestrator = new Mock<IGameOrchestrator>();
        var mockGameRepository = new Mock<IGameRepository>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockGameRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var command = new DefaultCommand(mockLogger, testConsole, mockBanner, mockGameOrchestrator.Object, mockBatchGameOrchestrator.Object, mockGameRepository.Object, mockGameResultsRenderer);

        var result = await command.RunAsync();

        result.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_WhenExecuted_CallsGameOrchestrator()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockGameOrchestrator = new Mock<IGameOrchestrator>();
        var mockGameRepository = new Mock<IGameRepository>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockGameRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var command = new DefaultCommand(mockLogger, testConsole, mockBanner, mockGameOrchestrator.Object, mockBatchGameOrchestrator.Object, mockGameRepository.Object, mockGameResultsRenderer);

        await command.RunAsync();

        mockGameOrchestrator.Verify(o => o.OrchestrateGameAsync(), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenExecuted_RendersGameResults()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockGameOrchestrator = new Mock<IGameOrchestrator>();
        var mockGameRepository = new Mock<IGameRepository>();
        var mockGameResultsRenderer = new Mock<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockGameRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var command = new DefaultCommand(mockLogger, testConsole, mockBanner, mockGameOrchestrator.Object, mockBatchGameOrchestrator.Object, mockGameRepository.Object, mockGameResultsRenderer.Object);

        await command.RunAsync();

        mockGameResultsRenderer.Verify(r => r.RenderResults(game), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenExecuted_DisplaysStatusMessage()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockGameOrchestrator = new Mock<IGameOrchestrator>();
        var mockGameRepository = new Mock<IGameRepository>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockGameRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var command = new DefaultCommand(mockLogger, testConsole, mockBanner, mockGameOrchestrator.Object, mockBatchGameOrchestrator.Object, mockGameRepository.Object, mockGameResultsRenderer);

        await command.RunAsync();

        testConsole.Output.Should().Contain("Playing a game between 4 ChaosBots");
    }

    [Fact]
    public async Task RunAsync_Should_PersistCompletedGame_WhenGameCompletes()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockGameOrchestrator = new Mock<IGameOrchestrator>();
        var mockGameRepository = new Mock<IGameRepository>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockGameRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockGameOrchestrator.Object,
            mockBatchGameOrchestrator.Object,
            mockGameRepository.Object,
            mockGameResultsRenderer);

        var result = await command.RunAsync();

        result.Should().Be(0);
        mockGameRepository.Verify(
            x => x.SaveCompletedGameAsync(
                It.Is<Game>(g => g.GameStatus == GameStatus.Complete && g.WinningTeam == Team.Team1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_Should_RenderResults_EvenWhen_PersistenceFails()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockGameOrchestrator = new Mock<IGameOrchestrator>();
        var mockGameRepository = new Mock<IGameRepository>();
        var mockGameResultsRenderer = new Mock<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockGameRepository
            .Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockGameOrchestrator.Object,
            mockBatchGameOrchestrator.Object,
            mockGameRepository.Object,
            mockGameResultsRenderer.Object);

        var result = await command.RunAsync();

        result.Should().Be(0);
        mockGameResultsRenderer.Verify(
            x => x.RenderResults(It.Is<Game>(g => g == game)),
            Times.Once,
            "Game results should be rendered even when persistence fails");
    }

    [Fact]
    public async Task RunAsync_Should_ContinueGracefully_When_PersistenceFails()
    {
        var testConsole = new TestConsole();
        var mockLogger = new Mock<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockGameOrchestrator = new Mock<IGameOrchestrator>();
        var mockGameRepository = new Mock<IGameRepository>();
        var mockGameResultsRenderer = new Mock<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        var expectedException = new InvalidOperationException("Database connection failed");

        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockGameRepository
            .Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var command = new DefaultCommand(
            mockLogger.Object,
            testConsole,
            mockBanner,
            mockGameOrchestrator.Object,
            mockBatchGameOrchestrator.Object,
            mockGameRepository.Object,
            mockGameResultsRenderer.Object);

        var result = await command.RunAsync();

        result.Should().Be(0, "Command should complete successfully even when persistence fails");
        mockGameResultsRenderer.Verify(
            x => x.RenderResults(It.Is<Game>(g => g == game)),
            Times.Once,
            "Game results should be rendered even when persistence fails");
        mockLogger.Verify(
            x => x.IsEnabled(LogLevel.Error),
            Times.AtLeastOnce(),
            "Error logging should be attempted when persistence fails");
    }

    [Fact]
    public async Task RunAsync_Should_CallRepositoryWith_CompletedGameData()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockGameOrchestrator = new Mock<IGameOrchestrator>();
        var mockGameRepository = new Mock<IGameRepository>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockGameRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockGameOrchestrator.Object,
            mockBatchGameOrchestrator.Object,
            mockGameRepository.Object,
            mockGameResultsRenderer);

        await command.RunAsync();

        mockGameRepository.Verify(
            x => x.SaveCompletedGameAsync(
                It.Is<Game>(g =>
                    g.GameStatus == GameStatus.Complete &&
                    g.WinningTeam == Team.Team1 &&
                    g.Team1Score == 10 &&
                    g.Team2Score == 7),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void Count_Should_DefaultToOne()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockGameOrchestrator = Mock.Of<IGameOrchestrator>();
        var mockBatchGameOrchestrator = Mock.Of<IBatchGameOrchestrator>();
        var mockGameRepository = Mock.Of<IGameRepository>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockGameOrchestrator,
            mockBatchGameOrchestrator,
            mockGameRepository,
            mockGameResultsRenderer);

        command.Count.Should().Be(1);
    }

    [Fact]
    public async Task RunAsync_WithCountOne_UsesGameOrchestrator()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockGameOrchestrator = new Mock<IGameOrchestrator>();
        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var mockGameRepository = new Mock<IGameRepository>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);
        mockGameRepository.Setup(x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockGameOrchestrator.Object,
            mockBatchGameOrchestrator.Object,
            mockGameRepository.Object,
            mockGameResultsRenderer)
        {
            Count = 1,
        };

        await command.RunAsync();

        mockGameOrchestrator.Verify(o => o.OrchestrateGameAsync(), Times.Once);
        mockBatchGameOrchestrator.Verify(
            o => o.RunBatchAsync(It.IsAny<int>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RunAsync_WithCountGreaterThanOne_UsesBatchGameOrchestrator()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockGameOrchestrator = new Mock<IGameOrchestrator>();
        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var mockGameRepository = Mock.Of<IGameRepository>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var batchResults = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 6,
            Team2Wins = 4,
            FailedGames = 0,
            TotalDeals = 50,
            ElapsedTime = TimeSpan.FromSeconds(5),
        };

        mockBatchGameOrchestrator.Setup(x => x.RunBatchAsync(
                It.IsAny<int>(),
                It.IsAny<IProgress<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchResults);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockGameOrchestrator.Object,
            mockBatchGameOrchestrator.Object,
            mockGameRepository,
            mockGameResultsRenderer)
        {
            Count = 10,
        };

        await command.RunAsync();

        mockBatchGameOrchestrator.Verify(
            o => o.RunBatchAsync(10, It.IsAny<IProgress<int>>(), default),
            Times.Once);
        mockGameOrchestrator.Verify(o => o.OrchestrateGameAsync(), Times.Never);
    }

    [Fact]
    public async Task RunAsync_WithBatchExecution_DisplaysBatchResults()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockGameOrchestrator = Mock.Of<IGameOrchestrator>();
        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var mockGameRepository = Mock.Of<IGameRepository>();
        var mockGameResultsRenderer = new Mock<IGameResultsRenderer>();

        var batchResults = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 6,
            Team2Wins = 4,
            FailedGames = 0,
            TotalDeals = 50,
            ElapsedTime = TimeSpan.FromSeconds(5),
        };

        mockBatchGameOrchestrator.Setup(x => x.RunBatchAsync(
                It.IsAny<int>(),
                It.IsAny<IProgress<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchResults);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockGameOrchestrator,
            mockBatchGameOrchestrator.Object,
            mockGameRepository,
            mockGameResultsRenderer.Object)
        {
            Count = 10,
        };

        await command.RunAsync();

        mockGameResultsRenderer.Verify(r => r.RenderBatchResults(batchResults), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithBatchExecution_DoesNotCallGameResultsRenderer()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockGameOrchestrator = Mock.Of<IGameOrchestrator>();
        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var mockGameRepository = Mock.Of<IGameRepository>();
        var mockGameResultsRenderer = new Mock<IGameResultsRenderer>();

        var batchResults = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 6,
            Team2Wins = 4,
            FailedGames = 0,
            TotalDeals = 50,
            ElapsedTime = TimeSpan.FromSeconds(5),
        };

        mockBatchGameOrchestrator.Setup(x => x.RunBatchAsync(
                It.IsAny<int>(),
                It.IsAny<IProgress<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchResults);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockGameOrchestrator,
            mockBatchGameOrchestrator.Object,
            mockGameRepository,
            mockGameResultsRenderer.Object)
        {
            Count = 10,
        };

        await command.RunAsync();

        mockGameResultsRenderer.Verify(r => r.RenderResults(It.IsAny<Game>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_WithBatchExecution_PassesProgressReporter()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockGameOrchestrator = Mock.Of<IGameOrchestrator>();
        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var mockGameRepository = Mock.Of<IGameRepository>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var batchResults = new BatchGameResults
        {
            TotalGames = 5,
            Team1Wins = 3,
            Team2Wins = 2,
            FailedGames = 0,
            TotalDeals = 25,
            ElapsedTime = TimeSpan.FromSeconds(2),
        };

        mockBatchGameOrchestrator.Setup(x => x.RunBatchAsync(
                It.IsAny<int>(),
                It.IsAny<IProgress<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchResults);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockGameOrchestrator,
            mockBatchGameOrchestrator.Object,
            mockGameRepository,
            mockGameResultsRenderer)
        {
            Count = 5,
        };

        await command.RunAsync();

        mockBatchGameOrchestrator.Verify(
            o => o.RunBatchAsync(5, It.IsNotNull<IProgress<int>>(), default),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithBatchExecution_DoesNotPersistGamesInDefaultCommand()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockGameOrchestrator = Mock.Of<IGameOrchestrator>();
        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var mockGameRepository = new Mock<IGameRepository>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var batchResults = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 6,
            Team2Wins = 4,
            FailedGames = 0,
            TotalDeals = 50,
            ElapsedTime = TimeSpan.FromSeconds(5),
        };

        mockBatchGameOrchestrator.Setup(x => x.RunBatchAsync(
                It.IsAny<int>(),
                It.IsAny<IProgress<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchResults);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockGameOrchestrator,
            mockBatchGameOrchestrator.Object,
            mockGameRepository.Object,
            mockGameResultsRenderer)
        {
            Count = 10,
        };

        await command.RunAsync();

        mockGameRepository.Verify(
            x => x.SaveCompletedGameAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Batch mode should handle its own persistence via BatchGameOrchestrator");
    }
}
