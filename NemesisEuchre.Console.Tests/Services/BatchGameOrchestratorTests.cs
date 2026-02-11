using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;
using NemesisEuchre.Console.Services.Orchestration;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Options;

namespace NemesisEuchre.Console.Tests.Services;

public class BatchGameOrchestratorTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceProvider> _scopedServiceProviderMock;
    private readonly Mock<IGameOrchestrator> _gameOrchestratorMock;
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<IOptions<PersistenceOptions>> _persistenceOptionsMock;
    private readonly Mock<ILogger<BatchGameOrchestrator>> _loggerMock;
    private readonly Mock<IParallelismCoordinator> _parallelismCoordinatorMock;
    private readonly Mock<ISubBatchStrategy> _subBatchStrategyMock;
    private readonly Mock<IPersistenceCoordinator> _persistenceCoordinatorMock;
    private readonly BatchGameOrchestrator _sut;

    public BatchGameOrchestratorTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _scopedServiceProviderMock = new Mock<IServiceProvider>();
        _gameOrchestratorMock = new Mock<IGameOrchestrator>();
        _gameRepositoryMock = new Mock<IGameRepository>();
        _persistenceOptionsMock = new Mock<IOptions<PersistenceOptions>>();
        _loggerMock = new Mock<ILogger<BatchGameOrchestrator>>();
        _parallelismCoordinatorMock = new Mock<IParallelismCoordinator>();
        _subBatchStrategyMock = new Mock<ISubBatchStrategy>();
        _persistenceCoordinatorMock = new Mock<IPersistenceCoordinator>();

        _persistenceOptionsMock.Setup(x => x.Value).Returns(new PersistenceOptions
        {
            BatchSize = 100,
            MaxBatchSizeForChangeTracking = 50,
        });

        _serviceScopeFactoryMock.Setup(x => x.CreateScope())
            .Returns(_serviceScopeMock.Object);

        _serviceScopeMock.Setup(x => x.ServiceProvider)
            .Returns(_scopedServiceProviderMock.Object);

        _scopedServiceProviderMock.Setup(x => x.GetService(typeof(IGameOrchestrator)))
            .Returns(_gameOrchestratorMock.Object);

        _scopedServiceProviderMock.Setup(x => x.GetService(typeof(IGameRepository)))
            .Returns(_gameRepositoryMock.Object);

        _gameRepositoryMock.Setup(x => x.SaveCompletedGamesBulkAsync(It.IsAny<IEnumerable<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Game>, IProgress<int>?, CancellationToken>((games, progress, _) => progress?.Report(games.Count()))
            .Returns(Task.CompletedTask);

        _subBatchStrategyMock.Setup(x => x.ShouldUseSubBatches(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(false);

        _parallelismCoordinatorMock.Setup(x => x.CalculateEffectiveParallelism())
            .Returns(4);

        _persistenceCoordinatorMock.Setup(x => x.ConsumeAndPersistAsync(
            It.IsAny<BatchExecutionState>(),
            It.IsAny<GamePersistenceOptions?>(),
            It.IsAny<CancellationToken>()))
            .Returns<BatchExecutionState, GamePersistenceOptions?, CancellationToken>(
                async (state, persistenceOptions, ct) =>
                {
                    await foreach (var game in state.Reader.ReadAllAsync(ct).ConfigureAwait(false))
                    {
                        if (persistenceOptions?.PersistToSql != true)
                        {
                            state.SavedGames++;
                        }
                        else
                        {
                            try
                            {
                                using var scope = _serviceScopeFactoryMock.Object.CreateScope();
                                var gameRepository = scope.ServiceProvider.GetRequiredService<IGameRepository>();
                                var saveProgress = new Progress<int>(count => state.SavedGames += count);
                                await gameRepository.SaveCompletedGamesBulkAsync([game], saveProgress, ct).ConfigureAwait(false);
                            }
#pragma warning disable RCS1075, S108
                            catch (Exception)
                            {
                            }
#pragma warning restore RCS1075, S108
                        }
                    }
                });

        _sut = new BatchGameOrchestrator(
            _serviceScopeFactoryMock.Object,
            _parallelismCoordinatorMock.Object,
            _subBatchStrategyMock.Object,
            _persistenceCoordinatorMock.Object,
            _persistenceOptionsMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RunBatchAsync_WithSingleGame_ReturnsCorrectResults()
    {
        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        var results = await _sut.RunBatchAsync(1, cancellationToken: TestContext.Current.CancellationToken);

        results.TotalGames.Should().Be(1);
        results.Team1Wins.Should().Be(1);
        results.Team2Wins.Should().Be(0);
        results.FailedGames.Should().Be(0);
        results.TotalDeals.Should().Be(5);
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
            .ReturnsAsync(() => games[Interlocked.Increment(ref callCount) - 1]);

        var results = await _sut.RunBatchAsync(5, cancellationToken: TestContext.Current.CancellationToken);

        results.TotalGames.Should().Be(5);
        results.Team1Wins.Should().Be(3);
        results.Team2Wins.Should().Be(2);
        results.FailedGames.Should().Be(0);
        results.TotalDeals.Should().Be(25);
        results.Team1WinRate.Should().BeApproximately(0.6, 0.01);
        results.Team2WinRate.Should().BeApproximately(0.4, 0.01);
    }

    [Fact]
    public async Task RunBatchAsync_CreatesCorrectNumberOfScopes()
    {
        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        await _sut.RunBatchAsync(5, persistenceOptions: new GamePersistenceOptions(true, null), cancellationToken: TestContext.Current.CancellationToken);

        _serviceScopeFactoryMock.Verify(x => x.CreateScope(), Times.Exactly(10), "5 scopes for games + 5 scopes for persistence (one per game in mock consumer)");
    }

    [Fact]
    public async Task RunBatchAsync_DisposesAllScopes()
    {
        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        await _sut.RunBatchAsync(5, persistenceOptions: new GamePersistenceOptions(true, null), cancellationToken: TestContext.Current.CancellationToken);

        _serviceScopeMock.Verify(x => x.Dispose(), Times.Exactly(10), "5 scopes for games + 5 scopes for persistence (one per game in mock consumer)");
    }

    [Fact]
    public async Task RunBatchAsync_WithProgressReporter_ReportsProgress()
    {
        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        var reportedCompletedValues = new List<int>();
        var progressReporter = new Mock<IBatchProgressReporter>();
        progressReporter.Setup(x => x.ReportGameCompleted(It.IsAny<int>()))
            .Callback<int>(reportedCompletedValues.Add);

        await _sut.RunBatchAsync(3, progressReporter: progressReporter.Object, cancellationToken: TestContext.Current.CancellationToken);

        reportedCompletedValues.Should().HaveCount(3);
        reportedCompletedValues.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task RunBatchAsync_WithMaxDegreeOfParallelism_LimitsParallelism()
    {
        var parallelismOptions = new Mock<IOptions<GameExecutionOptions>>();
        parallelismOptions.Setup(x => x.Value).Returns(new GameExecutionOptions
        {
            MaxDegreeOfParallelism = 2,
        });
        var realParallelismCoordinator = new ParallelismCoordinator(parallelismOptions.Object);

        var sut = new BatchGameOrchestrator(
            _serviceScopeFactoryMock.Object,
            realParallelismCoordinator,
            _subBatchStrategyMock.Object,
            _persistenceCoordinatorMock.Object,
            _persistenceOptionsMock.Object,
            _loggerMock.Object);

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

        await sut.RunBatchAsync(10, cancellationToken: TestContext.Current.CancellationToken);

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
                if (Interlocked.Increment(ref callCount) == 2)
                {
                    throw new InvalidOperationException("Test exception");
                }

                return game;
            });

        var results = await _sut.RunBatchAsync(3, cancellationToken: TestContext.Current.CancellationToken);

        results.TotalGames.Should().Be(3);
        results.Team1Wins.Should().Be(2);
        results.Team2Wins.Should().Be(0);
        results.FailedGames.Should().Be(1);
        results.TotalDeals.Should().Be(10);
    }

    [Fact]
    public async Task RunBatchAsync_WithFailedGame_LogsError()
    {
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

        await _sut.RunBatchAsync(1, cancellationToken: TestContext.Current.CancellationToken);

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

    [Fact]
    public async Task RunBatchAsync_UsesConfiguredMaxDegreeOfParallelism()
    {
        var parallelismOptions = new Mock<IOptions<GameExecutionOptions>>();
        parallelismOptions.Setup(x => x.Value).Returns(new GameExecutionOptions
        {
            MaxDegreeOfParallelism = 3,
        });
        var realParallelismCoordinator = new ParallelismCoordinator(parallelismOptions.Object);

        var sut = new BatchGameOrchestrator(
            _serviceScopeFactoryMock.Object,
            realParallelismCoordinator,
            _subBatchStrategyMock.Object,
            _persistenceCoordinatorMock.Object,
            _persistenceOptionsMock.Object,
            _loggerMock.Object);

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

        await sut.RunBatchAsync(10, cancellationToken: TestContext.Current.CancellationToken);

        maxConcurrent.Should().BeLessThanOrEqualTo(3);
    }

    [Fact]
    public Task RunBatchAsync_UsesDefaultOptionsWhenNotConfigured()
    {
        var sut = new BatchGameOrchestrator(
            _serviceScopeFactoryMock.Object,
            _parallelismCoordinatorMock.Object,
            _subBatchStrategyMock.Object,
            _persistenceCoordinatorMock.Object,
            _persistenceOptionsMock.Object,
            _loggerMock.Object);

        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        var act = async () => await sut.RunBatchAsync(5);

        return act.Should().NotThrowAsync();
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
            .ReturnsAsync(() => games[Interlocked.Increment(ref callCount) - 1]);

        var results = await _sut.RunBatchAsync(4, cancellationToken: TestContext.Current.CancellationToken);

        results.Team1WinRate.Should().BeApproximately(0.75, 0.01);
        results.Team2WinRate.Should().BeApproximately(0.25, 0.01);
    }

    [Fact]
    public async Task RunBatchAsync_WithNoGamesCompleted_ReturnsZeroWinRates()
    {
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ThrowsAsync(new InvalidOperationException("All games failed"));

        var results = await _sut.RunBatchAsync(3, cancellationToken: TestContext.Current.CancellationToken);

        results.Team1WinRate.Should().Be(0.0);
        results.Team2WinRate.Should().Be(0.0);
        results.FailedGames.Should().Be(3);
    }

    [Fact]
    public async Task RunBatchAsync_Should_PersistCompletedGamesInBatches()
    {
        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        await _sut.RunBatchAsync(5, persistenceOptions: new GamePersistenceOptions(true, null), cancellationToken: TestContext.Current.CancellationToken);

        _gameRepositoryMock.Verify(
            x => x.SaveCompletedGamesBulkAsync(It.IsAny<IEnumerable<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task RunBatchAsync_Should_PersistGamesWithCorrectData()
    {
        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        await _sut.RunBatchAsync(3, persistenceOptions: new GamePersistenceOptions(true, null), cancellationToken: TestContext.Current.CancellationToken);

        _gameRepositoryMock.Verify(
            x => x.SaveCompletedGamesBulkAsync(
                It.Is<IEnumerable<Game>>(games =>
                    games.All(g => g.WinningTeam == Team.Team1 && g.CompletedDeals.Count == 5)),
                It.IsAny<IProgress<int>>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task RunBatchAsync_WithPersistenceFailure_ContinuesBatch()
    {
        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        _gameRepositoryMock.Setup(x => x.SaveCompletedGamesBulkAsync(It.IsAny<IEnumerable<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var results = await _sut.RunBatchAsync(3, persistenceOptions: new GamePersistenceOptions(true, null), cancellationToken: TestContext.Current.CancellationToken);

        results.TotalGames.Should().Be(3);
        results.Team1Wins.Should().Be(3);
        results.FailedGames.Should().Be(0);
        _gameRepositoryMock.Verify(
            x => x.SaveCompletedGamesBulkAsync(It.IsAny<IEnumerable<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task RunBatchAsync_WithPersistenceFailure_LogsError()
    {
        var persistenceLogger = new Mock<ILogger<BatchPersistenceCoordinator>>();
        persistenceLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
        var realPersistenceCoordinator = new BatchPersistenceCoordinator(
            _serviceScopeFactoryMock.Object,
            Mock.Of<IGameToTrainingDataConverter>(),
            Mock.Of<ITrainingDataAccumulator>(),
            Options.Create(new PersistenceOptions { BatchSize = 100 }),
            persistenceLogger.Object);

        var sut = new BatchGameOrchestrator(
            _serviceScopeFactoryMock.Object,
            _parallelismCoordinatorMock.Object,
            _subBatchStrategyMock.Object,
            realPersistenceCoordinator,
            _persistenceOptionsMock.Object,
            _loggerMock.Object);

        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        _gameRepositoryMock.Setup(x => x.SaveCompletedGamesBulkAsync(It.IsAny<IEnumerable<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        await sut.RunBatchAsync(1, persistenceOptions: new GamePersistenceOptions(true, null), cancellationToken: TestContext.Current.CancellationToken);

        persistenceLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RunBatchAsync_Should_NotPersist_FailedGames()
    {
        var game = CreateGameWithWinner(Team.Team1);
        var callCount = 0;

        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(() =>
            {
                if (Interlocked.Increment(ref callCount) == 2)
                {
                    throw new InvalidOperationException("Game failed");
                }

                return game;
            });

        var persistedGameCount = 0;
        _gameRepositoryMock.Setup(x => x.SaveCompletedGamesBulkAsync(It.IsAny<IEnumerable<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Game>, IProgress<int>?, CancellationToken>((games, _, _) => Interlocked.Add(ref persistedGameCount, games.Count()))
            .Returns(Task.CompletedTask);

        await _sut.RunBatchAsync(3, persistenceOptions: new GamePersistenceOptions(true, null), cancellationToken: TestContext.Current.CancellationToken);

        persistedGameCount.Should().Be(2, "Only 2 games should be persisted (1 failed)");
    }

    [Fact]
    public async Task RunBatchAsync_WithDoNotPersist_SkipsPersistence()
    {
        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        await _sut.RunBatchAsync(5, persistenceOptions: new GamePersistenceOptions(false, null), cancellationToken: TestContext.Current.CancellationToken);

        _gameRepositoryMock.Verify(
            x => x.SaveCompletedGamesBulkAsync(It.IsAny<IEnumerable<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Repository should not be called when persistence is disabled");
    }

    [Fact]
    public async Task RunBatchAsync_WithDoNotPersist_StillReportsProgress()
    {
        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        var progressReporter = new Mock<IBatchProgressReporter>();

        await _sut.RunBatchAsync(3, progressReporter: progressReporter.Object, persistenceOptions: new GamePersistenceOptions(false, null), cancellationToken: TestContext.Current.CancellationToken);

        progressReporter.Verify(x => x.ReportGameCompleted(It.IsAny<int>()), Times.Exactly(3));
    }

    [Fact]
    public async Task RunBatchAsync_WithDoNotPersist_ReturnsCorrectResults()
    {
        var games = new[]
        {
            CreateGameWithWinner(Team.Team1),
            CreateGameWithWinner(Team.Team2),
            CreateGameWithWinner(Team.Team1),
        };

        var callCount = 0;
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(() => games[Interlocked.Increment(ref callCount) - 1]);

        var results = await _sut.RunBatchAsync(3, persistenceOptions: new GamePersistenceOptions(false, null), cancellationToken: TestContext.Current.CancellationToken);

        results.TotalGames.Should().Be(3);
        results.Team1Wins.Should().Be(2);
        results.Team2Wins.Should().Be(1);
        results.FailedGames.Should().Be(0);
    }

    [Fact]
    public async Task RunBatchAsync_WithDoNotPersist_LogsSkippedMessage()
    {
        var persistenceLogger = new Mock<ILogger<BatchPersistenceCoordinator>>();
        persistenceLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
        var realPersistenceCoordinator = new BatchPersistenceCoordinator(
            _serviceScopeFactoryMock.Object,
            Mock.Of<IGameToTrainingDataConverter>(),
            Mock.Of<ITrainingDataAccumulator>(),
            Options.Create(new PersistenceOptions { BatchSize = 100 }),
            persistenceLogger.Object);

        var sut = new BatchGameOrchestrator(
            _serviceScopeFactoryMock.Object,
            _parallelismCoordinatorMock.Object,
            _subBatchStrategyMock.Object,
            realPersistenceCoordinator,
            _persistenceOptionsMock.Object,
            _loggerMock.Object);

        var game = CreateGameWithWinner(Team.Team1);
        _gameOrchestratorMock.Setup(x => x.OrchestrateGameAsync())
            .ReturnsAsync(game);

        await sut.RunBatchAsync(3, persistenceOptions: new GamePersistenceOptions(false, null), cancellationToken: TestContext.Current.CancellationToken);

        persistenceLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private static Game CreateGameWithWinner(Team winningTeam, int numberOfDeals = 5)
    {
        var game = new Game
        {
            WinningTeam = winningTeam,
        };

        for (int i = 0; i < numberOfDeals; i++)
        {
            game.CompletedDeals.Add(new Deal());
        }

        return game;
    }
}
