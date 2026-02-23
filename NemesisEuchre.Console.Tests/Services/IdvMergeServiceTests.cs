using System.Collections.Concurrent;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using NemesisEuchre.Console.Services;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;

using MicrosoftOptions = Microsoft.Extensions.Options.Options;

namespace NemesisEuchre.Console.Tests.Services;

public class IdvMergeServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly Mock<IIdvFileService> _mockIdvFileService = new();
    private readonly IdvMergeService _service;
    private readonly ConcurrentDictionary<string, IdvFileMetadata> _lastSavedMetadata = new();
    private readonly List<string> _savedPlayCardPaths = [];
    private readonly List<string> _savedCallTrumpPaths = [];
    private readonly List<string> _savedDiscardCardPaths = [];
    private readonly Dictionary<string, IdvFileMetadata> _sourceMetadataOverrides = [];
    private bool _disposed;

    public IdvMergeServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"IdvMergeServiceTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);

        var options = MicrosoftOptions.Create(new PersistenceOptions { IdvOutputPath = _tempDirectory });

        _mockIdvFileService
            .Setup(x => x.LoadMetadata(It.IsAny<string>()))
            .Returns((string path) =>
            {
                if (_lastSavedMetadata.TryGetValue(path, out var saved))
                {
                    return saved;
                }

                if (_sourceMetadataOverrides.TryGetValue(path, out var overridden))
                {
                    return overridden;
                }

                return CreateDefaultMetadata(path);
            });

        _mockIdvFileService
            .Setup(x => x.SaveMetadata(It.IsAny<IdvFileMetadata>(), It.IsAny<string>()))
            .Callback((IdvFileMetadata metadata, string path) => _lastSavedMetadata[path] = metadata);

        _mockIdvFileService
            .Setup(x => x.StreamFromBinary<PlayCardTrainingData>(It.IsAny<string>()))
            .Returns([new PlayCardTrainingData()]);
        _mockIdvFileService
            .Setup(x => x.StreamFromBinary<CallTrumpTrainingData>(It.IsAny<string>()))
            .Returns([new CallTrumpTrainingData()]);
        _mockIdvFileService
            .Setup(x => x.StreamFromBinary<DiscardCardTrainingData>(It.IsAny<string>()))
            .Returns([new DiscardCardTrainingData()]);

        _mockIdvFileService
            .Setup(x => x.Save(It.IsAny<IEnumerable<PlayCardTrainingData>>(), It.IsAny<string>()))
            .Callback((IEnumerable<PlayCardTrainingData> data, string path) =>
            {
                _ = data.ToList();
                _savedPlayCardPaths.Add(path);
            });
        _mockIdvFileService
            .Setup(x => x.Save(It.IsAny<IEnumerable<CallTrumpTrainingData>>(), It.IsAny<string>()))
            .Callback((IEnumerable<CallTrumpTrainingData> data, string path) =>
            {
                _ = data.ToList();
                _savedCallTrumpPaths.Add(path);
            });
        _mockIdvFileService
            .Setup(x => x.Save(It.IsAny<IEnumerable<DiscardCardTrainingData>>(), It.IsAny<string>()))
            .Callback((IEnumerable<DiscardCardTrainingData> data, string path) =>
            {
                _ = data.ToList();
                _savedDiscardCardPaths.Add(path);
            });

        var metadataService = new IdvMetadataService(
            _mockIdvFileService.Object,
            Mock.Of<ILogger<IdvMetadataService>>());

        _service = new IdvMergeService(
            _mockIdvFileService.Object,
            metadataService,
            options,
            Mock.Of<ILogger<IdvMergeService>>());
    }

    [Fact]
    public async Task MergeAsync_WithValidSources_SavesOutputFilesForAllThreeDecisionTypes()
    {
        CreateSourceFiles("source1");
        CreateSourceFiles("source2");

        await _service.MergeAsync(
            ["source1", "source2"],
            "merged",
            allowOverwrite: false,
            cancellationToken: TestContext.Current.CancellationToken);

        _savedPlayCardPaths.Should().ContainSingle(p => p.EndsWith("merged_PlayCard.idv"));
        _savedCallTrumpPaths.Should().ContainSingle(p => p.EndsWith("merged_CallTrump.idv"));
        _savedDiscardCardPaths.Should().ContainSingle(p => p.EndsWith("merged_DiscardCard.idv"));
    }

    [Fact]
    public Task MergeAsync_WhenSourcePlayCardFileMissing_ThrowsFileNotFoundException()
    {
        CreateSourceFile("source1", "CallTrump");
        CreateSourceFile("source1", "DiscardCard");

        var act = async () => await _service.MergeAsync(
            ["source1", "source2"],
            "merged",
            allowOverwrite: false,
            cancellationToken: TestContext.Current.CancellationToken);

        return act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("*source1_PlayCard.idv*");
    }

    [Fact]
    public Task MergeAsync_WhenOutputExistsAndOverwriteFalse_ThrowsInvalidOperationException()
    {
        CreateSourceFiles("source1");
        CreateSourceFiles("source2");
        File.WriteAllText(Path.Combine(_tempDirectory, "merged_PlayCard.idv"), "existing");

        var act = async () => await _service.MergeAsync(
            ["source1", "source2"],
            "merged",
            allowOverwrite: false,
            cancellationToken: TestContext.Current.CancellationToken);

        return act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*merged_PlayCard.idv*");
    }

    [Fact]
    public async Task MergeAsync_WhenOutputExistsAndOverwriteTrue_Succeeds()
    {
        CreateSourceFiles("source1");
        CreateSourceFiles("source2");
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "merged_PlayCard.idv"), "existing", TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "merged_CallTrump.idv.meta.json"), "existing", TestContext.Current.CancellationToken);

        var act = async () => await _service.MergeAsync(
            ["source1", "source2"],
            "merged",
            allowOverwrite: true,
            cancellationToken: TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
        _savedPlayCardPaths.Should().HaveCount(1);
    }

    [Fact]
    public async Task MergeAsync_AggregatesGameAndRowCountsCorrectly()
    {
        CreateSourceFiles("source1");
        CreateSourceFiles("source2");

        SetSourceMetadata("source1", "PlayCard", gameCount: 1000, dealCount: 5000, trickCount: 20000, rowCount: 100);
        SetSourceMetadata("source1", "CallTrump", gameCount: 1000, dealCount: 5000, trickCount: 20000, rowCount: 150);
        SetSourceMetadata("source1", "DiscardCard", gameCount: 1000, dealCount: 5000, trickCount: 20000, rowCount: 50);
        SetSourceMetadata("source2", "PlayCard", gameCount: 2000, dealCount: 10000, trickCount: 40000, rowCount: 200);
        SetSourceMetadata("source2", "CallTrump", gameCount: 2000, dealCount: 10000, trickCount: 40000, rowCount: 250);
        SetSourceMetadata("source2", "DiscardCard", gameCount: 2000, dealCount: 10000, trickCount: 40000, rowCount: 100);

        await _service.MergeAsync(
            ["source1", "source2"],
            "merged",
            allowOverwrite: false,
            cancellationToken: TestContext.Current.CancellationToken);

        _mockIdvFileService.Verify(
            x => x.SaveMetadata(
                It.Is<IdvFileMetadata>(m =>
                    m.DecisionType == DecisionType.Play &&
                    m.GameCount == 3000 &&
                    m.DealCount == 15000 &&
                    m.TrickCount == 60000 &&
                    m.RowCount == 300),
                It.IsAny<string>()),
            Times.Once);

        _mockIdvFileService.Verify(
            x => x.SaveMetadata(
                It.Is<IdvFileMetadata>(m =>
                    m.DecisionType == DecisionType.CallTrump &&
                    m.RowCount == 400),
                It.IsAny<string>()),
            Times.Once);

        _mockIdvFileService.Verify(
            x => x.SaveMetadata(
                It.Is<IdvFileMetadata>(m =>
                    m.DecisionType == DecisionType.Discard &&
                    m.RowCount == 150),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task MergeAsync_UnionsActorsAcrossSources()
    {
        CreateSourceFiles("source1");
        CreateSourceFiles("source2");

        var chaosActor = new ActorInfo(ActorType.Chaos, null, 0f);
        var modelActor1 = new ActorInfo(ActorType.Model, "gen1", 0.1f);
        var modelActor2 = new ActorInfo(ActorType.Model, "gen2", 0.2f);

        SetSourceMetadataWithActors("source1", "PlayCard", [chaosActor, modelActor1]);
        SetSourceMetadataWithActors("source1", "CallTrump", [chaosActor, modelActor1]);
        SetSourceMetadataWithActors("source1", "DiscardCard", [chaosActor, modelActor1]);
        SetSourceMetadataWithActors("source2", "PlayCard", [chaosActor, modelActor2]);
        SetSourceMetadataWithActors("source2", "CallTrump", [chaosActor, modelActor2]);
        SetSourceMetadataWithActors("source2", "DiscardCard", [chaosActor, modelActor2]);

        await _service.MergeAsync(
            ["source1", "source2"],
            "merged",
            allowOverwrite: false,
            cancellationToken: TestContext.Current.CancellationToken);

        _mockIdvFileService.Verify(
            x => x.SaveMetadata(
                It.Is<IdvFileMetadata>(m => m.Actors.Count == 3),
                It.IsAny<string>()),
            Times.Exactly(3));
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
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

    private static IdvFileMetadata CreateDefaultMetadata(string path)
    {
        var decisionType = path switch
        {
            _ when path.Contains("CallTrump") => DecisionType.CallTrump,
            _ when path.Contains("DiscardCard") => DecisionType.Discard,
            _ => DecisionType.Play,
        };
        return new IdvFileMetadata(
            "test",
            decisionType,
            RowCount: 100,
            GameCount: 1000,
            DealCount: 5000,
            TrickCount: 20000,
            [new ActorInfo(ActorType.Chaos, null, 0f)],
            DateTime.UtcNow);
    }

    private void CreateSourceFiles(string name)
    {
        foreach (var suffix in (string[])["PlayCard", "CallTrump", "DiscardCard"])
        {
            CreateSourceFile(name, suffix);
        }
    }

    private void CreateSourceFile(string name, string suffix)
    {
        File.WriteAllBytes(Path.Combine(_tempDirectory, $"{name}_{suffix}{FileExtensions.Idv}"), []);
    }

    private void SetSourceMetadata(
        string name,
        string suffix,
        int gameCount,
        int dealCount,
        int trickCount,
        int rowCount)
    {
        var decisionType = suffix switch
        {
            "CallTrump" => DecisionType.CallTrump,
            "DiscardCard" => DecisionType.Discard,
            _ => DecisionType.Play,
        };
        var idvPath = Path.Combine(_tempDirectory, $"{name}_{suffix}{FileExtensions.Idv}");
        var metaPath = idvPath + FileExtensions.IdvMetadataSuffix;

        _sourceMetadataOverrides[metaPath] = new IdvFileMetadata(
            name,
            decisionType,
            rowCount,
            gameCount,
            dealCount,
            trickCount,
            [new ActorInfo(ActorType.Chaos, null, 0f)],
            DateTime.UtcNow);
    }

    private void SetSourceMetadataWithActors(string name, string suffix, List<ActorInfo> actors)
    {
        var decisionType = suffix switch
        {
            "CallTrump" => DecisionType.CallTrump,
            "DiscardCard" => DecisionType.Discard,
            _ => DecisionType.Play,
        };
        var idvPath = Path.Combine(_tempDirectory, $"{name}_{suffix}{FileExtensions.Idv}");
        var metaPath = idvPath + FileExtensions.IdvMetadataSuffix;

        _sourceMetadataOverrides[metaPath] = new IdvFileMetadata(
            name,
            decisionType,
            RowCount: 100,
            GameCount: 1000,
            DealCount: 5000,
            TrickCount: 20000,
            actors,
            DateTime.UtcNow);
    }
}
