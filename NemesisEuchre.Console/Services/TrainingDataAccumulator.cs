using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.Console.Models;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.Console.Services;

public interface ITrainingDataAccumulator
{
    void Add(TrainingDataBatch batch);

    void SaveChunk(string generationName, bool allowOverwrite = false);

    Task FinalizeAsync(string generationName, Action<string>? onStatusUpdate = null, CancellationToken cancellationToken = default);
}

public class TrainingDataAccumulator(
    ITrainingDataBuffer buffer,
    IIdvChunkMerger merger,
    IIdvMetadataService metadataService,
    IOptions<PersistenceOptions> persistenceOptions,
    ILogger<TrainingDataAccumulator> logger) : ITrainingDataAccumulator
{
    private readonly HashSet<Actor> _actors = [];
    private readonly HashSet<string> _savedGenerationNames = [];
    private int _chunkIndex;
    private int _gameCount;
    private int _dealCount;
    private int _trickCount;

    public void Add(TrainingDataBatch batch)
    {
        buffer.Add(batch);

        _gameCount += batch.Stats.GameCount;
        _dealCount += batch.Stats.DealCount;
        _trickCount += batch.Stats.TrickCount;
        _actors.UnionWith(batch.Stats.Actors);
    }

    public void SaveChunk(string generationName, bool allowOverwrite = false)
    {
        var outputPath = persistenceOptions.Value.IdvOutputPath;

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        if (_chunkIndex == 0)
        {
            GuardAgainstOverwrite(outputPath, generationName, allowOverwrite);

            var chunkDir = GetChunkDirectory(outputPath, generationName);
            if (!Directory.Exists(chunkDir))
            {
                Directory.CreateDirectory(chunkDir);
            }
        }

        var chunkDirectory = GetChunkDirectory(outputPath, generationName);
        buffer.SaveChunk(chunkDirectory, _chunkIndex);

        _chunkIndex++;
    }

    public async Task FinalizeAsync(string generationName, Action<string>? onStatusUpdate = null, CancellationToken cancellationToken = default)
    {
        if (buffer.HasData())
        {
            onStatusUpdate?.Invoke("Saving remaining training data...");
            SaveChunk(generationName);
        }

        if (_chunkIndex == 0)
        {
            return;
        }

        var outputPath = persistenceOptions.Value.IdvOutputPath;
        var actorInfos = _actors.Select(a => new ActorInfo(a.ActorType, a.ModelName, a.ExplorationTemperature)).ToList();
        var (playCardPaths, callTrumpPaths, discardCardPaths) = buffer.GetChunkPaths();
        var (playCardRows, callTrumpRows, discardCardRows) = buffer.GetTotalRows();

        if (_chunkIndex == 1)
        {
            onStatusUpdate?.Invoke("Finalizing IDV files...");
            await Task.Run(
                () => Parallel.Invoke(
                    () => RenameChunkToFinal(playCardPaths, outputPath, generationName, "PlayCard", DecisionType.Play, playCardRows, actorInfos),
                    () => RenameChunkToFinal(callTrumpPaths, outputPath, generationName, "CallTrump", DecisionType.CallTrump, callTrumpRows, actorInfos),
                    () => RenameChunkToFinal(discardCardPaths, outputPath, generationName, "DiscardCard", DecisionType.Discard, discardCardRows, actorInfos)),
                cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var totalRows = playCardRows + callTrumpRows + discardCardRows;
            onStatusUpdate?.Invoke($"Merging {_chunkIndex} chunks ({totalRows:N0} rows)...");
            await Task.Run(
                () => Parallel.Invoke(
                    () => MergeChunksToFinal<PlayCardTrainingData>(playCardPaths, outputPath, generationName, "PlayCard", DecisionType.Play, playCardRows, actorInfos),
                    () => MergeChunksToFinal<CallTrumpTrainingData>(callTrumpPaths, outputPath, generationName, "CallTrump", DecisionType.CallTrump, callTrumpRows, actorInfos),
                    () => MergeChunksToFinal<DiscardCardTrainingData>(discardCardPaths, outputPath, generationName, "DiscardCard", DecisionType.Discard, discardCardRows, actorInfos)),
                cancellationToken).ConfigureAwait(false);
        }

        onStatusUpdate?.Invoke("Cleaning up temporary files...");
        var chunkDir = GetChunkDirectory(outputPath, generationName);
        merger.CleanupChunkDirectory(chunkDir);
        LoggerMessages.LogIdvChunkCleanup(logger, chunkDir);

        var parentChunkDir = Path.Combine(outputPath, "_chunks");
        if (Directory.Exists(parentChunkDir) && Directory.GetDirectories(parentChunkDir).Length == 0)
        {
            Directory.Delete(parentChunkDir);
        }

        _savedGenerationNames.Add(generationName);
    }

    private static string GetChunkDirectory(string outputPath, string generationName)
    {
        return Path.Combine(outputPath, "_chunks", generationName);
    }

    private void GuardAgainstOverwrite(string outputPath, string generationName, bool allowOverwrite)
    {
        if (allowOverwrite || _savedGenerationNames.Contains(generationName))
        {
            return;
        }

        string[] suffixes = ["PlayCard", "CallTrump", "DiscardCard"];

        var conflictingFiles = suffixes
            .SelectMany(s => new[]
            {
                Path.Combine(outputPath, $"{generationName}_{s}{FileExtensions.Idv}"),
                Path.Combine(outputPath, $"{generationName}_{s}{FileExtensions.IdvMetadata}"),
            })
            .Where(File.Exists)
            .ToList();

        if (conflictingFiles.Count > 0)
        {
            throw new InvalidOperationException(
                $"IDV files already exist and would be overwritten. Use --overwrite to replace them.{Environment.NewLine}" +
                string.Join(Environment.NewLine, conflictingFiles));
        }
    }

    private void RenameChunkToFinal(
        IReadOnlyList<string> chunkPaths,
        string outputPath,
        string generationName,
        string decisionName,
        DecisionType decisionType,
        int totalRows,
        List<ActorInfo> actorInfos)
    {
        var finalPath = Path.Combine(outputPath, $"{generationName}_{decisionName}{FileExtensions.Idv}");
        merger.RenameChunk(chunkPaths[0], finalPath, totalRows);

        SaveMetadata(finalPath, generationName, decisionType, totalRows, actorInfos);
    }

    private void MergeChunksToFinal<T>(
        IReadOnlyList<string> chunkPaths,
        string outputPath,
        string generationName,
        string decisionName,
        DecisionType decisionType,
        int totalRows,
        List<ActorInfo> actorInfos)
        where T : class, new()
    {
        var finalPath = Path.Combine(outputPath, $"{generationName}_{decisionName}{FileExtensions.Idv}");
        merger.MergeChunks<T>(chunkPaths, finalPath, totalRows);

        SaveMetadata(finalPath, generationName, decisionType, totalRows, actorInfos);
    }

    private void SaveMetadata(
        string filePath,
        string generationName,
        DecisionType decisionType,
        int rowCount,
        List<ActorInfo> actorInfos)
    {
        var metadata = new IdvFileMetadata(
            generationName,
            decisionType,
            rowCount,
            _gameCount,
            _dealCount,
            _trickCount,
            actorInfos,
            DateTime.UtcNow);

        metadataService.SaveMetadataWithVerification(filePath, metadata);
    }
}
