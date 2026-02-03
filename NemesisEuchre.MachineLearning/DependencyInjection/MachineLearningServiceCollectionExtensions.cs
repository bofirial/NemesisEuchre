using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.MachineLearning.Caching;
using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Loading;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Services;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.MachineLearning.DependencyInjection;

public static class MachineLearningServiceCollectionExtensions
{
    public static IServiceCollection AddNemesisEuchreMachineLearning(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<MLContext>();
        services.AddScoped<IDataSplitter, DataSplitter>();
        services.AddSingleton<IModelCache, ModelCache>();
        services.AddSingleton<IModelVersionManager, ModelVersionManager>();
        services.AddSingleton<IModelLoader, ModelLoader>();
        services.AddSingleton<IPredictionEngineProvider, CachedPredictionEngineProvider>();
        services.AddScoped<IModelPersistenceService, ModelPersistenceService>();

        services.AddScoped<ITrainingDataLoader<CallTrumpTrainingData>, CallTrumpTrainingDataLoader>();
        services.AddScoped<ITrainingDataLoader<DiscardCardTrainingData>, DiscardCardTrainingDataLoader>();
        services.AddScoped<ITrainingDataLoader<PlayCardTrainingData>, PlayCardTrainingDataLoader>();

        services.AddScoped<IModelTrainer<CallTrumpTrainingData>, CallTrumpRegressionModelTrainer>();
        services.AddScoped<IModelTrainer<DiscardCardTrainingData>, DiscardCardRegressionModelTrainer>();
        services.AddScoped<IModelTrainer<PlayCardTrainingData>, PlayCardRegressionModelTrainer>();

        services.AddSingleton<IFeatureEngineer<CallTrumpDecisionEntity, CallTrumpTrainingData>, CallTrumpFeatureEngineer>();
        services.AddSingleton<IFeatureEngineer<DiscardCardDecisionEntity, DiscardCardTrainingData>, DiscardCardFeatureEngineer>();
        services.AddSingleton<IFeatureEngineer<PlayCardDecisionEntity, PlayCardTrainingData>, PlayCardFeatureEngineer>();

        services.AddSingleton<ICallTrumpInferenceFeatureBuilder, CallTrumpInferenceFeatureBuilder>();
        services.AddSingleton<IDiscardCardInferenceFeatureBuilder, DiscardCardInferenceFeatureBuilder>();
        services.AddSingleton<IPlayCardInferenceFeatureBuilder, PlayCardInferenceFeatureBuilder>();

        services.AddOptions<MachineLearningOptions>()
            .Bind(configuration.GetSection("MachineLearning"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
