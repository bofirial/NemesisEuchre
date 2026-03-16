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
        services.AddSingleton<IIdvFileService, IdvFileService>();
        services.AddScoped<IDataSplitter, DataSplitter>();
        services.AddSingleton<IModelCache, ModelCache>();
        services.AddSingleton<IModelLoader, ModelLoader>();
        services.AddSingleton<IPredictionEngineProvider, CachedPredictionEngineProvider>();
        services.AddScoped<IModelPersistenceService, ModelPersistenceService>();

        services.AddScoped<IModelTrainer<CallTrumpTrainingData>, CallTrumpRegressionModelTrainer>();
        services.AddScoped<IModelTrainer<DiscardCardTrainingData>, DiscardCardRegressionModelTrainer>();
        services.AddScoped<IModelTrainer<PlayCardTrainingData>, PlayCardRegressionModelTrainer>();
        services.AddScoped<IModelTrainer<SimplePlayCardTrainingData>, SimplePlayCardRegressionModelTrainer>();
        services.AddScoped<IModelTrainer<AdvancedPlayCardTrainingData>, AdvancedPlayCardRegressionModelTrainer>();
        services.AddScoped<IModelTrainer<AdvancedCallTrumpTrainingData>, AdvancedCallTrumpRegressionModelTrainer>();
        services.AddScoped<IModelTrainer<AdvancedDiscardCardTrainingData>, AdvancedDiscardCardRegressionModelTrainer>();

        services.AddSingleton<PlayCardFeatureBuilder>();
        services.AddSingleton<CallTrumpFeatureBuilder>();
        services.AddSingleton<DiscardCardFeatureBuilder>();
        services.AddSingleton<IFeatureEngineer<CallTrumpDecisionEntity, AllCallTrumpTrainingData>, CallTrumpFeatureEngineer>();
        services.AddSingleton<IFeatureEngineer<DiscardCardDecisionEntity, AllDiscardCardTrainingData>, DiscardCardFeatureEngineer>();
        services.AddSingleton<IFeatureEngineer<PlayCardDecisionEntity, AllPlayCardTrainingData>, PlayCardFeatureEngineer>();

        services.AddSingleton<ICallTrumpInferenceFeatureBuilder, CallTrumpInferenceFeatureBuilder>();
        services.AddSingleton<IDiscardCardInferenceFeatureBuilder, DiscardCardInferenceFeatureBuilder>();
        services.AddSingleton<IPlayCardInferenceFeatureBuilder, PlayCardInferenceFeatureBuilder>();
        services.AddSingleton<ISimplePlayCardInferenceFeatureBuilder, SimplePlayCardInferenceFeatureBuilder>();
        services.AddSingleton<IAdvancedPlayCardInferenceFeatureBuilder, AdvancedPlayCardInferenceFeatureBuilder>();
        services.AddSingleton<IAdvancedCallTrumpInferenceFeatureBuilder, AdvancedCallTrumpInferenceFeatureBuilder>();
        services.AddSingleton<IAdvancedDiscardCardInferenceFeatureBuilder, AdvancedDiscardCardInferenceFeatureBuilder>();

        services.AddOptions<MachineLearningOptions>()
            .Bind(configuration.GetSection("MachineLearning"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
