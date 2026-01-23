using Microsoft.Extensions.DependencyInjection;

using NemesisEuchre.GameEngine.PlayerBots;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.DependencyInjection;

public static class GameEngineServiceCollectionExtensions
{
    public static void AddNemesisEuchreGameEngine(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IGameFactory, GameFactory>();
        serviceCollection.AddScoped<IDealFactory, DealFactory>();
        serviceCollection.AddScoped<IGameOrchestrator, GameOrchestrator>();
        serviceCollection.AddScoped<IGameScoreUpdater, GameScoreUpdater>();
        serviceCollection.AddScoped<ICardShuffler, CardShuffler>();

        serviceCollection.AddScoped<IPlayerActor, ChaosBot>();
        serviceCollection.AddScoped<IPlayerActor, ChadBot>();
        serviceCollection.AddScoped<IPlayerActor, BetaBot>();

        serviceCollection.AddScoped<IDealOrchestrator, DealOrchestrator>();
        serviceCollection.AddScoped<ITrumpSelectionOrchestrator, TrumpSelectionOrchestrator>();
    }
}
