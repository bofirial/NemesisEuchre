using Microsoft.Extensions.Logging;
using Microsoft.ML;

using NemesisEuchre.Foundation;
using NemesisEuchre.MachineLearning.Caching;

namespace NemesisEuchre.MachineLearning.Loading;

public interface IModelLoader
{
    PredictionEngine<TData, TPrediction> LoadModel<TData, TPrediction>(
        string modelsDirectory,
        string modelName,
        string decisionType)
        where TData : class
        where TPrediction : class, new();

    void InvalidateCache(string modelPath);

    void InvalidateAll();
}

public class ModelLoader(
    IModelCache modelCache,
    ILogger<ModelLoader> logger) : IModelLoader
{
    public PredictionEngine<TData, TPrediction> LoadModel<TData, TPrediction>(
        string modelsDirectory,
        string modelName,
        string decisionType)
        where TData : class
        where TPrediction : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelsDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelName);
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionType);

        var normalizedDecisionType = decisionType.ToLowerInvariant();
        var fileName = $"{modelName}_{normalizedDecisionType}.zip";
        var modelFilePath = Path.Combine(modelsDirectory, fileName);

        LoggerMessages.LogLoadingModelWithDecisionType(logger, modelName, decisionType);

        return modelCache.GetOrCreatePredictionEngine<TData, TPrediction>(modelFilePath);
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
