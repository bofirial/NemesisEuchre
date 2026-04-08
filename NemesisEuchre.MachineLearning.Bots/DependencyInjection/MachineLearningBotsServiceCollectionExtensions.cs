using Microsoft.Extensions.DependencyInjection;

using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.MachineLearning.Bots.Simulation;

namespace NemesisEuchre.MachineLearning.Bots.DependencyInjection;

public static class MachineLearningBotsServiceCollectionExtensions
{
    public static void AddNemesisEuchreMachineLearningBots(this IServiceCollection services)
    {
        services.AddScoped<IPlayerActorFactory, ModelBotFactory>();
        services.AddScoped<IPlayerActorFactory, ModelTrainerBotFactory>();
        services.AddScoped<IDealSimulator, DealSimulator>();
        services.AddScoped<IHiddenCardDistributor, HiddenCardDistributor>();
        services.AddScoped<IPlayerActorFactory, MonteCarloBotFactory>();
    }
}
