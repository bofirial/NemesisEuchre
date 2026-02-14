using System.Collections.Concurrent;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NemesisEuchre.Console.Models;
using NemesisEuchre.Console.Services;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.Console.Tests.Services;

public class TrainingDataAccumulatorTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly Mock<IIdvFileService> _mockIdvFileService = new();
    private readonly TrainingDataAccumulator _accumulator;
    private readonly ConcurrentDictionary<string, IdvFileMetadata> _lastSavedMetadata = new();
    private readonly List<(string path, int count)> _savedPlayCardCalls = [];
    private readonly List<(string path, int count)> _savedCallTrumpCalls = [];
    private readonly List<(string path, int count)> _savedDiscardCardCalls = [];
    private bool _disposed;

    public TrainingDataAccumulatorTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"TrainingDataAccumulatorTests_{Guid.NewGuid()}");
        var options = Options.Create(new PersistenceOptions { IdvOutputPath = _tempDirectory });

        _mockIdvFileService
            .Setup(x => x.LoadMetadata(It.IsAny<string>()))
            .Returns((string path) =>
            {
                var saved = _lastSavedMetadata.GetValueOrDefault(path);
                return saved ?? new IdvFileMetadata("test", DecisionType.Play, 0, 0, 0, 0, [], DateTime.UtcNow);
            });

        _mockIdvFileService
            .Setup(x => x.SaveMetadata(It.IsAny<IdvFileMetadata>(), It.IsAny<string>()))
            .Callback((IdvFileMetadata metadata, string path) => _lastSavedMetadata[path] = metadata);

        _mockIdvFileService
            .Setup(x => x.Save(It.IsAny<IEnumerable<PlayCardTrainingData>>(), It.IsAny<string>()))
            .Callback((IEnumerable<PlayCardTrainingData> data, string path) =>
            {
                var materialized = data.ToList();
                _savedPlayCardCalls.Add((path, count: materialized.Count));
                CreateStubFile(path);
            });
        _mockIdvFileService
            .Setup(x => x.Save(It.IsAny<IEnumerable<CallTrumpTrainingData>>(), It.IsAny<string>()))
            .Callback((IEnumerable<CallTrumpTrainingData> data, string path) =>
            {
                var materialized = data.ToList();
                _savedCallTrumpCalls.Add((path, count: materialized.Count));
                CreateStubFile(path);
            });
        _mockIdvFileService
            .Setup(x => x.Save(It.IsAny<IEnumerable<DiscardCardTrainingData>>(), It.IsAny<string>()))
            .Callback((IEnumerable<DiscardCardTrainingData> data, string path) =>
            {
                var materialized = data.ToList();
                _savedDiscardCardCalls.Add((path, count: materialized.Count));
                CreateStubFile(path);
            });

        var buffer = new TrainingDataBuffer(
            _mockIdvFileService.Object,
            Mock.Of<ILogger<TrainingDataBuffer>>());
        var merger = new IdvChunkMerger(
            _mockIdvFileService.Object,
            Mock.Of<ILogger<IdvChunkMerger>>());
        var metadataService = new IdvMetadataService(
            _mockIdvFileService.Object,
            Mock.Of<ILogger<IdvMetadataService>>());

        _accumulator = new TrainingDataAccumulator(
            buffer,
            merger,
            metadataService,
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
        _accumulator.SaveChunk("test-generation");
        _accumulator.Finalize("test-generation");

        _savedPlayCardCalls.Should().ContainSingle(c => c.count == 5);
        _savedCallTrumpCalls.Should().ContainSingle(c => c.count == 3);
        _savedDiscardCardCalls.Should().ContainSingle(c => c.count == 1);
    }

    [Fact]
    public void Add_ShouldNotThrow_WhenBatchIsEmpty()
    {
        var emptyBatch = new TrainingDataBatch([], [], [], new TrainingDataBatchStats(0, 0, 0, []));

        var act = () => _accumulator.Add(emptyBatch);

        act.Should().NotThrow();
    }

    [Fact]
    public void Finalize_ShouldProduceFinalFiles_WithCorrectPaths()
    {
        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        _accumulator.SaveChunk("generation-42");
        _accumulator.Finalize("generation-42");

        var chunkDir = Path.Combine(_tempDirectory, "_chunks", "generation-42");
        _mockIdvFileService.Verify(
            x => x.Save(
                It.IsAny<List<PlayCardTrainingData>>(),
                Path.Combine(chunkDir, "PlayCard_chunk0001.idv")),
            Times.Once);
    }

    [Fact]
    public void SaveChunk_ShouldCreateOutputDirectory_IfItDoesNotExist()
    {
        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }

        _accumulator.SaveChunk("test-generation");

        Directory.Exists(_tempDirectory).Should().BeTrue();
    }

    [Fact]
    public void Finalize_ShouldSaveMetadata_ForEachIdvFile()
    {
        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        _accumulator.SaveChunk("gen-1");
        _accumulator.Finalize("gen-1");

        _mockIdvFileService.Verify(
            x => x.SaveMetadata(It.IsAny<IdvFileMetadata>(), It.IsAny<string>()),
            Times.Exactly(3));
    }

    [Fact]
    public void Finalize_ShouldSaveMetadata_WithCorrectRowCounts()
    {
        var batch = CreateBatch(playCardCount: 5, callTrumpCount: 3, discardCardCount: 2);
        _accumulator.Add(batch);

        _accumulator.SaveChunk("gen-1");
        _accumulator.Finalize("gen-1");

        _mockIdvFileService.Verify(
            x => x.SaveMetadata(
                It.Is<IdvFileMetadata>(m => m.RowCount == 5 && m.DecisionType == DecisionType.Play),
                It.IsAny<string>()),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.SaveMetadata(
                It.Is<IdvFileMetadata>(m => m.RowCount == 3 && m.DecisionType == DecisionType.CallTrump),
                It.IsAny<string>()),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.SaveMetadata(
                It.Is<IdvFileMetadata>(m => m.RowCount == 2 && m.DecisionType == DecisionType.Discard),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void Finalize_ShouldSaveMetadata_WithAccumulatedGameCount()
    {
        var stats1 = new TrainingDataBatchStats(10, 50, 200, []);
        var batch1 = new TrainingDataBatch([], [], [], stats1);
        var stats2 = new TrainingDataBatchStats(5, 25, 100, []);
        var batch2 = new TrainingDataBatch([], [], [], stats2);

        _accumulator.Add(batch1);
        _accumulator.SaveChunk("gen-1");
        _accumulator.Add(batch2);
        _accumulator.SaveChunk("gen-1");
        _accumulator.Finalize("gen-1");

        _mockIdvFileService.Verify(
            x => x.SaveMetadata(
                It.Is<IdvFileMetadata>(m => m.GameCount == 15 && m.DealCount == 75 && m.TrickCount == 300),
                It.IsAny<string>()),
            Times.Exactly(3));
    }

    [Fact]
    public void Finalize_ShouldSaveMetadata_WithCorrectFilePaths()
    {
        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        _accumulator.SaveChunk("gen-1");
        _accumulator.Finalize("gen-1");

        _mockIdvFileService.Verify(
            x => x.SaveMetadata(
                It.IsAny<IdvFileMetadata>(),
                It.Is<string>(path => path.EndsWith("gen-1_PlayCard.idv.meta.json"))),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.SaveMetadata(
                It.IsAny<IdvFileMetadata>(),
                It.Is<string>(path => path.EndsWith("gen-1_CallTrump.idv.meta.json"))),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.SaveMetadata(
                It.IsAny<IdvFileMetadata>(),
                It.Is<string>(path => path.EndsWith("gen-1_DiscardCard.idv.meta.json"))),
            Times.Once);
    }

    [Fact]
    public void Add_ShouldAccumulateStats_AcrossMultipleBatches()
    {
        var actor1 = new Actor(ActorType.Chaos);
        var actor2 = new Actor(ActorType.Model, "gen1", 0.1f);

        var stats1 = new TrainingDataBatchStats(3, 15, 60, [actor1]);
        var batch1 = new TrainingDataBatch([], [], [], stats1);
        var stats2 = new TrainingDataBatchStats(2, 10, 40, [actor1, actor2]);
        var batch2 = new TrainingDataBatch([], [], [], stats2);

        _accumulator.Add(batch1);
        _accumulator.Add(batch2);
        _accumulator.SaveChunk("gen-1");
        _accumulator.Finalize("gen-1");

        _mockIdvFileService.Verify(
            x => x.SaveMetadata(
                It.Is<IdvFileMetadata>(m =>
                    m.GameCount == 5 &&
                    m.DealCount == 25 &&
                    m.TrickCount == 100 &&
                    m.Actors.Count == 2),
                It.IsAny<string>()),
            Times.Exactly(3));
    }

    [Fact]
    public void SaveChunk_ThrowsInvalidOperationException_WhenIdvFileExists()
    {
        Directory.CreateDirectory(_tempDirectory);
        File.WriteAllText(Path.Combine(_tempDirectory, "gen1_PlayCard.idv"), "existing");

        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        var act = () => _accumulator.SaveChunk("gen1");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*IDV files already exist*--overwrite*");
    }

    [Fact]
    public void SaveChunk_ThrowsInvalidOperationException_WhenMetadataFileExists()
    {
        Directory.CreateDirectory(_tempDirectory);
        File.WriteAllText(Path.Combine(_tempDirectory, "gen1_CallTrump.idv.meta.json"), "existing");

        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        var act = () => _accumulator.SaveChunk("gen1");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*IDV files already exist*--overwrite*");
    }

    [Fact]
    public void SaveChunk_ListsAllConflictingFiles_InExceptionMessage()
    {
        Directory.CreateDirectory(_tempDirectory);
        File.WriteAllText(Path.Combine(_tempDirectory, "gen1_PlayCard.idv"), "existing");
        File.WriteAllText(Path.Combine(_tempDirectory, "gen1_CallTrump.idv.meta.json"), "existing");
        File.WriteAllText(Path.Combine(_tempDirectory, "gen1_DiscardCard.idv"), "existing");

        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        var act = () => _accumulator.SaveChunk("gen1");

        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should()
            .Contain("gen1_PlayCard.idv").And
            .Contain("gen1_CallTrump.idv.meta.json").And
            .Contain("gen1_DiscardCard.idv");
    }

    [Fact]
    public void SaveChunk_DoesNotWriteAnyFiles_WhenGuardFails()
    {
        Directory.CreateDirectory(_tempDirectory);
        File.WriteAllText(Path.Combine(_tempDirectory, "gen1_PlayCard.idv"), "existing");

        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        var act = () => _accumulator.SaveChunk("gen1");

        act.Should().Throw<InvalidOperationException>();
        _savedPlayCardCalls.Should().BeEmpty();
        _savedCallTrumpCalls.Should().BeEmpty();
        _savedDiscardCardCalls.Should().BeEmpty();
    }

    [Fact]
    public void SaveChunk_Succeeds_WhenFilesExistAndAllowOverwriteIsTrue()
    {
        Directory.CreateDirectory(_tempDirectory);
        File.WriteAllText(Path.Combine(_tempDirectory, "gen1_PlayCard.idv"), "existing");
        File.WriteAllText(Path.Combine(_tempDirectory, "gen1_CallTrump.idv.meta.json"), "existing");

        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        var act = () => _accumulator.SaveChunk("gen1", allowOverwrite: true);

        act.Should().NotThrow();
        _savedPlayCardCalls.Should().HaveCount(1);
    }

    [Fact]
    public void SaveChunk_SkipsOverwriteCheck_OnSubsequentChunks()
    {
        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        _accumulator.SaveChunk("gen1");

        Directory.CreateDirectory(_tempDirectory);
        foreach (var suffix in (string[])["PlayCard", "CallTrump", "DiscardCard"])
        {
            File.WriteAllText(Path.Combine(_tempDirectory, $"gen1_{suffix}.idv"), "data");
            File.WriteAllText(Path.Combine(_tempDirectory, $"gen1_{suffix}.idv.meta.json"), "data");
        }

        _accumulator.Add(CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1));
        var act = () => _accumulator.SaveChunk("gen1");

        act.Should().NotThrow();
    }

    [Fact]
    public void SaveChunk_ClearsInternalLists()
    {
        var batch1 = CreateBatch(playCardCount: 3, callTrumpCount: 2, discardCardCount: 1);
        _accumulator.Add(batch1);
        _accumulator.SaveChunk("gen1");

        var batch2 = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch2);
        _accumulator.SaveChunk("gen1");

        _savedPlayCardCalls.Should().HaveCount(2);
        _savedPlayCardCalls[0].count.Should().Be(3);
        _savedPlayCardCalls[1].count.Should().Be(1);
    }

    [Fact]
    public void SaveChunk_WritesToChunkSubdirectory()
    {
        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        _accumulator.SaveChunk("test-gen");

        var expectedChunkDir = Path.Combine(_tempDirectory, "_chunks", "test-gen");
        _mockIdvFileService.Verify(
            x => x.Save(
                It.IsAny<List<PlayCardTrainingData>>(),
                Path.Combine(expectedChunkDir, "PlayCard_chunk0001.idv")),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.Save(
                It.IsAny<List<CallTrumpTrainingData>>(),
                Path.Combine(expectedChunkDir, "CallTrump_chunk0001.idv")),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.Save(
                It.IsAny<List<DiscardCardTrainingData>>(),
                Path.Combine(expectedChunkDir, "DiscardCard_chunk0001.idv")),
            Times.Once);
    }

    [Fact]
    public void Finalize_SingleChunk_RenamesInsteadOfMerging()
    {
        var batch = CreateBatch(playCardCount: 2, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        _accumulator.SaveChunk("gen1");
        _accumulator.Finalize("gen1");

        _mockIdvFileService.Verify(
            x => x.StreamFromBinary<PlayCardTrainingData>(It.IsAny<string>()),
            Times.Never);
        _mockIdvFileService.Verify(
            x => x.StreamFromBinary<CallTrumpTrainingData>(It.IsAny<string>()),
            Times.Never);
        _mockIdvFileService.Verify(
            x => x.StreamFromBinary<DiscardCardTrainingData>(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void Finalize_MultipleChunks_MergesViaStreaming()
    {
        _mockIdvFileService
            .Setup(x => x.StreamFromBinary<PlayCardTrainingData>(It.IsAny<string>()))
            .Returns([new PlayCardTrainingData()]);
        _mockIdvFileService
            .Setup(x => x.StreamFromBinary<CallTrumpTrainingData>(It.IsAny<string>()))
            .Returns([new CallTrumpTrainingData()]);
        _mockIdvFileService
            .Setup(x => x.StreamFromBinary<DiscardCardTrainingData>(It.IsAny<string>()))
            .Returns([new DiscardCardTrainingData()]);

        _accumulator.Add(CreateBatch(playCardCount: 2, callTrumpCount: 1, discardCardCount: 1));
        _accumulator.SaveChunk("gen1");
        _accumulator.Add(CreateBatch(playCardCount: 3, callTrumpCount: 2, discardCardCount: 0));
        _accumulator.SaveChunk("gen1");
        _accumulator.Finalize("gen1");

        _mockIdvFileService.Verify(
            x => x.StreamFromBinary<PlayCardTrainingData>(It.IsAny<string>()),
            Times.Exactly(2));
        _mockIdvFileService.Verify(
            x => x.StreamFromBinary<CallTrumpTrainingData>(It.IsAny<string>()),
            Times.Exactly(2));
        _mockIdvFileService.Verify(
            x => x.StreamFromBinary<DiscardCardTrainingData>(It.IsAny<string>()),
            Times.Exactly(2));
    }

    [Fact]
    public void Finalize_WritesCorrectCumulativeMetadata()
    {
        var batch1 = CreateBatch(playCardCount: 5, callTrumpCount: 3, discardCardCount: 2);
        var batch2 = CreateBatch(playCardCount: 10, callTrumpCount: 4, discardCardCount: 1);

        _mockIdvFileService
            .Setup(x => x.StreamFromBinary<PlayCardTrainingData>(It.IsAny<string>()))
            .Returns([new PlayCardTrainingData()]);
        _mockIdvFileService
            .Setup(x => x.StreamFromBinary<CallTrumpTrainingData>(It.IsAny<string>()))
            .Returns([new CallTrumpTrainingData()]);
        _mockIdvFileService
            .Setup(x => x.StreamFromBinary<DiscardCardTrainingData>(It.IsAny<string>()))
            .Returns([new DiscardCardTrainingData()]);

        _accumulator.Add(batch1);
        _accumulator.SaveChunk("gen1");
        _accumulator.Add(batch2);
        _accumulator.SaveChunk("gen1");
        _accumulator.Finalize("gen1");

        _mockIdvFileService.Verify(
            x => x.SaveMetadata(
                It.Is<IdvFileMetadata>(m => m.RowCount == 15 && m.DecisionType == DecisionType.Play),
                It.IsAny<string>()),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.SaveMetadata(
                It.Is<IdvFileMetadata>(m => m.RowCount == 7 && m.DecisionType == DecisionType.CallTrump),
                It.IsAny<string>()),
            Times.Once);
        _mockIdvFileService.Verify(
            x => x.SaveMetadata(
                It.Is<IdvFileMetadata>(m => m.RowCount == 3 && m.DecisionType == DecisionType.Discard),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void Finalize_DeletesChunkDirectory()
    {
        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        _accumulator.SaveChunk("gen1");

        var chunkDir = Path.Combine(_tempDirectory, "_chunks", "gen1");
        Directory.Exists(chunkDir).Should().BeTrue();

        _accumulator.Finalize("gen1");

        Directory.Exists(chunkDir).Should().BeFalse();
    }

    [Fact]
    public void Finalize_FlushesRemainingData_IfListsNotEmpty()
    {
        var batch = CreateBatch(playCardCount: 2, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        _accumulator.Finalize("gen1");

        _savedPlayCardCalls.Should().ContainSingle(c => c.count == 2);
    }

    [Fact]
    public void Finalize_IsNoOp_WhenNoDataAdded()
    {
        var act = () => _accumulator.Finalize("gen1");

        act.Should().NotThrow();
        _savedPlayCardCalls.Should().BeEmpty();
        _savedCallTrumpCalls.Should().BeEmpty();
        _savedDiscardCardCalls.Should().BeEmpty();
        _mockIdvFileService.Verify(
            x => x.SaveMetadata(It.IsAny<IdvFileMetadata>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void Finalize_InvokesStatusCallback_AtKeyPoints()
    {
        _mockIdvFileService
            .Setup(x => x.StreamFromBinary<PlayCardTrainingData>(It.IsAny<string>()))
            .Returns([new PlayCardTrainingData()]);
        _mockIdvFileService
            .Setup(x => x.StreamFromBinary<CallTrumpTrainingData>(It.IsAny<string>()))
            .Returns([new CallTrumpTrainingData()]);
        _mockIdvFileService
            .Setup(x => x.StreamFromBinary<DiscardCardTrainingData>(It.IsAny<string>()))
            .Returns([new DiscardCardTrainingData()]);

        _accumulator.Add(CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1));
        _accumulator.SaveChunk("gen1");
        _accumulator.Add(CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1));
        _accumulator.SaveChunk("gen1");

        var statusMessages = new List<string>();

        _accumulator.Finalize("gen1", statusMessages.Add);

        statusMessages.Should().HaveCountGreaterThanOrEqualTo(2);
        statusMessages.Should().Contain(msg => msg.Contains("Merging"));
        statusMessages.Should().Contain(msg => msg.Contains("Cleaning up"));
    }

    [Fact]
    public void Finalize_InvokesStatusCallback_ForSingleChunkRename()
    {
        var batch = CreateBatch(playCardCount: 1, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);
        _accumulator.SaveChunk("gen1");

        var statusMessages = new List<string>();

        _accumulator.Finalize("gen1", statusMessages.Add);

        statusMessages.Should().HaveCountGreaterThanOrEqualTo(2);
        statusMessages.Should().Contain(msg => msg.Contains("Finalizing"));
        statusMessages.Should().Contain(msg => msg.Contains("Cleaning up"));
    }

    [Fact]
    public void Finalize_InvokesStatusCallback_WhenFlushingRemainingData()
    {
        var batch = CreateBatch(playCardCount: 2, callTrumpCount: 1, discardCardCount: 1);
        _accumulator.Add(batch);

        var statusMessages = new List<string>();

        _accumulator.Finalize("gen1", statusMessages.Add);

        statusMessages.Should().Contain(msg => msg.Contains("Saving remaining"));
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
        _accumulator.SaveChunk("test-generation");
        _accumulator.Finalize("test-generation");

        _savedPlayCardCalls.Should().ContainSingle(c => c.count == 6);
        _savedCallTrumpCalls.Should().ContainSingle(c => c.count == 4);
        _savedDiscardCardCalls.Should().ContainSingle(c => c.count == 3);
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

    private static void CreateStubFile(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (dir != null && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllBytes(path, []);
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

        var stats = new TrainingDataBatchStats(1, 5, 20, [new Actor(ActorType.Chaos)]);

        return new TrainingDataBatch(
            playCardData,
            callTrumpData,
            discardCardData,
            stats);
    }
}
