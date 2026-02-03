using Microsoft.Extensions.DependencyInjection;

using NemesisEuchre.GameEngine.Handlers;
using NemesisEuchre.GameEngine.Mappers;
using NemesisEuchre.GameEngine.PlayerBots;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Services;
using NemesisEuchre.GameEngine.Utilities;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.GameEngine.DependencyInjection;

public static class GameEngineServiceCollectionExtensions
{
    public static void AddNemesisEuchreGameEngine(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>();

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
        serviceCollection.AddScoped<ITrickPlayingOrchestrator, TrickPlayingOrchestrator>();

        serviceCollection.AddScoped<ITrickWinnerCalculator, TrickWinnerCalculator>();
        serviceCollection.AddScoped<IDealResultCalculator, DealResultCalculator>();
        serviceCollection.AddScoped<IGameWinnerCalculator, GameWinnerCalculator>();

        serviceCollection.AddScoped<ITrumpSelectionValidator, TrumpSelectionValidator>();
        serviceCollection.AddScoped<ITrickPlayingValidator, TrickPlayingValidator>();
        serviceCollection.AddScoped<IDealValidator, DealValidator>();
        serviceCollection.AddScoped<ITrickValidator, TrickValidator>();
        serviceCollection.AddScoped<IDealResultValidator, DealResultValidator>();

        serviceCollection.AddScoped<ICallTrumpDecisionMapper, CallTrumpDecisionMapper>();
        serviceCollection.AddScoped<IGoingAloneHandler, GoingAloneHandler>();
        serviceCollection.AddScoped<IDealerDiscardHandler, DealerDiscardHandler>();
        serviceCollection.AddScoped<IPlayerContextBuilder, PlayerContextBuilder>();
        serviceCollection.AddScoped<IPlayerActorResolver, PlayerActorResolver>();
        serviceCollection.AddScoped<IDecisionRecorder, DecisionRecorder>();
    }
}
