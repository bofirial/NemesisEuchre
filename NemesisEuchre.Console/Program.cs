using DotMake.CommandLine;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Commands;
using NemesisEuchre.Console.DependencyInjection;
using NemesisEuchre.Console.Logging;
using NemesisEuchre.DataAccess.DependencyInjection;
using NemesisEuchre.GameEngine.DependencyInjection;
using NemesisEuchre.MachineLearning.Bots.DependencyInjection;
using NemesisEuchre.MachineLearning.DependencyInjection;

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

            services.AddNemesisEuchreConsole(config);
            services.AddNemesisEuchreGameEngine();
            services.AddNemesisEuchreDataAccess(config);
            services.AddNemesisEuchreMachineLearning(config);
            services.AddNemesisEuchreMachineLearningBots();
        });

        System.Console.OutputEncoding = System.Text.Encoding.UTF8;

        return Cli.RunAsync<DefaultCommand>(args, new CliSettings { EnableDefaultExceptionHandler = true });
    }
}
