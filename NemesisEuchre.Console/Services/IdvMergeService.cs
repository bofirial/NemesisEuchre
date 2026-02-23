using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.Foundation;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.Console.Services;

public interface IIdvMergeService
{
    Task MergeAsync(
        IReadOnlyList<string> sourceGenerationNames,
        string outputGenerationName,
        bool allowOverwrite,
        Action<string>? onStatusUpdate = null,
        CancellationToken cancellationToken = default);
}

public sealed class IdvMergeService(
    IIdvFileService idvFileService,
    IIdvMetadataService metadataService,
    IOptions<PersistenceOptions> persistenceOptions,
    ILogger<IdvMergeService> logger) : IIdvMergeService
{
    private static readonly string[] DecisionSuffixes = ["PlayCard", "CallTrump", "DiscardCard"];

    public async Task MergeAsync(
        IReadOnlyList<string> sourceGenerationNames,
        string outputGenerationName,
        bool allowOverwrite,
        Action<string>? onStatusUpdate = null,
        CancellationToken cancellationToken = default)
    {
        var basePath = persistenceOptions.Value.IdvOutputPath;

        ValidateSourceFiles(basePath, sourceGenerationNames);
        GuardAgainstOverwrite(basePath, outputGenerationName, allowOverwrite);

        onStatusUpdate?.Invoke($"Loading metadata from {sourceGenerationNames.Count} source(s)...");
        var allSourceMetadata = LoadAllSourceMetadata(basePath, sourceGenerationNames);

        var (gameCount, dealCount, trickCount, actors) = AggregateMetadata(allSourceMetadata);

        onStatusUpdate?.Invoke($"Merging {sourceGenerationNames.Count} source(s) into '{outputGenerationName}'...");
        LoggerMessages.LogIdvMergeSourcesStarting(logger, sourceGenerationNames.Count, outputGenerationName);

        var playCardPaths = GetSourcePaths(basePath, sourceGenerationNames, "PlayCard");
        var callTrumpPaths = GetSourcePaths(basePath, sourceGenerationNames, "CallTrump");
        var discardCardPaths = GetSourcePaths(basePath, sourceGenerationNames, "DiscardCard");

        await Task.Run(
            () => Parallel.Invoke(
                () => MergeDecisionType<PlayCardTrainingData>(
                    playCardPaths,
                    basePath,
                    outputGenerationName,
                    "PlayCard",
                    DecisionType.Play,
                    allSourceMetadata,
                    gameCount,
                    dealCount,
                    trickCount,
                    actors),
                () => MergeDecisionType<CallTrumpTrainingData>(
                    callTrumpPaths,
                    basePath,
                    outputGenerationName,
                    "CallTrump",
                    DecisionType.CallTrump,
                    allSourceMetadata,
                    gameCount,
                    dealCount,
                    trickCount,
                    actors),
                () => MergeDecisionType<DiscardCardTrainingData>(
                    discardCardPaths,
                    basePath,
                    outputGenerationName,
                    "DiscardCard",
                    DecisionType.Discard,
                    allSourceMetadata,
                    gameCount,
                    dealCount,
                    trickCount,
                    actors)),
            cancellationToken).ConfigureAwait(false);

        onStatusUpdate?.Invoke($"Merge complete. Output: '{outputGenerationName}'");
    }

    private static void ValidateSourceFiles(string basePath, IReadOnlyList<string> sourceNames)
    {
        foreach (var name in sourceNames)
        {
            foreach (var suffix in DecisionSuffixes)
            {
                var path = Path.Combine(basePath, $"{name}_{suffix}{FileExtensions.Idv}");
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Source IDV file not found: {path}", path);
                }
            }
        }
    }

    private static void GuardAgainstOverwrite(string basePath, string outputName, bool allowOverwrite)
    {
        if (allowOverwrite)
        {
            return;
        }

        var conflictingFiles = DecisionSuffixes
            .SelectMany(s => new[]
            {
                Path.Combine(basePath, $"{outputName}_{s}{FileExtensions.Idv}"),
                Path.Combine(basePath, $"{outputName}_{s}{FileExtensions.IdvMetadata}"),
            })
            .Where(File.Exists)
            .ToList();

        if (conflictingFiles.Count > 0)
        {
            throw new InvalidOperationException(
                $"Output IDV files already exist. Use --overwrite to replace them.{Environment.NewLine}" +
                string.Join(Environment.NewLine, conflictingFiles));
        }
    }

    private static (int gameCount, int dealCount, int trickCount, List<ActorInfo> actors) AggregateMetadata(
        List<IdvFileMetadata> allMetadata)
    {
        var gameCount = allMetadata
            .Where(m => m.DecisionType == DecisionType.Play)
            .Sum(m => m.GameCount);
        var dealCount = allMetadata
            .Where(m => m.DecisionType == DecisionType.Play)
            .Sum(m => m.DealCount);
        var trickCount = allMetadata
            .Where(m => m.DecisionType == DecisionType.Play)
            .Sum(m => m.TrickCount);
        var actors = allMetadata
            .SelectMany(m => m.Actors)
            .DistinctBy(a => (actorType: a.ActorType, modelName: a.ModelName, explorationTemperature: a.ExplorationTemperature))
            .ToList();

        return (gameCount, dealCount, trickCount, actors);
    }

    private static List<string> GetSourcePaths(
        string basePath,
        IReadOnlyList<string> sourceNames,
        string suffix)
    {
        return [.. sourceNames.Select(n => Path.Combine(basePath, $"{n}_{suffix}{FileExtensions.Idv}"))];
    }

    private List<IdvFileMetadata> LoadAllSourceMetadata(string basePath, IReadOnlyList<string> sourceNames)
    {
        var metadata = new List<IdvFileMetadata>();

        foreach (var name in sourceNames)
        {
            foreach (var suffix in DecisionSuffixes)
            {
                var idvPath = Path.Combine(basePath, $"{name}_{suffix}{FileExtensions.Idv}");
                var metaPath = idvPath + FileExtensions.IdvMetadataSuffix;
                metadata.Add(idvFileService.LoadMetadata(metaPath));
            }
        }

        return metadata;
    }

    private void MergeDecisionType<T>(
        IReadOnlyList<string> sourcePaths,
        string basePath,
        string outputName,
        string suffix,
        DecisionType decisionType,
        List<IdvFileMetadata> allMetadata,
        int gameCount,
        int dealCount,
        int trickCount,
        List<ActorInfo> actors)
        where T : class, new()
    {
        var outputPath = Path.Combine(basePath, $"{outputName}_{suffix}{FileExtensions.Idv}");
        var rowCount = allMetadata
            .Where(m => m.DecisionType == decisionType)
            .Sum(m => m.RowCount);

        idvFileService.Save(InterleaveAllSources<T>(sourcePaths), outputPath);

        var metadata = new IdvFileMetadata(
            outputName,
            decisionType,
            rowCount,
            gameCount,
            dealCount,
            trickCount,
            actors,
            DateTime.UtcNow);

        metadataService.SaveMetadataWithVerification(outputPath, metadata);
        LoggerMessages.LogIdvFileSaved(logger, outputPath, rowCount);
    }

    private IEnumerable<T> InterleaveAllSources<T>(IReadOnlyList<string> sourcePaths)
        where T : class, new()
    {
        var enumerators = sourcePaths
            .Select(p => idvFileService.StreamFromBinary<T>(p).GetEnumerator())
            .ToList();

        try
        {
            var active = new List<IEnumerator<T>>(enumerators);
            while (active.Count > 0)
            {
                for (var i = active.Count - 1; i >= 0; i--)
                {
                    if (active[i].MoveNext())
                    {
                        yield return active[i].Current;
                    }
                    else
                    {
                        active[i].Dispose();
                        active.RemoveAt(i);
                    }
                }
            }
        }
        finally
        {
            foreach (var enumerator in enumerators)
            {
                enumerator.Dispose();
            }
        }
    }
}
