using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

using NemesisEuchre.Foundation;

namespace NemesisEuchre.MachineLearning.Caching;

public interface IModelCache
{
    PredictionEngine<TData, TPrediction> GetOrCreatePredictionEngine<TData, TPrediction>(string modelPath)
        where TData : class
        where TPrediction : class, new();

    PredictionEngine<TData, TPrediction> CreateNewPredictionEngine<TData, TPrediction>(string modelPath)
        where TData : class
        where TPrediction : class, new();

    void InvalidateCache(string modelPath);

    void InvalidateAll();
}

public class ModelCache(MLContext mlContext, ILogger<ModelCache> logger) : IModelCache
{
    private readonly ConcurrentDictionary<string, Lazy<object>> _engineCache = new();
    private readonly ConcurrentDictionary<string, Lazy<ITransformer>> _modelCache = new();
    private readonly Lock _mlContextLock = new();

    public PredictionEngine<TData, TPrediction> GetOrCreatePredictionEngine<TData, TPrediction>(string modelPath)
        where TData : class
        where TPrediction : class, new()
    {
        var lazyEngine = _engineCache.GetOrAdd(modelPath, path =>
            new Lazy<object>(() => BuildPredictionEngine<TData, TPrediction>(path)));

        return (PredictionEngine<TData, TPrediction>)lazyEngine.Value;
    }

    public PredictionEngine<TData, TPrediction> CreateNewPredictionEngine<TData, TPrediction>(string modelPath)
        where TData : class
        where TPrediction : class, new()
    {
        var model = GetOrLoadModel(modelPath);

        lock (_mlContextLock)
        {
            return mlContext.Model.CreatePredictionEngine<TData, TPrediction>(model);
        }
    }

    public void InvalidateCache(string modelPath)
    {
        if (_engineCache.TryRemove(modelPath, out var lazyEngine))
        {
            if (lazyEngine.IsValueCreated && lazyEngine.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }

            LoggerMessages.LogModelCacheInvalidated(logger, modelPath);
        }

        _modelCache.TryRemove(modelPath, out _);
    }

    public void InvalidateAll()
    {
        foreach (var lazyEngine in _engineCache.Values)
        {
            if (lazyEngine.IsValueCreated && lazyEngine.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _engineCache.Clear();
        _modelCache.Clear();
        LoggerMessages.LogModelCacheCleared(logger);
    }

    private ITransformer GetOrLoadModel(string modelPath)
    {
        var lazyModel = _modelCache.GetOrAdd(modelPath, path =>
            new Lazy<ITransformer>(() => LoadModel(path)));

        return lazyModel.Value;
    }

    private ITransformer LoadModel(string modelPath)
    {
        LoggerMessages.LogLoadingModel(logger, modelPath);

        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"Model file not found at path: {modelPath}", modelPath);
        }

        ITransformer model;
        lock (_mlContextLock)
        {
            model = mlContext.Model.Load(modelPath, out _);
        }

        LoggerMessages.LogModelLoadedSuccessfully(logger, modelPath);

        return model;
    }

    private PredictionEngine<TData, TPrediction> BuildPredictionEngine<TData, TPrediction>(string modelPath)
        where TData : class
        where TPrediction : class, new()
    {
        var model = GetOrLoadModel(modelPath);

        lock (_mlContextLock)
        {
            return mlContext.Model.CreatePredictionEngine<TData, TPrediction>(model);
        }
    }
}
