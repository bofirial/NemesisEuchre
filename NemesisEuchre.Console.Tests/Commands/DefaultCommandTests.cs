using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Commands;
using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;
using NemesisEuchre.Foundation.Constants;
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
        var mockSingleGameRunner = new Mock<ISingleGameRunner>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockSingleGameRunner.Setup(x => x.RunAsync(It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(game);

        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var command = new DefaultCommand(mockLogger, testConsole, mockBanner.Object, mockSingleGameRunner.Object, mockBatchGameOrchestrator.Object, mockGameResultsRenderer);

        await command.RunAsync();

        mockBanner.Verify(b => b.Display(), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenExecuted_OutputsWelcomeMessage()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = new Mock<IApplicationBanner>();
        var mockSingleGameRunner = new Mock<ISingleGameRunner>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockSingleGameRunner.Setup(x => x.RunAsync(It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(game);

        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var command = new DefaultCommand(mockLogger, testConsole, mockBanner.Object, mockSingleGameRunner.Object, mockBatchGameOrchestrator.Object, mockGameResultsRenderer);

        await command.RunAsync();

        testConsole.Output.Should().Contain("Welcome to NemesisEuchre - AI-Powered Euchre Strategy");
    }

    [Fact]
    public async Task RunAsync_WhenExecuted_ReturnsZero()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = new Mock<ISingleGameRunner>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockSingleGameRunner.Setup(x => x.RunAsync(It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(game);

        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var command = new DefaultCommand(mockLogger, testConsole, mockBanner, mockSingleGameRunner.Object, mockBatchGameOrchestrator.Object, mockGameResultsRenderer);

        var result = await command.RunAsync();

        result.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_WhenExecuted_CallsSingleGameRunner()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = new Mock<ISingleGameRunner>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockSingleGameRunner.Setup(x => x.RunAsync(It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(game);

        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var command = new DefaultCommand(mockLogger, testConsole, mockBanner, mockSingleGameRunner.Object, mockBatchGameOrchestrator.Object, mockGameResultsRenderer);

        await command.RunAsync();

        mockSingleGameRunner.Verify(o => o.RunAsync(It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenExecuted_DisplaysStatusMessage()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = new Mock<ISingleGameRunner>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockSingleGameRunner.Setup(x => x.RunAsync(It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(game);

        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var command = new DefaultCommand(mockLogger, testConsole, mockBanner, mockSingleGameRunner.Object, mockBatchGameOrchestrator.Object, mockGameResultsRenderer);

        await command.RunAsync();

        testConsole.Output.Should().Contain("Playing a game between 2 ChaosBots and 2 ChaosBots");
    }

    [Fact]
    public void Count_Should_DefaultToOne()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = Mock.Of<ISingleGameRunner>();
        var mockBatchGameOrchestrator = Mock.Of<IBatchGameOrchestrator>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockSingleGameRunner,
            mockBatchGameOrchestrator,
            mockGameResultsRenderer);

        command.Count.Should().Be(1);
    }

    [Fact]
    public async Task RunAsync_WithCountOne_UsesSingleGameRunner()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = new Mock<ISingleGameRunner>();
        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockSingleGameRunner.Setup(x => x.RunAsync(It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(game);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockSingleGameRunner.Object,
            mockBatchGameOrchestrator.Object,
            mockGameResultsRenderer)
        {
            Count = 1,
        };

        await command.RunAsync();

        mockSingleGameRunner.Verify(o => o.RunAsync(It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        mockBatchGameOrchestrator.Verify(
            o => o.RunBatchAsync(It.IsAny<int>(), It.IsAny<IBatchProgressReporter>(), It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RunAsync_WithCountGreaterThanOne_UsesBatchGameOrchestrator()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = new Mock<ISingleGameRunner>();
        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var batchResults = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 6,
            Team2Wins = 4,
            FailedGames = 0,
            TotalDeals = 50,
            TotalTricks = 0,
            TotalCallTrumpDecisions = 0,
            TotalDiscardCardDecisions = 0,
            TotalPlayCardDecisions = 0,
            ElapsedTime = TimeSpan.FromSeconds(5),
        };

        mockBatchGameOrchestrator.Setup(x => x.RunBatchAsync(
                It.IsAny<int>(),
                It.IsAny<IBatchProgressReporter>(),
                It.IsAny<bool>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchResults);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockSingleGameRunner.Object,
            mockBatchGameOrchestrator.Object,
            mockGameResultsRenderer)
        {
            Count = 10,
        };

        await command.RunAsync();

        mockBatchGameOrchestrator.Verify(
            o => o.RunBatchAsync(10, It.IsAny<IBatchProgressReporter>(), It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), default),
            Times.Once);
        mockSingleGameRunner.Verify(o => o.RunAsync(It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_WithBatchExecution_DisplaysBatchResults()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = Mock.Of<ISingleGameRunner>();
        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var mockGameResultsRenderer = new Mock<IGameResultsRenderer>();

        var batchResults = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 6,
            Team2Wins = 4,
            FailedGames = 0,
            TotalDeals = 50,
            TotalTricks = 0,
            TotalCallTrumpDecisions = 0,
            TotalDiscardCardDecisions = 0,
            TotalPlayCardDecisions = 0,
            ElapsedTime = TimeSpan.FromSeconds(5),
        };

        mockBatchGameOrchestrator.Setup(x => x.RunBatchAsync(
                It.IsAny<int>(),
                It.IsAny<IBatchProgressReporter>(),
                It.IsAny<bool>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchResults);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockSingleGameRunner,
            mockBatchGameOrchestrator.Object,
            mockGameResultsRenderer.Object)
        {
            Count = 10,
        };

        await command.RunAsync();

        mockGameResultsRenderer.Verify(r => r.RenderBatchResults(batchResults), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithBatchExecution_DoesNotCallSingleGameRunner()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = new Mock<ISingleGameRunner>();
        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var mockGameResultsRenderer = new Mock<IGameResultsRenderer>();

        var batchResults = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 6,
            Team2Wins = 4,
            FailedGames = 0,
            TotalDeals = 50,
            TotalTricks = 0,
            TotalCallTrumpDecisions = 0,
            TotalDiscardCardDecisions = 0,
            TotalPlayCardDecisions = 0,
            ElapsedTime = TimeSpan.FromSeconds(5),
        };

        mockBatchGameOrchestrator.Setup(x => x.RunBatchAsync(
                It.IsAny<int>(),
                It.IsAny<IBatchProgressReporter>(),
                It.IsAny<bool>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchResults);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockSingleGameRunner.Object,
            mockBatchGameOrchestrator.Object,
            mockGameResultsRenderer.Object)
        {
            Count = 10,
        };

        await command.RunAsync();

        mockSingleGameRunner.Verify(o => o.RunAsync(It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_WithBatchExecution_PassesProgressReporter()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = Mock.Of<ISingleGameRunner>();
        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var batchResults = new BatchGameResults
        {
            TotalGames = 5,
            Team1Wins = 3,
            Team2Wins = 2,
            FailedGames = 0,
            TotalDeals = 25,
            TotalTricks = 0,
            TotalCallTrumpDecisions = 0,
            TotalDiscardCardDecisions = 0,
            TotalPlayCardDecisions = 0,
            ElapsedTime = TimeSpan.FromSeconds(2),
        };

        mockBatchGameOrchestrator.Setup(x => x.RunBatchAsync(
                It.IsAny<int>(),
                It.IsAny<IBatchProgressReporter>(),
                It.IsAny<bool>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchResults);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockSingleGameRunner,
            mockBatchGameOrchestrator.Object,
            mockGameResultsRenderer)
        {
            Count = 5,
        };

        await command.RunAsync();

        mockBatchGameOrchestrator.Verify(
            o => o.RunBatchAsync(5, It.IsNotNull<IBatchProgressReporter>(), It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), default),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithSingleGame_PassesNullActorTypesWhenNotSpecified()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = new Mock<ISingleGameRunner>();
        var mockBatchGameOrchestrator = Mock.Of<IBatchGameOrchestrator>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockSingleGameRunner.Setup(x => x.RunAsync(It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(game);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockSingleGameRunner.Object,
            mockBatchGameOrchestrator,
            mockGameResultsRenderer)
        {
            Count = 1,
        };

        await command.RunAsync();

        mockSingleGameRunner.Verify(o => o.RunAsync(false, null, null, default), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithSingleGame_PassesTeam1ActorTypesWhenSpecified()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = new Mock<ISingleGameRunner>();
        var mockBatchGameOrchestrator = Mock.Of<IBatchGameOrchestrator>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockSingleGameRunner.Setup(x => x.RunAsync(It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(game);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockSingleGameRunner.Object,
            mockBatchGameOrchestrator,
            mockGameResultsRenderer)
        {
            Count = 1,
            Team1 = ActorType.Gen1,
        };

        await command.RunAsync();

        mockSingleGameRunner.Verify(
            o => o.RunAsync(
                false,
                It.Is<ActorType[]>(a => a.Length == 2 && a[0] == ActorType.Gen1 && a[1] == ActorType.Gen1),
                null,
                default),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithSingleGame_PassesTeam2ActorTypesWhenSpecified()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = new Mock<ISingleGameRunner>();
        var mockBatchGameOrchestrator = Mock.Of<IBatchGameOrchestrator>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockSingleGameRunner.Setup(x => x.RunAsync(It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(game);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockSingleGameRunner.Object,
            mockBatchGameOrchestrator,
            mockGameResultsRenderer)
        {
            Count = 1,
            Team2 = ActorType.Gen1,
        };

        await command.RunAsync();

        mockSingleGameRunner.Verify(
            o => o.RunAsync(
                false,
                null,
                It.Is<ActorType[]>(a => a.Length == 2 && a[0] == ActorType.Gen1 && a[1] == ActorType.Gen1),
                default),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithSingleGame_PassesBothTeamActorTypesWhenSpecified()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = new Mock<ISingleGameRunner>();
        var mockBatchGameOrchestrator = Mock.Of<IBatchGameOrchestrator>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockSingleGameRunner.Setup(x => x.RunAsync(It.IsAny<bool>(), It.IsAny<ActorType[]?>(), It.IsAny<ActorType[]?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(game);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockSingleGameRunner.Object,
            mockBatchGameOrchestrator,
            mockGameResultsRenderer)
        {
            Count = 1,
            Team1 = ActorType.Gen1,
            Team2 = ActorType.Chaos,
        };

        await command.RunAsync();

        mockSingleGameRunner.Verify(
            o => o.RunAsync(
                false,
                It.Is<ActorType[]>(a => a.Length == 2 && a[0] == ActorType.Gen1 && a[1] == ActorType.Gen1),
                It.Is<ActorType[]>(a => a.Length == 2 && a[0] == ActorType.Chaos && a[1] == ActorType.Chaos),
                default),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithBatchGames_PassesNullActorTypesWhenNotSpecified()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = Mock.Of<ISingleGameRunner>();
        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var batchResults = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 6,
            Team2Wins = 4,
            FailedGames = 0,
            TotalDeals = 50,
            TotalTricks = 0,
            TotalCallTrumpDecisions = 0,
            TotalDiscardCardDecisions = 0,
            TotalPlayCardDecisions = 0,
            ElapsedTime = TimeSpan.FromSeconds(5),
        };

        mockBatchGameOrchestrator.Setup(x => x.RunBatchAsync(
                It.IsAny<int>(),
                It.IsAny<IBatchProgressReporter>(),
                It.IsAny<bool>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchResults);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockSingleGameRunner,
            mockBatchGameOrchestrator.Object,
            mockGameResultsRenderer)
        {
            Count = 10,
        };

        await command.RunAsync();

        mockBatchGameOrchestrator.Verify(
            o => o.RunBatchAsync(10, It.IsAny<IBatchProgressReporter>(), false, null, null, default),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithBatchGames_PassesTeam1ActorTypesWhenSpecified()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = Mock.Of<ISingleGameRunner>();
        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var batchResults = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 6,
            Team2Wins = 4,
            FailedGames = 0,
            TotalDeals = 50,
            TotalTricks = 0,
            TotalCallTrumpDecisions = 0,
            TotalDiscardCardDecisions = 0,
            TotalPlayCardDecisions = 0,
            ElapsedTime = TimeSpan.FromSeconds(5),
        };

        mockBatchGameOrchestrator.Setup(x => x.RunBatchAsync(
                It.IsAny<int>(),
                It.IsAny<IBatchProgressReporter>(),
                It.IsAny<bool>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchResults);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockSingleGameRunner,
            mockBatchGameOrchestrator.Object,
            mockGameResultsRenderer)
        {
            Count = 10,
            Team1 = ActorType.Gen1,
        };

        await command.RunAsync();

        mockBatchGameOrchestrator.Verify(
            o => o.RunBatchAsync(
                10,
                It.IsAny<IBatchProgressReporter>(),
                false,
                It.Is<ActorType[]>(a => a.Length == 2 && a[0] == ActorType.Gen1 && a[1] == ActorType.Gen1),
                null,
                default),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithBatchGames_PassesTeam2ActorTypesWhenSpecified()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = Mock.Of<ISingleGameRunner>();
        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var batchResults = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 6,
            Team2Wins = 4,
            FailedGames = 0,
            TotalDeals = 50,
            TotalTricks = 0,
            TotalCallTrumpDecisions = 0,
            TotalDiscardCardDecisions = 0,
            TotalPlayCardDecisions = 0,
            ElapsedTime = TimeSpan.FromSeconds(5),
        };

        mockBatchGameOrchestrator.Setup(x => x.RunBatchAsync(
                It.IsAny<int>(),
                It.IsAny<IBatchProgressReporter>(),
                It.IsAny<bool>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchResults);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockSingleGameRunner,
            mockBatchGameOrchestrator.Object,
            mockGameResultsRenderer)
        {
            Count = 10,
            Team2 = ActorType.Chaos,
        };

        await command.RunAsync();

        mockBatchGameOrchestrator.Verify(
            o => o.RunBatchAsync(
                10,
                It.IsAny<IBatchProgressReporter>(),
                false,
                null,
                It.Is<ActorType[]>(a => a.Length == 2 && a[0] == ActorType.Chaos && a[1] == ActorType.Chaos),
                default),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithBatchGames_PassesBothTeamActorTypesWhenSpecified()
    {
        var testConsole = new TestConsole();
        var mockLogger = Mock.Of<ILogger<DefaultCommand>>();
        var mockBanner = Mock.Of<IApplicationBanner>();
        var mockSingleGameRunner = Mock.Of<ISingleGameRunner>();
        var mockBatchGameOrchestrator = new Mock<IBatchGameOrchestrator>();
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var batchResults = new BatchGameResults
        {
            TotalGames = 10,
            Team1Wins = 6,
            Team2Wins = 4,
            FailedGames = 0,
            TotalDeals = 50,
            TotalTricks = 0,
            TotalCallTrumpDecisions = 0,
            TotalDiscardCardDecisions = 0,
            TotalPlayCardDecisions = 0,
            ElapsedTime = TimeSpan.FromSeconds(5),
        };

        mockBatchGameOrchestrator.Setup(x => x.RunBatchAsync(
                It.IsAny<int>(),
                It.IsAny<IBatchProgressReporter>(),
                It.IsAny<bool>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<ActorType[]?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchResults);

        var command = new DefaultCommand(
            mockLogger,
            testConsole,
            mockBanner,
            mockSingleGameRunner,
            mockBatchGameOrchestrator.Object,
            mockGameResultsRenderer)
        {
            Count = 10,
            Team1 = ActorType.Gen1,
            Team2 = ActorType.Chaos,
        };

        await command.RunAsync();

        mockBatchGameOrchestrator.Verify(
            o => o.RunBatchAsync(
                10,
                It.IsAny<IBatchProgressReporter>(),
                false,
                It.Is<ActorType[]>(a => a.Length == 2 && a[0] == ActorType.Gen1 && a[1] == ActorType.Gen1),
                It.Is<ActorType[]>(a => a.Length == 2 && a[0] == ActorType.Chaos && a[1] == ActorType.Chaos),
                default),
            Times.Once);
    }
}
