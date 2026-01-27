using DotMake.CommandLine;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.Commands;
using NemesisEuchre.Console.Services;
using NemesisEuchre.DataAccess.DependencyInjection;
using NemesisEuchre.GameEngine.DependencyInjection;
using NemesisEuchre.GameEngine.Models;

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

            services.AddNemesisEuchreGameEngine();
            services.Configure<GameOptions>(_ => { });
            services.AddNemesisEuchreDataAccess(config);
        });

        return Cli.RunAsync<DefaultCommand>(args, new CliSettings { EnableDefaultExceptionHandler = true });
    }
}
