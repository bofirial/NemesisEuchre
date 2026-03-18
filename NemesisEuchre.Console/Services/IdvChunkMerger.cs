using System.Diagnostics.CodeAnalysis;

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
        ForceFinalization();
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

        ForceFinalization();

        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Directory.Delete(chunkDirectory, true);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                ForceFinalization();
                Thread.Sleep(attempt * 200);
            }
            catch (IOException ex)
            {
                LoggerMessages.LogIdvChunkCleanupFailed(logger, chunkDirectory, ex);
            }
        }
    }

    [SuppressMessage("Reliability", "S1215:GC.Collect should not be called", Justification = "ML.NET BinaryLoader releases file handles via GC finalization, not IDisposable")]
    private static void ForceFinalization()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private IEnumerable<T> StreamAllChunks<T>(IReadOnlyList<string> chunkPaths)
        where T : class, new()
    {
        foreach (var path in chunkPaths)
        {
            foreach (var row in idvFileService.StreamFromBinaryDetached<T>(path))
            {
                yield return row;
            }
        }
    }
}
