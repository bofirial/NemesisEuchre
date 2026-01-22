using Microsoft.Extensions.DependencyInjection;

namespace NemesisEuchre.GameEngine.DependencyInjection;

public static class GameEngineServiceCollectionExtensions
{
    public static void AddNemesisEuchreGameEngine(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IGameFactory, GameFactory>();
        serviceCollection.AddScoped<IDealFactory, DealFactory>();
        serviceCollection.AddScoped<ICardShuffler, CardShuffler>();
    }
}
