using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.Console.Tests.Services;

public class TrainingDataAccumulatorTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly Mock<IIdvFileService> _mockIdvFileService = new();
    private readonly TrainingDataAccumulator _accumulator;
    private bool _disposed;

    public TrainingDataAccumulatorTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"TrainingDataAccumulatorTests_{Guid.NewGuid()}");
        var options = Options.Create(new PersistenceOptions { IdvOutputPath = _tempDirectory });
        _accumulator = new TrainingDataAccumulator(
            _mockIdvFileService.Object,
            options,
            Mock.Of<ILogger<TrainingDataAccumulator>>());
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Add_ShouldAccumulateTrainingData_FromMultipleBatches()
    {
        var batch1 = CreateBatch(playCardCount: 2, callTrumpCount: 1, discardCardCount: 1);
        var batch2 = CreateBatch(playCardCount: 3, callTrumpCount: 2, discardCardCount: 0);

        _accumulator.Add(batch1);
        _accumulator.Add(batch2);
        _accumulator.Save("test-generation");

        _mockIdvFileService.Verify(
            x => x.Save(It.Is<List<PlayCardTrainingData>>(d => d.Count == 5), It.IsAny<string>()),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.Save(It.Is<List<CallTrumpTrainingData>>(d => d.Count == 3), It.IsAny<string>()),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.Save(It.Is<List<DiscardCardTrainingData>>(d => d.Count == 1), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void Add_ShouldNotThrow_WhenBatchIsEmpty()
    {
        var emptyBatch = new TrainingDataBatch([], [], []);

        var act = () => _accumulator.Add(emptyBatch);

        act.Should().NotThrow();
    }

    [Fact]
    public void Save_ShouldCallIdvFileService_WithCorrectFilePaths()
    {
        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        _accumulator.Save("generation-42");

        _mockIdvFileService.Verify(
            x => x.Save(
                It.IsAny<List<PlayCardTrainingData>>(),
                Path.Combine(_tempDirectory, "generation-42_PlayCard.idv")),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.Save(
                It.IsAny<List<CallTrumpTrainingData>>(),
                Path.Combine(_tempDirectory, "generation-42_CallTrump.idv")),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.Save(
                It.IsAny<List<DiscardCardTrainingData>>(),
                Path.Combine(_tempDirectory, "generation-42_DiscardCard.idv")),
            Times.Once);
    }

    [Fact]
    public void Save_ShouldCreateOutputDirectory_IfItDoesNotExist()
    {
        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }

        _accumulator.Save("test-generation");

        Directory.Exists(_tempDirectory).Should().BeTrue();
    }

    [Fact]
    public void Save_ShouldCallIdvFileService_WithEmptyCollections_WhenNoDataAccumulated()
    {
        _accumulator.Save("empty-generation");

        _mockIdvFileService.Verify(
            x => x.Save(It.Is<List<PlayCardTrainingData>>(d => d.Count == 0), It.IsAny<string>()),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.Save(It.Is<List<CallTrumpTrainingData>>(d => d.Count == 0), It.IsAny<string>()),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.Save(It.Is<List<DiscardCardTrainingData>>(d => d.Count == 0), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void Save_ShouldPreserveAccumulatedData_ForMultipleSaveCalls()
    {
        var batch = CreateBatch(playCardCount: 2, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        _accumulator.Save("generation-1");
        _accumulator.Save("generation-2");

        _mockIdvFileService.Verify(
            x => x.Save(It.Is<List<PlayCardTrainingData>>(d => d.Count == 2), It.IsAny<string>()),
            Times.Exactly(2));
        _mockIdvFileService.Verify(
            x => x.Save(It.Is<List<CallTrumpTrainingData>>(d => d.Count == 1), It.IsAny<string>()),
            Times.Exactly(2));
        _mockIdvFileService.Verify(
            x => x.Save(It.Is<List<DiscardCardTrainingData>>(d => d.Count == 1), It.IsAny<string>()),
            Times.Exactly(2));
    }

    [Fact]
    public void Save_ShouldUseCorrectNamingConvention()
    {
        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        _accumulator.Save("my-generation");

        _mockIdvFileService.Verify(
            x => x.Save(
                It.IsAny<List<PlayCardTrainingData>>(),
                It.Is<string>(path => path.EndsWith("my-generation_PlayCard.idv"))),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.Save(
                It.IsAny<List<CallTrumpTrainingData>>(),
                It.Is<string>(path => path.EndsWith("my-generation_CallTrump.idv"))),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.Save(
                It.IsAny<List<DiscardCardTrainingData>>(),
                It.Is<string>(path => path.EndsWith("my-generation_DiscardCard.idv"))),
            Times.Once);
    }

    [Fact]
    public void Add_ShouldAccumulateDataAcrossBatches_WithDifferentCounts()
    {
        var batch1 = CreateBatch(playCardCount: 5, callTrumpCount: 0, discardCardCount: 2);
        var batch2 = CreateBatch(playCardCount: 0, callTrumpCount: 3, discardCardCount: 0);
        var batch3 = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);

        _accumulator.Add(batch1);
        _accumulator.Add(batch2);
        _accumulator.Add(batch3);
        _accumulator.Save("test-generation");

        _mockIdvFileService.Verify(
            x => x.Save(It.Is<List<PlayCardTrainingData>>(d => d.Count == 6), It.IsAny<string>()),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.Save(It.Is<List<CallTrumpTrainingData>>(d => d.Count == 4), It.IsAny<string>()),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.Save(It.Is<List<DiscardCardTrainingData>>(d => d.Count == 3), It.IsAny<string>()),
            Times.Once);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && Directory.Exists(_tempDirectory))
            {
                try
                {
                    Directory.Delete(_tempDirectory, true);
                }
#pragma warning disable S108
                catch
                {
                }
#pragma warning restore S108
            }

            _disposed = true;
        }
    }

    private static TrainingDataBatch CreateBatch(
        int playCardCount,
        int callTrumpCount,
        int discardCardCount)
    {
        var playCardData = Enumerable.Range(0, playCardCount)
            .Select(_ => new PlayCardTrainingData { Card1Rank = 1.0f })
            .ToList();
        var callTrumpData = Enumerable.Range(0, callTrumpCount)
            .Select(_ => new CallTrumpTrainingData { Card1Rank = 1.0f })
            .ToList();
        var discardCardData = Enumerable.Range(0, discardCardCount)
            .Select(_ => new DiscardCardTrainingData { Card1Rank = 1.0f })
            .ToList();

        return new TrainingDataBatch(
            playCardData,
            callTrumpData,
            discardCardData);
    }
}
