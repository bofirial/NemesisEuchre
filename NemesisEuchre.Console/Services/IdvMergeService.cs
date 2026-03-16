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
        DecisionType decisionTypeFilter = DecisionType.All,
        Action<string>? onStatusUpdate = null,
        CancellationToken cancellationToken = default);
}

public sealed class IdvMergeService(
    IIdvFileService idvFileService,
    IIdvMetadataService metadataService,
    IOptions<PersistenceOptions> persistenceOptions,
    ILogger<IdvMergeService> logger) : IIdvMergeService
{
    private static readonly (string suffix, DecisionType type, Action<IdvMergeService, MergeOperationContext> merge)[] DecisionMergeMap =
    [
        (suffix: "PlayCard", type: DecisionType.Play, merge: (svc, ctx) => svc.MergeDecisionType<AllPlayCardTrainingData>(ctx, "PlayCard", DecisionType.Play)),
        (suffix: "CallTrump", type: DecisionType.CallTrump, merge: (svc, ctx) => svc.MergeDecisionType<CallTrumpTrainingData>(ctx, "CallTrump", DecisionType.CallTrump)),
        (suffix: "DiscardCard", type: DecisionType.Discard, merge: (svc, ctx) => svc.MergeDecisionType<DiscardCardTrainingData>(ctx, "DiscardCard", DecisionType.Discard)),
    ];

    private static readonly (string suffix, DecisionType type)[] DecisionSuffixMap =
    [
        (suffix: "PlayCard", type: DecisionType.Play),
        (suffix: "CallTrump", type: DecisionType.CallTrump),
        (suffix: "DiscardCard", type: DecisionType.Discard),
    ];

    public async Task MergeAsync(
        IReadOnlyList<string> sourceGenerationNames,
        string outputGenerationName,
        bool allowOverwrite,
        DecisionType decisionTypeFilter = DecisionType.All,
        Action<string>? onStatusUpdate = null,
        CancellationToken cancellationToken = default)
    {
        var basePath = persistenceOptions.Value.IdvOutputPath;
        var activeSuffixes = GetActiveSuffixes(decisionTypeFilter);

        ValidateSourceFiles(basePath, sourceGenerationNames, activeSuffixes);
        GuardAgainstOverwrite(basePath, outputGenerationName, allowOverwrite, activeSuffixes);

        onStatusUpdate?.Invoke($"Loading metadata from {sourceGenerationNames.Count} source(s)...");
        var allSourceMetadata = LoadAllSourceMetadata(basePath, sourceGenerationNames, activeSuffixes);

        var (gameCount, dealCount, trickCount, actors) = AggregateMetadata(allSourceMetadata);

        onStatusUpdate?.Invoke($"Merging {sourceGenerationNames.Count} source(s) into '{outputGenerationName}'...");
        LoggerMessages.LogIdvMergeSourcesStarting(logger, sourceGenerationNames.Count, outputGenerationName);

        var mergeContext = new MergeOperationContext(
            basePath,
            outputGenerationName,
            sourceGenerationNames,
            allSourceMetadata,
            gameCount,
            dealCount,
            trickCount,
            actors);

        await Task.Run(
            () =>
            {
                foreach (var (suffix, type, merge) in DecisionMergeMap)
                {
                    if (ShouldMerge(decisionTypeFilter, type))
                    {
                        merge(this, mergeContext);
                    }
                }
            },
            cancellationToken).ConfigureAwait(false);

        onStatusUpdate?.Invoke($"Merge complete. Output: '{outputGenerationName}'");
    }

    private static bool ShouldMerge(DecisionType filter, DecisionType candidate)
    {
        return filter == DecisionType.All || filter == candidate;
    }

    private static string[] GetActiveSuffixes(DecisionType filter)
    {
        return [.. DecisionSuffixMap.Where(e => ShouldMerge(filter, e.type)).Select(e => e.suffix)];
    }

    private static void ValidateSourceFiles(
        string basePath,
        IReadOnlyList<string> sourceNames,
        string[] activeSuffixes)
    {
        foreach (var name in sourceNames)
        {
            foreach (var suffix in activeSuffixes)
            {
                var path = Path.Combine(basePath, $"{name}_{suffix}{FileExtensions.Idv}");
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Source IDV file not found: {path}", path);
                }
            }
        }
    }

    private static void GuardAgainstOverwrite(
        string basePath,
        string outputName,
        bool allowOverwrite,
        string[] activeSuffixes)
    {
        if (allowOverwrite)
        {
            return;
        }

        var conflictingFiles = activeSuffixes
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

    private List<IdvFileMetadata> LoadAllSourceMetadata(
        string basePath,
        IReadOnlyList<string> sourceNames,
        string[] activeSuffixes)
    {
        var metadata = new List<IdvFileMetadata>();

        foreach (var name in sourceNames)
        {
            foreach (var suffix in activeSuffixes)
            {
                var idvPath = Path.Combine(basePath, $"{name}_{suffix}{FileExtensions.Idv}");
                var metaPath = idvPath + FileExtensions.IdvMetadataSuffix;
                metadata.Add(idvFileService.LoadMetadata(metaPath));
            }
        }

        return metadata;
    }

    private void MergeDecisionType<T>(
        MergeOperationContext context,
        string suffix,
        DecisionType decisionType)
        where T : class, new()
    {
        var sourcePaths = GetSourcePaths(context.BasePath, context.SourceNames, suffix);
        var outputPath = Path.Combine(context.BasePath, $"{context.OutputName}_{suffix}{FileExtensions.Idv}");
        var rowCount = context.AllMetadata
            .Where(m => m.DecisionType == decisionType)
            .Sum(m => m.RowCount);

        idvFileService.Save(InterleaveAllSources<T>(sourcePaths), outputPath);

        var metadata = new IdvFileMetadata(
            context.OutputName,
            decisionType,
            rowCount,
            context.GameCount,
            context.DealCount,
            context.TrickCount,
            context.Actors,
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

    private record MergeOperationContext(
        string BasePath,
        string OutputName,
        IReadOnlyList<string> SourceNames,
        List<IdvFileMetadata> AllMetadata,
        int GameCount,
        int DealCount,
        int TrickCount,
        List<ActorInfo> Actors);
}
