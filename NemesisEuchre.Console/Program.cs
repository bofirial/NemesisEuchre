using DotMake.CommandLine;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Commands;
using NemesisEuchre.Console.Logging;
using NemesisEuchre.Console.Services;
using NemesisEuchre.Console.Services.BehavioralTests;
using NemesisEuchre.Console.Services.BehavioralTests.Scenarios.CallTrump;
using NemesisEuchre.Console.Services.BehavioralTests.Scenarios.Discard;
using NemesisEuchre.Console.Services.BehavioralTests.Scenarios.PlayCard;
using NemesisEuchre.Console.Services.Orchestration;
using NemesisEuchre.Console.Services.TrainerExecutors;
using NemesisEuchre.DataAccess.DependencyInjection;
using NemesisEuchre.DataAccess.Options;
using NemesisEuchre.GameEngine.DependencyInjection;
using NemesisEuchre.GameEngine.Options;
using NemesisEuchre.MachineLearning.Bots.DependencyInjection;
using NemesisEuchre.MachineLearning.DependencyInjection;

using Spectre.Console;

namespace NemesisEuchre.Console;

public static class Program
{
    private static Task<int> Main(string[] args)
    {
        Cli.Ext.ConfigureServices(services =>
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            services.AddSingleton<IConfiguration>(config);

            services.AddLogging(builder => builder
                .AddConfiguration(config.GetSection("Logging"))
                .AddConsole()
                .AddFile(Path.Combine("logs", $"nemesiseuchre-{DateTime.Now:yyyyMMdd-HHmmss}.log")));

            services.AddScoped(_ => AnsiConsole.Console);

            services.AddScoped<IApplicationBanner, ApplicationBanner>();
            services.AddScoped<IDecisionRenderer, DecisionRenderer>();
            services.AddScoped<IGameResultsRenderer, GameResultsRenderer>();
            services.AddScoped<ISingleGameRunner, SingleGameRunner>();
            services.AddScoped<IParallelismCoordinator, ParallelismCoordinator>();
            services.AddScoped<ISubBatchStrategy, SubBatchStrategy>();
            services.AddScoped<IGameToTrainingDataConverter, GameToTrainingDataConverter>();
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
            services.AddScoped<IModelBehavioralTest, OneTrumpCardShouldNotDiscardTrump>();
            services.AddScoped<IModelBehavioralTest, LoneSuitShouldBeDiscardedToReduceToThreeSuits>();
            services.AddScoped<IModelBehavioralTest, LoneSuitShouldBeDiscardedToReduceToTwoSuits>();
            services.AddScoped<IModelBehavioralTest, FiveTrumpInHandShouldNotPass>();
            services.AddScoped<IModelBehavioralTest, NoTrumpInHandShouldPass>();
            services.AddScoped<IModelBehavioralTest, TopThreeTrumpCardsInHandShouldNotPass>();
            services.AddScoped<IModelBehavioralTest, PerfectHandShouldGoAlone>();
            services.AddScoped<IModelBehavioralTest, ForcedCallShouldChooseBestTrump>();
            services.AddScoped<IModelBehavioralTest, StrongHandWithRightBowerUpShouldScoreHigherWithTeamDealer>();
            services.AddScoped<IModelBehavioralTest, LeadWithRightBower>();
            services.AddScoped<IModelBehavioralTest, DontTrumpOverPartner>();

            services.AddNemesisEuchreGameEngine();
            services.Configure<GameOptions>(_ => { });
            services.AddOptions<GameExecutionOptions>()
                .Bind(config.GetSection("GameExecution"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddOptions<PersistenceOptions>()
                .Bind(config.GetSection("Persistence"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddNemesisEuchreDataAccess(config);
            services.AddNemesisEuchreMachineLearning(config);
            services.AddNemesisEuchreMachineLearningBots();
        });

        System.Console.OutputEncoding = System.Text.Encoding.UTF8;

        return Cli.RunAsync<DefaultCommand>(args, new CliSettings { EnableDefaultExceptionHandler = true });
    }
}
