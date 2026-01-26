using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Commands;
using NemesisEuchre.Console.Services;
using NemesisEuchre.GameEngine;
using NemesisEuchre.GameEngine.Constants;
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
        var mockGameResultsRenderer = new Mock<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);

        var command = new DefaultCommand(mockLogger, testConsole, mockBanner.Object, mockGameOrchestrator.Object, mockGameResultsRenderer.Object);

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
        var mockGameResultsRenderer = new Mock<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);

        var command = new DefaultCommand(mockLogger, testConsole, mockBanner.Object, mockGameOrchestrator.Object, mockGameResultsRenderer.Object);

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
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);

        var command = new DefaultCommand(mockLogger, testConsole, mockBanner, mockGameOrchestrator.Object, mockGameResultsRenderer);

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
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);

        var command = new DefaultCommand(mockLogger, testConsole, mockBanner, mockGameOrchestrator.Object, mockGameResultsRenderer);

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
        var mockGameResultsRenderer = new Mock<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);

        var command = new DefaultCommand(mockLogger, testConsole, mockBanner, mockGameOrchestrator.Object, mockGameResultsRenderer.Object);

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
        var mockGameResultsRenderer = Mock.Of<IGameResultsRenderer>();

        var game = new Game
        {
            GameStatus = GameStatus.Complete,
            Team1Score = 10,
            Team2Score = 7,
            WinningTeam = Team.Team1,
        };
        mockGameOrchestrator.Setup(x => x.OrchestrateGameAsync()).ReturnsAsync(game);

        var command = new DefaultCommand(mockLogger, testConsole, mockBanner, mockGameOrchestrator.Object, mockGameResultsRenderer);

        await command.RunAsync();

        testConsole.Output.Should().Contain("Playing a game between 4 ChaosBots");
    }
}
