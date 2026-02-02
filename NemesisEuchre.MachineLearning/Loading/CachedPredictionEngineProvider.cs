using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

using NemesisEuchre.Foundation;
using NemesisEuchre.MachineLearning.Options;

namespace NemesisEuchre.MachineLearning.Loading;

public interface IPredictionEngineProvider
{
    PredictionEngine<TData, TPrediction>? TryGetEngine<TData, TPrediction>(
        string decisionType,
        int generation = 1)
        where TData : class
        where TPrediction : class, new();
}

public class CachedPredictionEngineProvider(
    IModelLoader modelLoader,
    IOptions<MachineLearningOptions> options,
    ILogger<CachedPredictionEngineProvider> logger) : IPredictionEngineProvider
{
    private readonly IModelLoader _modelLoader = modelLoader ?? throw new ArgumentNullException(nameof(modelLoader));
    private readonly string _modelsDirectory = options.Value.ModelOutputPath;
    private readonly ILogger<CachedPredictionEngineProvider> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Dictionary<string, object?> _cache = [];
    private readonly Lock _lock = new();

    public PredictionEngine<TData, TPrediction>? TryGetEngine<TData, TPrediction>(
        string decisionType,
        int generation = 1)
        where TData : class
        where TPrediction : class, new()
    {
        var cacheKey = $"{decisionType}_Gen{generation}_{typeof(TData).Name}_{typeof(TPrediction).Name}";

        lock (_lock)
        {
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                return cached as PredictionEngine<TData, TPrediction>;
            }

            var engine = TryLoadModel<TData, TPrediction>(decisionType, generation);
            _cache[cacheKey] = engine;
            return engine;
        }
    }

    private PredictionEngine<TData, TPrediction>? TryLoadModel<TData, TPrediction>(
        string decisionType,
        int generation)
        where TData : class
        where TPrediction : class, new()
    {
        try
        {
            return _modelLoader.LoadModel<TData, TPrediction>(
                _modelsDirectory,
                generation,
                decisionType,
                version: null);
        }
        catch (FileNotFoundException ex)
        {
            LoggerMessages.LogModelNotFound(_logger, decisionType, ex);
            return null;
        }
        catch (Exception ex)
        {
            LoggerMessages.LogModelLoadFailed(_logger, decisionType, ex);
            return null;
        }
    }
}
