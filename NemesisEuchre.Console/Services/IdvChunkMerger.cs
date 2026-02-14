using Microsoft.Extensions.Logging;

using NemesisEuchre.Foundation;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.Console.Services;

public interface IIdvChunkMerger
{
    void MergeChunks<T>(IReadOnlyList<string> chunkPaths, string finalPath, int totalRows)
        where T : class, new();

    void RenameChunk(string chunkPath, string finalPath, int totalRows);

    void CleanupChunkDirectory(string chunkDirectory);
}

public sealed class IdvChunkMerger(
    IIdvFileService idvFileService,
    ILogger<IdvChunkMerger> logger) : IIdvChunkMerger
{
    public void MergeChunks<T>(IReadOnlyList<string> chunkPaths, string finalPath, int totalRows)
        where T : class, new()
    {
        LoggerMessages.LogIdvChunkMerging(logger, chunkPaths.Count, finalPath);

        idvFileService.Save(StreamAllChunks<T>(chunkPaths), finalPath);
        LoggerMessages.LogIdvMergeComplete(logger, finalPath, totalRows, chunkPaths.Count);
    }

    public void RenameChunk(string chunkPath, string finalPath, int totalRows)
    {
        File.Move(chunkPath, finalPath, overwrite: true);
        LoggerMessages.LogIdvFileSaved(logger, finalPath, totalRows);
    }

    public void CleanupChunkDirectory(string chunkDirectory)
    {
        if (!Directory.Exists(chunkDirectory))
        {
            return;
        }

        try
        {
            Directory.Delete(chunkDirectory, true);
        }
        catch (IOException)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(100);
            Directory.Delete(chunkDirectory, true);
        }
    }

    private IEnumerable<T> StreamAllChunks<T>(IReadOnlyList<string> chunkPaths)
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
}
