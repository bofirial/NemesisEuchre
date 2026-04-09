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

    PredictionEngine<TData, TPrediction> CreateNewPredictionEngine<TData, TPrediction>(
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
    IModelFileProvider modelFileProvider,
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

        var modelFilePath = modelFileProvider.EnsureModelFile(modelsDirectory, modelName, decisionType);

        LoggerMessages.LogLoadingModelWithDecisionType(logger, modelName, decisionType);

        return modelCache.GetOrCreatePredictionEngine<TData, TPrediction>(modelFilePath);
    }

    public PredictionEngine<TData, TPrediction> CreateNewPredictionEngine<TData, TPrediction>(
        string modelsDirectory,
        string modelName,
        string decisionType)
        where TData : class
        where TPrediction : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelsDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelName);
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionType);

        var modelFilePath = modelFileProvider.EnsureModelFile(modelsDirectory, modelName, decisionType);

        return modelCache.CreateNewPredictionEngine<TData, TPrediction>(modelFilePath);
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
