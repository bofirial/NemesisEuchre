using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Services;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Tests.Services;

public class BatchPersistenceCoordinatorTests
{
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory = new();
    private readonly Mock<IServiceScope> _mockScope = new();
    private readonly Mock<IServiceProvider> _mockServiceProvider = new();
    private readonly Mock<IGameRepository> _mockGameRepository = new();
    private readonly Mock<IBatchProgressReporter> _mockProgressReporter = new();
    private readonly BatchPersistenceCoordinator _coordinator;

    public BatchPersistenceCoordinatorTests()
    {
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IGameRepository))).Returns(_mockGameRepository.Object);
        _coordinator = new BatchPersistenceCoordinator(
            _mockScopeFactory.Object,
            Mock.Of<ILogger<BatchPersistenceCoordinator>>());
    }

    [Fact]
    public async Task SavePendingGamesAsync_ShouldExtractGames_WhenCountMeetsBatchSize()
    {
        using var state = new BatchExecutionState(2);
        state.PendingGames.AddRange([new Game(), new Game()]);
        _mockGameRepository
            .Setup(x => x.SaveCompletedGamesBulkAsync(It.IsAny<List<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _coordinator.SavePendingGamesAsync(state, _mockProgressReporter.Object, false, false, TestContext.Current.CancellationToken);

        state.PendingGames.Should().BeEmpty();
        _mockGameRepository.Verify(
            x => x.SaveCompletedGamesBulkAsync(It.Is<List<Game>>(g => g.Count == 2), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SavePendingGamesAsync_ShouldNotExtractGames_WhenCountBelowBatchSizeAndNotForced()
    {
        using var state = new BatchExecutionState(10);
        state.PendingGames.Add(new Game());

        await _coordinator.SavePendingGamesAsync(state, _mockProgressReporter.Object, false, false, TestContext.Current.CancellationToken);

        state.PendingGames.Should().HaveCount(1);
        _mockGameRepository.Verify(
            x => x.SaveCompletedGamesBulkAsync(It.IsAny<List<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SavePendingGamesAsync_ShouldExtractGames_WhenForceIsTrue()
    {
        using var state = new BatchExecutionState(100);
        state.PendingGames.Add(new Game());
        _mockGameRepository
            .Setup(x => x.SaveCompletedGamesBulkAsync(It.IsAny<List<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _coordinator.SavePendingGamesAsync(state, _mockProgressReporter.Object, false, true, TestContext.Current.CancellationToken);

        state.PendingGames.Should().BeEmpty();
        _mockGameRepository.Verify(
            x => x.SaveCompletedGamesBulkAsync(It.Is<List<Game>>(g => g.Count == 1), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SavePendingGamesAsync_ShouldNotExtractGames_WhenForceIsTrueButNoGames()
    {
        using var state = new BatchExecutionState(10);

        await _coordinator.SavePendingGamesAsync(state, _mockProgressReporter.Object, false, true, TestContext.Current.CancellationToken);

        _mockGameRepository.Verify(
            x => x.SaveCompletedGamesBulkAsync(It.IsAny<List<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SavePendingGamesAsync_ShouldSkipPersistence_WhenDoNotPersistIsTrue()
    {
        using var state = new BatchExecutionState(1);
        state.PendingGames.Add(new Game());

        await _coordinator.SavePendingGamesAsync(state, _mockProgressReporter.Object, true, false, TestContext.Current.CancellationToken);

        state.PendingGames.Should().BeEmpty();
        state.SavedGames.Should().Be(1);
        _mockGameRepository.Verify(
            x => x.SaveCompletedGamesBulkAsync(It.IsAny<List<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SavePendingGamesAsync_ShouldReportProgress_WhenDoNotPersistIsTrue()
    {
        using var state = new BatchExecutionState(1);
        state.PendingGames.Add(new Game());

        await _coordinator.SavePendingGamesAsync(state, _mockProgressReporter.Object, true, false, TestContext.Current.CancellationToken);

        _mockProgressReporter.Verify(x => x.ReportGamesSaved(1), Times.Once);
    }

    [Fact]
    public async Task SavePendingGamesAsync_ShouldHandleExceptionDuringPersistence()
    {
        using var state = new BatchExecutionState(1);
        state.PendingGames.Add(new Game());
        _mockGameRepository
            .Setup(x => x.SaveCompletedGamesBulkAsync(It.IsAny<List<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var act = () => _coordinator.SavePendingGamesAsync(state, _mockProgressReporter.Object, false, false, TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
        state.PendingGames.Should().BeEmpty();
    }

    [Fact]
    public async Task SavePendingGamesAsync_ShouldWorkWithoutProgressReporter()
    {
        using var state = new BatchExecutionState(1);
        state.PendingGames.Add(new Game());

        var act = () => _coordinator.SavePendingGamesAsync(state, null, true, false, TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
        state.SavedGames.Should().Be(1);
    }
}
