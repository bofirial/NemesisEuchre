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
        services.AddScoped<ISingleGameRunner, SingleGameRunner>();
        services.AddScoped<IParallelismCoordinator, ParallelismCoordinator>();
        services.AddScoped<ISubBatchStrategy, SubBatchStrategy>();
        services.AddScoped<IBatchExecutionFacade, BatchExecutionFacade>();
        services.AddScoped<IGameToTrainingDataConverter, GameToTrainingDataConverter>();
        services.AddScoped<ITrainingDataBuffer, TrainingDataBuffer>();
        services.AddScoped<IIdvChunkMerger, IdvChunkMerger>();
        services.AddScoped<IIdvMetadataService, IdvMetadataService>();
        services.AddScoped<ITrainingDataAccumulator, TrainingDataAccumulator>();
        services.AddScoped<IPersistenceCoordinator, BatchPersistenceCoordinator>();
        services.AddScoped<IBatchGameOrchestrator, BatchGameOrchestrator>();

        services.AddScoped<IModelTrainingOrchestrator, ModelTrainingOrchestrator>();
        services.AddScoped<ITrainerFactory, TrainerFactory>();
        services.AddScoped<ITrainingProgressCoordinator, TrainingProgressCoordinator>();
        services.AddScoped<ITrainingResultsRenderer, TrainingResultsRenderer>();
        services.AddScoped<ITrainerExecutor, CallTrumpRegressionTrainerExecutor>();
        services.AddScoped<ITrainerExecutor, DiscardCardRegressionTrainerExecutor>();
        services.AddScoped<ITrainerExecutor, PlayCardRegressionTrainerExecutor>();

        services.AddScoped<IModelBehavioralTestRunner, ModelBehavioralTestRunner>();
        services.AddScoped<ITestResultsRenderer, TestResultsRenderer>();

        services.AddScoped<IModelBehavioralTest, FiveTrumpPlusOneNonTrumpShouldDiscardNonTrump>();
        services.AddScoped<IModelBehavioralTest, ForcedCallShouldChooseBestTrump>();
        services.AddScoped<IModelBehavioralTest, NoTrumpInHandShouldPass>();
        services.AddScoped<IModelBehavioralTest, PerfectHandShouldGoAlone>();
        services.AddScoped<IModelBehavioralTest, StrongHandWithRightBowerUpShouldScoreHigherWithTeamDealer>();
        services.AddScoped<IModelBehavioralTest, TopThreeTrumpCardsInHandShouldNotPass>();

        services.AddScoped<IModelBehavioralTest, OneTrumpCardShouldNotDiscardTrump>();
        services.AddScoped<IModelBehavioralTest, LoneSuitShouldBeDiscardedToReduceToThreeSuits>();
        services.AddScoped<IModelBehavioralTest, LoneSuitShouldBeDiscardedToReduceToTwoSuits>();
        services.AddScoped<IModelBehavioralTest, FiveTrumpInHandShouldNotPass>();

        services.AddScoped<IModelBehavioralTest, PartnerWinningTrickShouldNotPlayTrump>();
        services.AddScoped<IModelBehavioralTest, OpponentWinningTrickShouldPlayTrump>();
        services.AddScoped<IModelBehavioralTest, OpponentWinningTrickShouldPlayLowestTrump>();
        services.AddScoped<IModelBehavioralTest, OpponentVoidInSuitShouldLeadTheOtherAce>();

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
