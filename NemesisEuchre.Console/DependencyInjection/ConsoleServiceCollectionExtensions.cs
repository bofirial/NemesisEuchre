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

        services.AddScoped<ICallTrumpBehavioralTestRunner, CallTrumpBehavioralTestRunner>();
        services.AddScoped<ICallTrumpBehavioralTestRunner, AdvancedCallTrumpBehavioralTestRunner>();
        services.AddScoped<IDiscardCardBehavioralTestRunner, DiscardCardBehavioralTestRunner>();
        services.AddScoped<IDiscardCardBehavioralTestRunner, AdvancedDiscardCardBehavioralTestRunner>();
        services.AddScoped<IPlayCardBehavioralTestRunner, PlayCardBehavioralTestRunner>();
        services.AddScoped<IPlayCardBehavioralTestRunner, SimplePlayCardBehavioralTestRunner>();
        services.AddScoped<IPlayCardBehavioralTestRunner, AdvancedPlayCardBehavioralTestRunner>();

        services.AddScoped<ICallTrumpBehavioralTest, NoTrumpInHandShouldPass>();
        services.AddScoped<ICallTrumpBehavioralTest, FiveTrumpInHandShouldNotPass>();
        services.AddScoped<ICallTrumpBehavioralTest, TopThreeTrumpCardsInHandShouldNotPass>();
        services.AddScoped<ICallTrumpBehavioralTest, PerfectHandShouldGoAlone>();
        services.AddScoped<ICallTrumpBehavioralTest, ForcedCallShouldChooseBestTrump>();
        services.AddScoped<ICallTrumpBehavioralTest, StrongHandWithRightBowerUpShouldScoreHigherWithTeamDealer>();

        services.AddScoped<IDiscardCardBehavioralTest, FiveTrumpPlusOneNonTrumpShouldDiscardNonTrump>();
        services.AddScoped<IDiscardCardBehavioralTest, OneTrumpCardShouldNotDiscardTrump>();
        services.AddScoped<IDiscardCardBehavioralTest, LoneSuitShouldBeDiscardedToReduceToThreeSuits>();
        services.AddScoped<IDiscardCardBehavioralTest, LoneSuitShouldBeDiscardedToReduceToTwoSuits>();

        services.AddScoped<IPlayCardBehavioralTest, PartnerWinningTrickShouldNotPlayTrump>();
        services.AddScoped<IPlayCardBehavioralTest, OpponentWinningTrickShouldPlayTrump>();
        services.AddScoped<IPlayCardBehavioralTest, OpponentWinningTrickShouldPlayLowestTrump>();
        services.AddScoped<IPlayCardBehavioralTest, OpponentVoidInSuitShouldLeadTheOtherAce>();

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
