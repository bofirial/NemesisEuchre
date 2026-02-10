using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.DataAccess.Repositories;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.Console.Tests.Services;

public class BatchPersistenceCoordinatorTests
{
    private static readonly GamePersistenceOptions SqlPersistence = new(true, null);
    private static readonly GamePersistenceOptions NoPersistence = new(false, null);

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
            Mock.Of<IGameToTrainingDataConverter>(),
            Mock.Of<ITrainingDataAccumulator>(),
            Options.Create(new PersistenceOptions { BatchSize = 2 }),
            Mock.Of<ILogger<BatchPersistenceCoordinator>>());
    }

    [Fact]
    public async Task ConsumeAndPersistAsync_ShouldFlushBatch_WhenCountMeetsBatchSize()
    {
        using var state = new BatchExecutionState(10);
        _mockGameRepository
            .Setup(x => x.SaveCompletedGamesBulkAsync(It.IsAny<List<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await state.Writer.WriteAsync(new Game(), TestContext.Current.CancellationToken);
        await state.Writer.WriteAsync(new Game(), TestContext.Current.CancellationToken);
        state.Writer.Complete();

        await _coordinator.ConsumeAndPersistAsync(state, _mockProgressReporter.Object, SqlPersistence, TestContext.Current.CancellationToken);

        _mockGameRepository.Verify(
            x => x.SaveCompletedGamesBulkAsync(It.Is<List<Game>>(g => g.Count == 2), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConsumeAndPersistAsync_ShouldFlushPartialBatch_WhenChannelCompletes()
    {
        using var state = new BatchExecutionState(10);
        _mockGameRepository
            .Setup(x => x.SaveCompletedGamesBulkAsync(It.IsAny<List<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await state.Writer.WriteAsync(new Game(), TestContext.Current.CancellationToken);
        state.Writer.Complete();

        await _coordinator.ConsumeAndPersistAsync(state, _mockProgressReporter.Object, SqlPersistence, TestContext.Current.CancellationToken);

        _mockGameRepository.Verify(
            x => x.SaveCompletedGamesBulkAsync(It.Is<List<Game>>(g => g.Count == 1), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConsumeAndPersistAsync_ShouldSkipPersistence_WhenDoNotPersistIsTrue()
    {
        using var state = new BatchExecutionState(10);
        await state.Writer.WriteAsync(new Game(), TestContext.Current.CancellationToken);
        state.Writer.Complete();

        await _coordinator.ConsumeAndPersistAsync(state, _mockProgressReporter.Object, NoPersistence, TestContext.Current.CancellationToken);

        state.SavedGames.Should().Be(1);
        _mockGameRepository.Verify(
            x => x.SaveCompletedGamesBulkAsync(It.IsAny<List<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ConsumeAndPersistAsync_ShouldReportProgress_WhenDoNotPersistIsTrue()
    {
        using var state = new BatchExecutionState(10);
        await state.Writer.WriteAsync(new Game(), TestContext.Current.CancellationToken);
        state.Writer.Complete();

        await _coordinator.ConsumeAndPersistAsync(state, _mockProgressReporter.Object, NoPersistence, TestContext.Current.CancellationToken);

        _mockProgressReporter.Verify(x => x.ReportGamesSaved(1), Times.Once);
    }

    [Fact]
    public async Task ConsumeAndPersistAsync_ShouldHandleExceptionDuringPersistence()
    {
        using var state = new BatchExecutionState(10);
        await state.Writer.WriteAsync(new Game(), TestContext.Current.CancellationToken);
        state.Writer.Complete();
        _mockGameRepository
            .Setup(x => x.SaveCompletedGamesBulkAsync(It.IsAny<List<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var act = () => _coordinator.ConsumeAndPersistAsync(state, _mockProgressReporter.Object, SqlPersistence, TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ConsumeAndPersistAsync_ShouldWorkWithoutProgressReporter()
    {
        using var state = new BatchExecutionState(10);
        await state.Writer.WriteAsync(new Game(), TestContext.Current.CancellationToken);
        state.Writer.Complete();

        var act = () => _coordinator.ConsumeAndPersistAsync(state, null, NoPersistence, TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
        state.SavedGames.Should().Be(1);
    }

    [Fact]
    public async Task ConsumeAndPersistAsync_ShouldCompleteSilently_WhenChannelIsEmpty()
    {
        using var state = new BatchExecutionState(10);
        state.Writer.Complete();

        var act = () => _coordinator.ConsumeAndPersistAsync(state, _mockProgressReporter.Object, SqlPersistence, TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
        _mockGameRepository.Verify(
            x => x.SaveCompletedGamesBulkAsync(It.IsAny<List<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ConsumeAndPersistAsync_ShouldFlushMultipleBatches()
    {
        using var state = new BatchExecutionState(10);
        _mockGameRepository
            .Setup(x => x.SaveCompletedGamesBulkAsync(It.IsAny<List<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        for (int i = 0; i < 5; i++)
        {
            await state.Writer.WriteAsync(new Game(), TestContext.Current.CancellationToken);
        }

        state.Writer.Complete();

        await _coordinator.ConsumeAndPersistAsync(state, _mockProgressReporter.Object, SqlPersistence, TestContext.Current.CancellationToken);

        _mockGameRepository.Verify(
            x => x.SaveCompletedGamesBulkAsync(It.IsAny<List<Game>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3),
            "Should flush 2 full batches (2 games each) and 1 partial batch (1 game)");
    }
}
