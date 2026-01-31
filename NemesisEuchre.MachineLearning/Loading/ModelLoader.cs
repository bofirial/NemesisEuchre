using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

using NemesisEuchre.DataAccess.Configuration;
using NemesisEuchre.MachineLearning.Caching;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Services;

namespace NemesisEuchre.MachineLearning.Loading;

public interface IModelLoader
{
    PredictionEngine<TData, TPrediction> LoadModel<TData, TPrediction>(
        string modelsDirectory,
        int generation,
        string decisionType,
        int? version = null)
        where TData : class
        where TPrediction : class, new();

    ModelMetadata LoadMetadata(
        string modelsDirectory,
        int generation,
        string decisionType,
        int? version = null);

    void InvalidateCache(string modelPath);

    void InvalidateAll();
}

public class ModelLoader(
    IModelCache modelCache,
    IModelVersionManager versionManager,
    ILogger<ModelLoader> logger) : IModelLoader
{
    public PredictionEngine<TData, TPrediction> LoadModel<TData, TPrediction>(
        string modelsDirectory,
        int generation,
        string decisionType,
        int? version = null)
        where TData : class
        where TPrediction : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelsDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionType);

        if (generation < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(generation), "Generation must be at least 1");
        }

        ModelFileInfo modelInfo;

        if (version.HasValue)
        {
            if (version.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(version), "Version must be at least 1");
            }

            var modelPath = versionManager.GetModelPath(modelsDirectory, generation, decisionType, version.Value);
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException(
                    $"Model file not found: gen{generation}_{decisionType}_v{version}.zip",
                    modelPath);
            }

            var metadataPath = Path.ChangeExtension(modelPath, ".json");
            modelInfo = new ModelFileInfo(modelPath, metadataPath, generation, decisionType, version.Value);
        }
        else
        {
            modelInfo = versionManager.GetLatestModel(modelsDirectory, generation, decisionType)
                ?? throw new FileNotFoundException($"No models found for gen{generation} {decisionType}");
        }

        LoggerMessages.LogLoadingModelWithVersion(logger, modelInfo.Generation, modelInfo.DecisionType, modelInfo.Version);

        return modelCache.GetOrCreatePredictionEngine<TData, TPrediction>(modelInfo.FilePath);
    }

    public ModelMetadata LoadMetadata(
        string modelsDirectory,
        int generation,
        string decisionType,
        int? version = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelsDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionType);

        if (generation < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(generation), "Generation must be at least 1");
        }

        ModelFileInfo modelInfo;

        if (version.HasValue)
        {
            if (version.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(version), "Version must be at least 1");
            }

            var modelPath = versionManager.GetModelPath(modelsDirectory, generation, decisionType, version.Value);
            var metadataPath = Path.ChangeExtension(modelPath, ".json");

            if (!File.Exists(metadataPath))
            {
                throw new FileNotFoundException(
                    $"Metadata file not found: gen{generation}_{decisionType}_v{version}.json",
                    metadataPath);
            }

            modelInfo = new ModelFileInfo(modelPath, metadataPath, generation, decisionType, version.Value);
        }
        else
        {
            modelInfo = versionManager.GetLatestModel(modelsDirectory, generation, decisionType)
                ?? throw new FileNotFoundException($"No models found for gen{generation} {decisionType}");
        }

        var json = File.ReadAllText(modelInfo.MetadataPath);
        return JsonSerializer.Deserialize<ModelMetadata>(json, JsonSerializationOptions.Default)
            ?? throw new InvalidOperationException($"Failed to deserialize metadata from {modelInfo.MetadataPath}");
    }

    public void InvalidateCache(string modelPath)
    {
        modelCache.InvalidateCache(modelPath);
    }

    public void InvalidateAll()
    {
        modelCache.InvalidateAll();
    }
}
