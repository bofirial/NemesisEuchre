using Microsoft.Extensions.DependencyInjection;

using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.MachineLearning.Bots.DependencyInjection;

public static class MachineLearningBotsServiceCollectionExtensions
{
    public static void AddNemesisEuchreMachineLearningBots(this IServiceCollection services)
    {
        services.AddScoped<IPlayerActor, Gen1Bot>();
        services.AddScoped<IPlayerActor, Gen1TrainerBot>();
    }
}
