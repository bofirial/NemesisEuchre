using System.Text.RegularExpressions;

namespace NemesisEuchre.MachineLearning.Services;

public interface IModelVersionManager
{
    string GetModelPath(string modelsDirectory, int generation, string decisionType, int version);

    int GetNextVersion(string modelsDirectory, int generation, string decisionType);

    ModelFileInfo? GetLatestModel(string modelsDirectory, int generation, string decisionType);

    IEnumerable<ModelFileInfo> GetAllModels(string modelsDirectory, int? generation = null, string? decisionType = null);
}

public partial class ModelVersionManager : IModelVersionManager
{
    public string GetModelPath(string modelsDirectory, int generation, string decisionType, int version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelsDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionType);

        if (generation < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(generation), "Generation must be at least 1");
        }

        if (version < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(version), "Version must be at least 1");
        }

        var normalizedDecisionType = decisionType.ToLowerInvariant();
        var fileName = $"gen{generation}_{normalizedDecisionType}_v{version}.zip";
        return Path.Combine(modelsDirectory, fileName);
    }

    public int GetNextVersion(string modelsDirectory, int generation, string decisionType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelsDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionType);

        if (generation < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(generation), "Generation must be at least 1");
        }

        if (!Directory.Exists(modelsDirectory))
        {
            return 1;
        }

        var existingModels = GetAllModels(modelsDirectory, generation, decisionType);
        var maxVersion = existingModels.MaxBy(m => m.Version)?.Version ?? 0;

        return maxVersion + 1;
    }

    public ModelFileInfo? GetLatestModel(string modelsDirectory, int generation, string decisionType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelsDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionType);

        if (generation < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(generation), "Generation must be at least 1");
        }

        if (!Directory.Exists(modelsDirectory))
        {
            return null;
        }

        var models = GetAllModels(modelsDirectory, generation, decisionType);
        return models.MaxBy(m => m.Version);
    }

    public IEnumerable<ModelFileInfo> GetAllModels(string modelsDirectory, int? generation = null, string? decisionType = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelsDirectory);

        if (generation < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(generation), "Generation must be at least 1");
        }

        if (!Directory.Exists(modelsDirectory))
        {
            return [];
        }

        var files = Directory.GetFiles(modelsDirectory, "*.zip");
        var regex = ModelFileRegex();
        var normalizedDecisionType = decisionType?.ToLowerInvariant();

        var models = new List<ModelFileInfo>();

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var match = regex.Match(fileName);

            if (!match.Success)
            {
                continue;
            }

            var fileGeneration = int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
            var fileDecisionType = match.Groups[2].Value.ToLowerInvariant();
            var fileVersion = int.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);

            if (generation.HasValue && fileGeneration != generation.Value)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(normalizedDecisionType) && fileDecisionType != normalizedDecisionType)
            {
                continue;
            }

            var metadataPath = Path.Combine(
                modelsDirectory,
                $"gen{fileGeneration}_{fileDecisionType}_v{fileVersion}.json");

            models.Add(new ModelFileInfo(
                file,
                metadataPath,
                fileGeneration,
                fileDecisionType,
                fileVersion));
        }

        return models;
    }

    [GeneratedRegex(@"gen(\d+)_([a-z]+)_v(\d+)\.zip", RegexOptions.IgnoreCase)]
    private static partial Regex ModelFileRegex();
}
