using DotMake.CommandLine;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Commands;
using NemesisEuchre.Console.Services;
using NemesisEuchre.Console.Services.Orchestration;
using NemesisEuchre.Console.Services.Persistence;
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
                .AddConsole());

            services.AddScoped(_ => AnsiConsole.Console);

            services.AddScoped<IApplicationBanner, ApplicationBanner>();
            services.AddScoped<IGameResultsRenderer, GameResultsRenderer>();
            services.AddScoped<ISingleGameRunner, SingleGameRunner>();
            services.AddScoped<IParallelismCoordinator, ParallelismCoordinator>();
            services.AddScoped<ISubBatchStrategy, SubBatchStrategy>();
            services.AddScoped<IPersistenceCoordinator, BatchPersistenceCoordinator>();
            services.AddScoped<IBatchGameOrchestrator, BatchGameOrchestrator>();

            services.AddScoped<IModelTrainingOrchestrator, ModelTrainingOrchestrator>();
            services.AddScoped<ITrainerFactory, TrainerFactory>();
            services.AddScoped<ITrainingResultsRenderer, TrainingResultsRenderer>();
            services.AddScoped<ITrainerExecutor, CallTrumpRegressionTrainerExecutor>();
            services.AddScoped<ITrainerExecutor, DiscardCardRegressionTrainerExecutor>();
            services.AddScoped<ITrainerExecutor, PlayCardRegressionTrainerExecutor>();

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

        return Cli.RunAsync<DefaultCommand>(args, new CliSettings { EnableDefaultExceptionHandler = true });
    }
}
