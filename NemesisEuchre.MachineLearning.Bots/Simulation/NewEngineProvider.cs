using Microsoft.ML;

using NemesisEuchre.MachineLearning.Loading;

namespace NemesisEuchre.MachineLearning.Bots.Simulation;

internal sealed class NewEngineProvider(IPredictionEngineProvider inner) : IPredictionEngineProvider
{
    public PredictionEngine<TData, TPrediction>? TryGetEngine<TData, TPrediction>(
        string decisionType,
        string modelName)
        where TData : class
        where TPrediction : class, new()
    {
        return inner.TryCreateNewEngine<TData, TPrediction>(decisionType, modelName);
    }

    public PredictionEngine<TData, TPrediction>? TryCreateNewEngine<TData, TPrediction>(
        string decisionType,
        string modelName)
        where TData : class
        where TPrediction : class, new()
    {
        return inner.TryCreateNewEngine<TData, TPrediction>(decisionType, modelName);
    }
}
