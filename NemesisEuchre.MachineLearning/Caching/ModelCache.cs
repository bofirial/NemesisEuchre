using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace NemesisEuchre.MachineLearning.Caching;

public interface IModelCache
{
    PredictionEngine<TData, TPrediction> GetOrCreatePredictionEngine<TData, TPrediction>(string modelPath)
        where TData : class
        where TPrediction : class, new();

    void InvalidateCache(string modelPath);

    void InvalidateAll();
}

public class ModelCache(MLContext mlContext, ILogger<ModelCache> logger) : IModelCache
{
    private readonly ConcurrentDictionary<string, Lazy<object>> _cache = new();

    public PredictionEngine<TData, TPrediction> GetOrCreatePredictionEngine<TData, TPrediction>(string modelPath)
        where TData : class
        where TPrediction : class, new()
    {
        var lazyEngine = _cache.GetOrAdd(modelPath, path =>
            new Lazy<object>(() => CreatePredictionEngine<TData, TPrediction>(path)));

        return (PredictionEngine<TData, TPrediction>)lazyEngine.Value;
    }

    public void InvalidateCache(string modelPath)
    {
        if (_cache.TryRemove(modelPath, out var lazyEngine))
        {
            if (lazyEngine.IsValueCreated && lazyEngine.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }

            LoggerMessages.LogModelCacheInvalidated(logger, modelPath);
        }
    }

    public void InvalidateAll()
    {
        foreach (var lazyEngine in _cache.Values)
        {
            if (lazyEngine.IsValueCreated && lazyEngine.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _cache.Clear();
        LoggerMessages.LogModelCacheCleared(logger);
    }

    private PredictionEngine<TData, TPrediction> CreatePredictionEngine<TData, TPrediction>(string modelPath)
        where TData : class
        where TPrediction : class, new()
    {
        LoggerMessages.LogLoadingModel(logger, modelPath);

        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"Model file not found at path: {modelPath}", modelPath);
        }

        var model = mlContext.Model.Load(modelPath, out _);
        var predictionEngine = mlContext.Model.CreatePredictionEngine<TData, TPrediction>(model);

        LoggerMessages.LogModelLoadedSuccessfully(logger, modelPath);

        return predictionEngine;
    }
}
