using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.FeatureEngineering;
using NemesisEuchre.MachineLearning.Models;
using NemesisEuchre.MachineLearning.Options;
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
        services.AddScoped<ITrainingDataLoader, TrainingDataLoader>();
        services.AddScoped<IModelTrainer, ModelTrainer>();

        services.AddSingleton<IFeatureEngineer<CallTrumpDecisionEntity, CallTrumpTrainingData>, CallTrumpFeatureEngineer>();
        services.AddSingleton<IFeatureEngineer<DiscardCardDecisionEntity, DiscardCardTrainingData>, DiscardCardFeatureEngineer>();
        services.AddSingleton<IFeatureEngineer<PlayCardDecisionEntity, PlayCardTrainingData>, PlayCardFeatureEngineer>();

        services.AddOptions<MachineLearningOptions>()
            .Bind(configuration.GetSection("MachineLearning"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
