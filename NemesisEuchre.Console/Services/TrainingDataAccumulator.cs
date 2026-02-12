using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.Console.Models;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.Console.Services;

public interface ITrainingDataAccumulator
{
    void Add(TrainingDataBatch batch);

    void SaveChunk(string generationName, bool allowOverwrite = false);

    void Finalize(string generationName);
}

public class TrainingDataAccumulator(
    IIdvFileService idvFileService,
    IOptions<PersistenceOptions> persistenceOptions,
    ILogger<TrainingDataAccumulator> logger) : ITrainingDataAccumulator
{
    private readonly List<PlayCardTrainingData> _playCardData = [];
    private readonly List<CallTrumpTrainingData> _callTrumpData = [];
    private readonly List<DiscardCardTrainingData> _discardCardData = [];
    private readonly HashSet<Actor> _actors = [];
    private readonly HashSet<string> _savedGenerationNames = [];
    private readonly List<string> _playCardChunkPaths = [];
    private readonly List<string> _callTrumpChunkPaths = [];
    private readonly List<string> _discardCardChunkPaths = [];
    private int _chunkIndex;
    private int _totalPlayCardRows;
    private int _totalCallTrumpRows;
    private int _totalDiscardCardRows;
    private int _gameCount;
    private int _dealCount;
    private int _trickCount;

    public void Add(TrainingDataBatch batch)
    {
        _playCardData.AddRange(batch.PlayCardData);
        _callTrumpData.AddRange(batch.CallTrumpData);
        _discardCardData.AddRange(batch.DiscardCardData);

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
        var chunkSuffix = $"_chunk{_chunkIndex + 1:D4}";

        SaveChunkFile(_playCardData, chunkDirectory, "PlayCard", chunkSuffix, _playCardChunkPaths);
        SaveChunkFile(_callTrumpData, chunkDirectory, "CallTrump", chunkSuffix, _callTrumpChunkPaths);
        SaveChunkFile(_discardCardData, chunkDirectory, "DiscardCard", chunkSuffix, _discardCardChunkPaths);

        _totalPlayCardRows += _playCardData.Count;
        _totalCallTrumpRows += _callTrumpData.Count;
        _totalDiscardCardRows += _discardCardData.Count;

        _playCardData.Clear();
        _callTrumpData.Clear();
        _discardCardData.Clear();

        _chunkIndex++;
    }

    public void Finalize(string generationName)
    {
        if (_playCardData.Count > 0 || _callTrumpData.Count > 0 || _discardCardData.Count > 0)
        {
            SaveChunk(generationName);
        }

        if (_chunkIndex == 0)
        {
            return;
        }

        var outputPath = persistenceOptions.Value.IdvOutputPath;
        var actorInfos = _actors.Select(a => new ActorInfo(a.ActorType, a.ModelName, a.ExplorationTemperature)).ToList();

        if (_chunkIndex == 1)
        {
            RenameChunkToFinal(_playCardChunkPaths, outputPath, generationName, "PlayCard", DecisionType.Play, _totalPlayCardRows, actorInfos);
            RenameChunkToFinal(_callTrumpChunkPaths, outputPath, generationName, "CallTrump", DecisionType.CallTrump, _totalCallTrumpRows, actorInfos);
            RenameChunkToFinal(_discardCardChunkPaths, outputPath, generationName, "DiscardCard", DecisionType.Discard, _totalDiscardCardRows, actorInfos);
        }
        else
        {
            MergeChunksToFinal<PlayCardTrainingData>(_playCardChunkPaths, outputPath, generationName, "PlayCard", DecisionType.Play, _totalPlayCardRows, actorInfos);
            MergeChunksToFinal<CallTrumpTrainingData>(_callTrumpChunkPaths, outputPath, generationName, "CallTrump", DecisionType.CallTrump, _totalCallTrumpRows, actorInfos);
            MergeChunksToFinal<DiscardCardTrainingData>(_discardCardChunkPaths, outputPath, generationName, "DiscardCard", DecisionType.Discard, _totalDiscardCardRows, actorInfos);
        }

        var chunkDir = GetChunkDirectory(outputPath, generationName);
        if (Directory.Exists(chunkDir))
        {
            Directory.Delete(chunkDir, true);
            LoggerMessages.LogIdvChunkCleanup(logger, chunkDir);
        }

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

    private void SaveChunkFile<T>(
        List<T> data,
        string chunkDirectory,
        string decisionName,
        string chunkSuffix,
        List<string> chunkPaths)
        where T : class
    {
        var chunkPath = Path.Combine(chunkDirectory, decisionName + chunkSuffix + FileExtensions.Idv);
        LoggerMessages.LogIdvChunkSaving(logger, _chunkIndex + 1, chunkPath, data.Count);
        idvFileService.Save(data, chunkPath);
        chunkPaths.Add(chunkPath);
    }

    private void RenameChunkToFinal(
        List<string> chunkPaths,
        string outputPath,
        string generationName,
        string decisionName,
        DecisionType decisionType,
        int totalRows,
        List<ActorInfo> actorInfos)
    {
        var finalPath = Path.Combine(outputPath, $"{generationName}_{decisionName}{FileExtensions.Idv}");
        File.Move(chunkPaths[0], finalPath, overwrite: true);
        LoggerMessages.LogIdvFileSaved(logger, finalPath, totalRows);

        SaveMetadataWithVerification(finalPath, generationName, decisionType, totalRows, actorInfos);
    }

    private void MergeChunksToFinal<T>(
        List<string> chunkPaths,
        string outputPath,
        string generationName,
        string decisionName,
        DecisionType decisionType,
        int totalRows,
        List<ActorInfo> actorInfos)
        where T : class, new()
    {
        var finalPath = Path.Combine(outputPath, $"{generationName}_{decisionName}{FileExtensions.Idv}");
        LoggerMessages.LogIdvChunkMerging(logger, chunkPaths.Count, finalPath);

        idvFileService.Save(StreamAllChunks<T>(chunkPaths), finalPath);
        LoggerMessages.LogIdvMergeComplete(logger, finalPath, totalRows, chunkPaths.Count);

        SaveMetadataWithVerification(finalPath, generationName, decisionType, totalRows, actorInfos);
    }

    private IEnumerable<T> StreamAllChunks<T>(List<string> chunkPaths)
        where T : class, new()
    {
        foreach (var path in chunkPaths)
        {
            foreach (var row in idvFileService.StreamFromBinary<T>(path))
            {
                yield return row;
            }
        }
    }

    private void SaveMetadataWithVerification(
        string filePath,
        string generationName,
        DecisionType decisionType,
        int rowCount,
        List<ActorInfo> actorInfos)
    {
        var metadataPath = filePath + FileExtensions.IdvMetadataSuffix;
        var metadata = new IdvFileMetadata(
            generationName,
            decisionType,
            rowCount,
            _gameCount,
            _dealCount,
            _trickCount,
            actorInfos,
            DateTime.UtcNow);

        idvFileService.SaveMetadata(metadata, metadataPath);
        LoggerMessages.LogIdvMetadataSaved(logger, metadataPath);

        var readBack = idvFileService.LoadMetadata(metadataPath);
        if (readBack.RowCount != rowCount)
        {
            LoggerMessages.LogIdvMetadataVerificationFailed(logger, metadataPath);
            throw new InvalidOperationException(
                $"IDV metadata verification failed for {metadataPath}: expected {rowCount} rows but read back {readBack.RowCount}");
        }
    }
}
