using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NemesisEuchre.Console.Services;
using NemesisEuchre.Console.Services.BehavioralTests;
using NemesisEuchre.Console.Services.BehavioralTests.Scenarios.CallTrump;
using NemesisEuchre.Console.Services.BehavioralTests.Scenarios.Discard;
using NemesisEuchre.Console.Services.BehavioralTests.Scenarios.PlayCard;
using NemesisEuchre.Console.Services.Orchestration;
using NemesisEuchre.Console.Services.TrainerExecutors;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.GameEngine.Options;

using Spectre.Console;

namespace NemesisEuchre.Console.DependencyInjection;

public static class ConsoleServiceCollectionExtensions
{
    public static IServiceCollection AddNemesisEuchreConsole(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped(_ => AnsiConsole.Console);

        services.AddScoped<IApplicationBanner, ApplicationBanner>();
        services.AddScoped<ICardDisplayRenderer, CardDisplayRenderer>();
        services.AddScoped<IBatchProgressRenderer, BatchProgressRenderer>();
        services.AddScoped<ITrickTableRenderer, TrickTableRenderer>();
        services.AddScoped<IDecisionRenderer, DecisionRenderer>();
        services.AddScoped<IGameResultsRenderer, GameResultsRenderer>();
        services.AddScoped<IBatchResultsExporter, BatchResultsExporter>();
        services.AddScoped<ITestResultsExporter, TestResultsExporter>();
        services.AddScoped<ISingleGameRunner, SingleGameRunner>();
        services.AddScoped<IParallelismCoordinator, ParallelismCoordinator>();
        services.AddScoped<ISubBatchStrategy, SubBatchStrategy>();
        services.AddScoped<IBatchExecutionFacade, BatchExecutionFacade>();
        services.AddScoped<IGameToTrainingDataConverter, GameToTrainingDataConverter>();
        services.AddScoped<IIdvChunkMerger, IdvChunkMerger>();
        services.AddScoped<IIdvMetadataService, IdvMetadataService>();
        services.AddScoped<IIdvMergeService, IdvMergeService>();
        services.AddScoped<ITrainingDataAccumulatorFactory, TrainingDataAccumulatorFactory>();
        services.AddScoped<IPersistenceCoordinator, BatchPersistenceCoordinator>();
        services.AddScoped<IBatchGameOrchestrator, BatchGameOrchestrator>();

        services.AddScoped<IModelTrainingOrchestrator, ModelTrainingOrchestrator>();
        services.AddScoped<ITrainerFactory, TrainerFactory>();
        services.AddScoped<ITrainingProgressCoordinator, TrainingProgressCoordinator>();
        services.AddScoped<ITrainingResultsRenderer, TrainingResultsRenderer>();
        services.AddScoped<ITrainerExecutor, CallTrumpRegressionTrainerExecutor>();
        services.AddScoped<ITrainerExecutor, DiscardCardRegressionTrainerExecutor>();
        services.AddScoped<ITrainerExecutor, PlayCardRegressionTrainerExecutor>();
        services.AddScoped<ITrainerExecutor, SimplePlayCardRegressionTrainerExecutor>();
        services.AddScoped<ITrainerExecutor, AdvancedPlayCardRegressionTrainerExecutor>();
        services.AddScoped<ITrainerExecutor, AdvancedCallTrumpRegressionTrainerExecutor>();
        services.AddScoped<ITrainerExecutor, AdvancedDiscardCardRegressionTrainerExecutor>();

        services.AddScoped<IModelBehavioralTestRunner, ModelBehavioralTestRunner>();
        services.AddScoped<ITestResultsRenderer, TestResultsRenderer>();

        services.AddScoped<CallTrumpBehavioralTestRunner>();
        services.AddScoped<AdvancedCallTrumpBehavioralTestRunner>();
        services.AddScoped<DiscardCardBehavioralTestRunner>();
        services.AddScoped<AdvancedDiscardCardBehavioralTestRunner>();

        services.AddScoped<IModelBehavioralTest>(sp =>
            new NoTrumpInHandShouldPass(sp.GetRequiredService<CallTrumpBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new NoTrumpInHandShouldPass(sp.GetRequiredService<AdvancedCallTrumpBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new FiveTrumpInHandShouldNotPass(sp.GetRequiredService<CallTrumpBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new FiveTrumpInHandShouldNotPass(sp.GetRequiredService<AdvancedCallTrumpBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new TopThreeTrumpCardsInHandShouldNotPass(sp.GetRequiredService<CallTrumpBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new TopThreeTrumpCardsInHandShouldNotPass(sp.GetRequiredService<AdvancedCallTrumpBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new PerfectHandShouldGoAlone(sp.GetRequiredService<CallTrumpBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new PerfectHandShouldGoAlone(sp.GetRequiredService<AdvancedCallTrumpBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new ForcedCallShouldChooseBestTrump(sp.GetRequiredService<CallTrumpBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new ForcedCallShouldChooseBestTrump(sp.GetRequiredService<AdvancedCallTrumpBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new StrongHandWithRightBowerUpShouldScoreHigherWithTeamDealer(sp.GetRequiredService<CallTrumpBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new StrongHandWithRightBowerUpShouldScoreHigherWithTeamDealer(sp.GetRequiredService<AdvancedCallTrumpBehavioralTestRunner>()));

        services.AddScoped<IModelBehavioralTest>(sp =>
            new FiveTrumpPlusOneNonTrumpShouldDiscardNonTrump(sp.GetRequiredService<DiscardCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new FiveTrumpPlusOneNonTrumpShouldDiscardNonTrump(sp.GetRequiredService<AdvancedDiscardCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new OneTrumpCardShouldNotDiscardTrump(sp.GetRequiredService<DiscardCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new OneTrumpCardShouldNotDiscardTrump(sp.GetRequiredService<AdvancedDiscardCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new LoneSuitShouldBeDiscardedToReduceToThreeSuits(sp.GetRequiredService<DiscardCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new LoneSuitShouldBeDiscardedToReduceToThreeSuits(sp.GetRequiredService<AdvancedDiscardCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new LoneSuitShouldBeDiscardedToReduceToTwoSuits(sp.GetRequiredService<DiscardCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new LoneSuitShouldBeDiscardedToReduceToTwoSuits(sp.GetRequiredService<AdvancedDiscardCardBehavioralTestRunner>()));

        services.AddScoped<PlayCardBehavioralTestRunner>();
        services.AddScoped<SimplePlayCardBehavioralTestRunner>();
        services.AddScoped<AdvancedPlayCardBehavioralTestRunner>();

        services.AddScoped<IModelBehavioralTest>(sp =>
            new PartnerWinningTrickShouldNotPlayTrump(sp.GetRequiredService<PlayCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new OpponentWinningTrickShouldPlayTrump(sp.GetRequiredService<PlayCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new OpponentWinningTrickShouldPlayLowestTrump(sp.GetRequiredService<PlayCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new OpponentVoidInSuitShouldLeadTheOtherAce(sp.GetRequiredService<PlayCardBehavioralTestRunner>()));

        services.AddScoped<IModelBehavioralTest>(sp =>
            new PartnerWinningTrickShouldNotPlayTrump(sp.GetRequiredService<SimplePlayCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new OpponentWinningTrickShouldPlayTrump(sp.GetRequiredService<SimplePlayCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new OpponentWinningTrickShouldPlayLowestTrump(sp.GetRequiredService<SimplePlayCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new OpponentVoidInSuitShouldLeadTheOtherAce(sp.GetRequiredService<SimplePlayCardBehavioralTestRunner>()));

        services.AddScoped<IModelBehavioralTest>(sp =>
            new PartnerWinningTrickShouldNotPlayTrump(sp.GetRequiredService<AdvancedPlayCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new OpponentWinningTrickShouldPlayTrump(sp.GetRequiredService<AdvancedPlayCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new OpponentWinningTrickShouldPlayLowestTrump(sp.GetRequiredService<AdvancedPlayCardBehavioralTestRunner>()));
        services.AddScoped<IModelBehavioralTest>(sp =>
            new OpponentVoidInSuitShouldLeadTheOtherAce(sp.GetRequiredService<AdvancedPlayCardBehavioralTestRunner>()));

        services.Configure<GameOptions>(_ => { });
        services.AddOptions<GameExecutionOptions>()
            .Bind(configuration.GetSection("GameExecution"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<PersistenceOptions>()
            .Bind(configuration.GetSection("Persistence"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
