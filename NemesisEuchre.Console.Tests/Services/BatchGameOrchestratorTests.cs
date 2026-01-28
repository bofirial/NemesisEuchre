using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Services;
using NemesisEuchre.GameEngine;
using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Tests.Services;

public class BatchGameOrchestratorTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceProvider> _scopedServiceProviderMock;
    private readonly Mock<IGameOrchestrator> _gameOrchestratorMock;
    private readonly Mock<ILogger<BatchGameOrchestrator>> _loggerMock;
    private readonly BatchGameOrchestrator _sut;

    public BatchGameOrchestratorTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _scopedServiceProviderMock = new Mock<IServiceProvider>();
        _gameOrchestratorMock = new Mock<IGameOrchestrator>();
        _loggerMock = new Mock<ILogger<BatchGameOrchestrator>>();

        _serviceScopeFactoryMock.Setup(x => x.CreateScope())
            .Returns(_serviceScopeMock.Object);

        _serviceScopeMock.Setup(x => x.ServiceProvider)
            .Returns(_scopedServiceProviderMock.Object);

        _scopedServiceProviderMock.Setup(x => x.GetService(typeof(IGameOrchestrator)))
            .Returns(_gameOrchestratorMock.Object);

        _sut = new BatchGameOrchestrator(_serviceScopeFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task RunBatchAsync_WithSingleGame_ReturnsCorrectResults()
    {
        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        var results = await _sut.RunBatchAsync(1);

        results.TotalGames.Should().Be(1);
        results.Team1Wins.Should().Be(1);
        results.Team2Wins.Should().Be(0);
        results.FailedGames.Should().Be(0);
        results.ElapsedTime.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task RunBatchAsync_WithMultipleGames_AggregatesWinsCorrectly()
    {
        var games = new[]
        {
            CreateGameWithWinner(Team.Team1),
            CreateGameWithWinner(Team.Team2),
            CreateGameWithWinner(Team.Team1),
            CreateGameWithWinner(Team.Team2),
            CreateGameWithWinner(Team.Team1),
        };

        var callCount = 0;
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(() => games[callCount++]);

        var results = await _sut.RunBatchAsync(5);

        results.TotalGames.Should().Be(5);
        results.Team1Wins.Should().Be(3);
        results.Team2Wins.Should().Be(2);
        results.FailedGames.Should().Be(0);
        results.Team1WinRate.Should().BeApproximately(0.6, 0.01);
        results.Team2WinRate.Should().BeApproximately(0.4, 0.01);
    }

    [Fact]
    public async Task RunBatchAsync_CreatesCorrectNumberOfScopes()
    {
        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        await _sut.RunBatchAsync(5);

        _serviceScopeFactoryMock.Verify(x => x.CreateScope(), Times.Exactly(5));
    }

    [Fact]
    public async Task RunBatchAsync_DisposesAllScopes()
    {
        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        await _sut.RunBatchAsync(5);

        _serviceScopeMock.Verify(x => x.Dispose(), Times.Exactly(5));
    }

    [Fact]
    public async Task RunBatchAsync_WithProgressReporter_ReportsProgress()
    {
        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        var reportedValues = new List<int>();
        var progress = new Progress<int>(reportedValues.Add);

        await _sut.RunBatchAsync(3, progress: progress);

        reportedValues.Should().HaveCount(3);
        reportedValues.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task RunBatchAsync_WithMaxConcurrentGames_LimitsParallelism()
    {
        var game = CreateGameWithWinner(Team.Team1);
        int currentConcurrent = 0;
        int maxConcurrent = 0;
        var lockObject = new object();

        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .Returns(async () =>
            {
                lock (lockObject)
                {
                    currentConcurrent++;
                    maxConcurrent = Math.Max(maxConcurrent, currentConcurrent);
                }

                await Task.Delay(50);

                lock (lockObject)
                {
                    currentConcurrent--;
                }

                return game;
            });

        await _sut.RunBatchAsync(10, maxConcurrentGames: 2);

        maxConcurrent.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task RunBatchAsync_WithFailedGame_ContinuesBatch()
    {
        var game = CreateGameWithWinner(Team.Team1);
        var callCount = 0;

        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(() =>
            {
                if (callCount++ == 1)
                {
                    throw new InvalidOperationException("Test exception");
                }

                return game;
            });

        var results = await _sut.RunBatchAsync(3);

        results.TotalGames.Should().Be(3);
        results.Team1Wins.Should().Be(2);
        results.Team2Wins.Should().Be(0);
        results.FailedGames.Should().Be(1);
    }

    [Fact]
    public async Task RunBatchAsync_WithFailedGame_LogsError()
    {
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

        await _sut.RunBatchAsync(1);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public Task RunBatchAsync_WithInvalidNumberOfGames_ThrowsArgumentOutOfRangeException(int numberOfGames)
    {
        var act = async () => await _sut.RunBatchAsync(numberOfGames);

        return act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName(nameof(numberOfGames));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public Task RunBatchAsync_WithInvalidMaxConcurrentGames_ThrowsArgumentOutOfRangeException(int maxConcurrentGames)
    {
        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        var act = async () => await _sut.RunBatchAsync(1, maxConcurrentGames);

        return act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName(nameof(maxConcurrentGames));
    }

    [Fact]
    public async Task RunBatchAsync_WithCancellationToken_ThrowsOperationCanceledException()
    {
        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .Returns(async () =>
            {
                await Task.Delay(100);
                return game;
            });

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = async () => await _sut.RunBatchAsync(5, cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RunBatchAsync_CalculatesWinRatesCorrectly()
    {
        var games = new[]
        {
            CreateGameWithWinner(Team.Team1),
            CreateGameWithWinner(Team.Team1),
            CreateGameWithWinner(Team.Team1),
            CreateGameWithWinner(Team.Team2),
        };

        var callCount = 0;
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(() => games[callCount++]);

        var results = await _sut.RunBatchAsync(4);

        results.Team1WinRate.Should().BeApproximately(0.75, 0.01);
        results.Team2WinRate.Should().BeApproximately(0.25, 0.01);
    }

    [Fact]
    public async Task RunBatchAsync_WithNoGamesCompleted_ReturnsZeroWinRates()
    {
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ThrowsAsync(new InvalidOperationException("All games failed"));

        var results = await _sut.RunBatchAsync(3);

        results.Team1WinRate.Should().Be(0.0);
        results.Team2WinRate.Should().Be(0.0);
        results.FailedGames.Should().Be(3);
    }

    private static Game CreateGameWithWinner(Team winningTeam)
    {
        return new Game
        {
            WinningTeam = winningTeam,
        };
    }
}
