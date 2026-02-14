using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Models;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.Console.Services;

public interface ITrainingDataBuffer
{
    void Add(TrainingDataBatch batch);

    void SaveChunk(string chunkDirectory, int chunkIndex);

    bool HasData();

    (int playCard, int callTrump, int discardCard) GetTotalRows();

    (IReadOnlyList<string> playCard, IReadOnlyList<string> callTrump, IReadOnlyList<string> discardCard) GetChunkPaths();
}

public sealed class TrainingDataBuffer(
    IIdvFileService idvFileService,
    ILogger<TrainingDataBuffer> logger) : ITrainingDataBuffer
{
    private readonly DecisionTypeAccumulator<PlayCardTrainingData> _playCardAccumulator
        = new("PlayCard", idvFileService, logger);

    private readonly DecisionTypeAccumulator<CallTrumpTrainingData> _callTrumpAccumulator
        = new("CallTrump", idvFileService, logger);

    private readonly DecisionTypeAccumulator<DiscardCardTrainingData> _discardCardAccumulator
        = new("DiscardCard", idvFileService, logger);

    public void Add(TrainingDataBatch batch)
    {
        _playCardAccumulator.AddRange(batch.PlayCardData);
        _callTrumpAccumulator.AddRange(batch.CallTrumpData);
        _discardCardAccumulator.AddRange(batch.DiscardCardData);
    }

    public void SaveChunk(string chunkDirectory, int chunkIndex)
    {
        Parallel.Invoke(
            () => _playCardAccumulator.SaveChunk(chunkDirectory, chunkIndex),
            () => _callTrumpAccumulator.SaveChunk(chunkDirectory, chunkIndex),
            () => _discardCardAccumulator.SaveChunk(chunkDirectory, chunkIndex));
    }

    public bool HasData()
    {
        return _playCardAccumulator.Count > 0
            || _callTrumpAccumulator.Count > 0
            || _discardCardAccumulator.Count > 0;
    }

    public (int playCard, int callTrump, int discardCard) GetTotalRows()
    {
        return (playCard: _playCardAccumulator.TotalRows, callTrump: _callTrumpAccumulator.TotalRows, discardCard: _discardCardAccumulator.TotalRows);
    }

    public (IReadOnlyList<string> playCard, IReadOnlyList<string> callTrump, IReadOnlyList<string> discardCard) GetChunkPaths()
    {
        return (playCard: _playCardAccumulator.ChunkPaths, callTrump: _callTrumpAccumulator.ChunkPaths, discardCard: _discardCardAccumulator.ChunkPaths);
    }
}
