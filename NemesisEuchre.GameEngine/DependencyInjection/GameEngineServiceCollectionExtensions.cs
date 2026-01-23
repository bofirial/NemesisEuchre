using Microsoft.Extensions.DependencyInjection;

using NemesisEuchre.GameEngine.PlayerBots;

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

        serviceCollection.AddScoped<IPlayerBot, ChaosBot>();
        serviceCollection.AddScoped<IPlayerBot, ChadBot>();
        serviceCollection.AddScoped<IPlayerBot, BetaBot>();

        serviceCollection.AddScoped<IDealOrchestrator, DealOrchestrator>();
        serviceCollection.AddScoped<ITrumpSelectionOrchestrator, TrumpSelectionOrchestrator>();
    }
}
