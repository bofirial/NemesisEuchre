using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NemesisEuchre.MachineLearning.DataAccess;
using NemesisEuchre.MachineLearning.Options;
using NemesisEuchre.MachineLearning.Trainers;

namespace NemesisEuchre.MachineLearning.DependencyInjection;

public static class MachineLearningServiceCollectionExtensions
{
    public static IServiceCollection AddNemesisEuchreMachineLearning(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ITrainingDataLoader, TrainingDataLoader>();
        services.AddScoped<IModelTrainer, ModelTrainer>();

        services.AddOptions<MachineLearningOptions>()
            .Bind(configuration.GetSection("MachineLearning"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
