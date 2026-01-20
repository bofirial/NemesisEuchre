using System.CommandLine;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NemesisEuchre.Console.CommandActions;

namespace NemesisEuchre.Console;

public static class Program
{
    private static Task<int> Main(string[] args)
    {
        var serviceProvider = BuildServiceProvider(args);

        var rootCommand = BuildRootCommand(serviceProvider);

        return rootCommand.Parse(args).InvokeAsync();
    }

    private static RootCommand BuildRootCommand(ServiceProvider serviceProvider)
    {
        var rootCommand = new RootCommand("Nemesis Euchre")
        {
            Action = serviceProvider.GetService<DefaultCommandAction>(),
        };

        var versionOption = rootCommand.Options.FirstOrDefault(o => o is VersionOption);

        versionOption!.Action = serviceProvider.GetService<VersionCommandAction>();

        return rootCommand;
    }

    private static ServiceProvider BuildServiceProvider(string[] args)
    {
        var services = new ServiceCollection();

        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        services.AddLogging(builder => builder.AddConfiguration(config));

        services.AddSingleton<IConfiguration>(config);

        services.AddScoped<DefaultCommandAction>();
        services.AddScoped<VersionCommandAction>();

        return services.BuildServiceProvider();
    }
}
