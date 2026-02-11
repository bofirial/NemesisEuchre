using Microsoft.Extensions.DependencyInjection;

using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.MachineLearning.Bots.DependencyInjection;

public static class MachineLearningBotsServiceCollectionExtensions
{
    public static void AddNemesisEuchreMachineLearningBots(this IServiceCollection services)
    {
        services.AddScoped<IPlayerActorFactory, ModelBotFactory>();
        services.AddScoped<IPlayerActorFactory, ModelTrainerBotFactory>();
    }
}
