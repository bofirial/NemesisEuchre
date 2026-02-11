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

    void Save(string generationName, bool allowOverwrite = false);
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

    public void Save(string generationName, bool allowOverwrite = false)
    {
        var outputPath = persistenceOptions.Value.IdvOutputPath;

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        string[] suffixes = ["PlayCard", "CallTrump", "DiscardCard"];

        if (!allowOverwrite && !_savedGenerationNames.Contains(generationName))
        {
            var conflictingFiles = suffixes
                .SelectMany(s => new[]
                {
                    Path.Combine(outputPath, $"{generationName}_{s}.idv"),
                    Path.Combine(outputPath, $"{generationName}_{s}.idv.meta.json"),
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

        var actorInfos = _actors.Select(a => new ActorInfo(a.ActorType, a.ModelName, a.ExplorationTemperature)).ToList();

        SaveIdvFileWithMetadata(
            _playCardData,
            Path.Combine(outputPath, $"{generationName}_PlayCard.idv"),
            generationName,
            DecisionType.Play,
            actorInfos);

        SaveIdvFileWithMetadata(
            _callTrumpData,
            Path.Combine(outputPath, $"{generationName}_CallTrump.idv"),
            generationName,
            DecisionType.CallTrump,
            actorInfos);

        SaveIdvFileWithMetadata(
            _discardCardData,
            Path.Combine(outputPath, $"{generationName}_DiscardCard.idv"),
            generationName,
            DecisionType.Discard,
            actorInfos);

        _savedGenerationNames.Add(generationName);
    }

    private void SaveIdvFileWithMetadata<T>(
        List<T> data,
        string filePath,
        string generationName,
        DecisionType decisionType,
        List<ActorInfo> actorInfos)
        where T : class
    {
        LoggerMessages.LogIdvFileSaving(logger, filePath, data.Count);
        idvFileService.Save(data, filePath);
        LoggerMessages.LogIdvFileSaved(logger, filePath, data.Count);

        var metadataPath = $"{filePath}.meta.json";
        var metadata = new IdvFileMetadata(
            generationName,
            decisionType,
            data.Count,
            _gameCount,
            _dealCount,
            _trickCount,
            actorInfos,
            DateTime.UtcNow);

        idvFileService.SaveMetadata(metadata, metadataPath);
        LoggerMessages.LogIdvMetadataSaved(logger, metadataPath);

        var readBack = idvFileService.LoadMetadata(metadataPath);
        if (readBack.RowCount != data.Count)
        {
            LoggerMessages.LogIdvMetadataVerificationFailed(logger, metadataPath);
            throw new InvalidOperationException(
                $"IDV metadata verification failed for {metadataPath}: expected {data.Count} rows but read back {readBack.RowCount}");
        }
    }
}
