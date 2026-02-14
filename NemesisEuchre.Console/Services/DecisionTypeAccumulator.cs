using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.Console.Services;

internal sealed class DecisionTypeAccumulator<T>(
    string decisionName,
    IIdvFileService idvFileService,
    ILogger logger)
    where T : class
{
    private readonly List<T> _data = [];
    private readonly List<string> _chunkPaths = [];

    public int Count => _data.Count;

    public int TotalRows { get; private set; }

    public IReadOnlyList<string> ChunkPaths => _chunkPaths;

    public void AddRange(IEnumerable<T> items)
    {
        _data.AddRange(items);
    }

    public void SaveChunk(string chunkDirectory, int chunkIndex)
    {
        var chunkSuffix = $"_chunk{chunkIndex + 1:D4}";
        var chunkPath = Path.Combine(chunkDirectory, decisionName + chunkSuffix + FileExtensions.Idv);

        LoggerMessages.LogIdvChunkSaving(logger, chunkIndex + 1, chunkPath, _data.Count);
        idvFileService.Save(_data, chunkPath);

        _chunkPaths.Add(chunkPath);
        TotalRows += _data.Count;
        _data.Clear();
    }

    public void Clear()
    {
        _data.Clear();
    }
}
